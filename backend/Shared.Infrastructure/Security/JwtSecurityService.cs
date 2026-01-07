using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Shared.Infrastructure.Security;

/// <summary>
/// Secure JWT token generation and validation
/// Implements OWASP 2025 JWT security recommendations
/// </summary>
public class JwtSecurityService
{
    private readonly JwtSecurityOptions _options;
    private readonly SigningCredentials _signingCredentials;
    private readonly TokenValidationParameters _validationParameters;

    public JwtSecurityService(JwtSecurityOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrEmpty(options.Secret) || options.Secret.Length < 32)
            throw new ArgumentException("JWT secret must be at least 32 characters", nameof(options));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret));
        _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = options.Issuer,
            ValidateAudience = true,
            ValidAudience = options.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1), // Allow 1 minute clock skew
            RequireExpirationTime = true,
            RequireSignedTokens = true
        };
    }

    /// <summary>
    /// Generates a secure JWT token with the specified claims
    /// </summary>
    public string GenerateToken(IEnumerable<Claim> claims, TimeSpan? expiry = null)
    {
        var tokenClaims = new List<Claim>(claims)
        {
            // Add standard security claims
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(tokenClaims),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Expires = DateTime.UtcNow.Add(expiry ?? _options.DefaultExpiry),
            NotBefore = DateTime.UtcNow.AddSeconds(-5), // Allow 5 seconds for clock skew
            SigningCredentials = _signingCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Validates a JWT token and returns the claims principal
    /// </summary>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, _validationParameters, out var validatedToken);

            // Additional validation
            if (validatedToken is not JwtSecurityToken jwtToken)
                return null;

            // Verify algorithm to prevent algorithm confusion attacks
            if (!jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
                return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Generates a refresh token
    /// </summary>
    public string GenerateRefreshToken()
    {
        return PasswordHasher.GenerateSecureToken(64);
    }

    /// <summary>
    /// Gets a specific claim from a token without full validation
    /// Useful for getting user info from expired tokens for refresh
    /// </summary>
    public string? GetClaimFromToken(string token, string claimType)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// JWT security configuration options
/// </summary>
public class JwtSecurityOptions
{
    /// <summary>
    /// Secret key for signing tokens. Must be at least 32 characters.
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Token issuer
    /// </summary>
    public string Issuer { get; set; } = "DisputePortal";

    /// <summary>
    /// Token audience
    /// </summary>
    public string Audience { get; set; } = "DisputePortalUsers";

    /// <summary>
    /// Default token expiry time
    /// </summary>
    public TimeSpan DefaultExpiry { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Refresh token expiry time
    /// </summary>
    public TimeSpan RefreshTokenExpiry { get; set; } = TimeSpan.FromDays(7);
}
