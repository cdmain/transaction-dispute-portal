using System.Text.RegularExpressions;
using System.Web;

namespace Shared.Infrastructure.Security;

/// <summary>
/// Input validation and sanitization utilities to prevent injection attacks
/// Implements OWASP 2025 recommendations for input handling
/// </summary>
public static class InputSanitizer
{
    private static readonly Regex SqlInjectionPattern = new(
        @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|EXECUTE|UNION|CAST|DECLARE|CURSOR|TRUNCATE|GRANT|REVOKE)\b)|(-{2})|(/\*)|(\*/)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex XssPattern = new(
        @"<script[^>]*>[\s\S]*?</script>|<[^>]+on\w+\s*=|javascript:|vbscript:|data:|expression\s*\(|url\s*\(",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex PathTraversalPattern = new(
        @"\.\./|\.\.\\|%2e%2e%2f|%2e%2e/|\.%2e/|%2e\./|%00|%0d|%0a",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex LdapInjectionPattern = new(
        @"[()\\*\x00]|[\x80-\xff]",
        RegexOptions.Compiled);

    private static readonly Regex CommandInjectionPattern = new(
        @"[;&|`$><]|\||&&|\|\|",
        RegexOptions.Compiled);

    /// <summary>
    /// Sanitizes a string by HTML encoding and removing dangerous patterns
    /// </summary>
    public static string Sanitize(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        // HTML encode to prevent XSS
        var sanitized = HttpUtility.HtmlEncode(input);

        // Remove null bytes
        sanitized = sanitized.Replace("\0", "");

        // Normalize line endings
        sanitized = sanitized.Replace("\r\n", "\n").Replace("\r", "\n");

        return sanitized;
    }

    /// <summary>
    /// Checks if input contains potential SQL injection patterns
    /// </summary>
    public static bool ContainsSqlInjection(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        return SqlInjectionPattern.IsMatch(input);
    }

    /// <summary>
    /// Checks if input contains potential XSS patterns
    /// </summary>
    public static bool ContainsXss(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        return XssPattern.IsMatch(input);
    }

    /// <summary>
    /// Checks if input contains path traversal attempts
    /// </summary>
    public static bool ContainsPathTraversal(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        return PathTraversalPattern.IsMatch(input);
    }

    /// <summary>
    /// Checks if input contains LDAP injection attempts
    /// </summary>
    public static bool ContainsLdapInjection(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        return LdapInjectionPattern.IsMatch(input);
    }

    /// <summary>
    /// Checks if input contains command injection attempts
    /// </summary>
    public static bool ContainsCommandInjection(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        return CommandInjectionPattern.IsMatch(input);
    }

    /// <summary>
    /// Validates that input is safe (no injection patterns detected)
    /// </summary>
    public static bool IsSafeInput(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return true;

        return !ContainsSqlInjection(input) && 
               !ContainsXss(input) && 
               !ContainsPathTraversal(input) &&
               !ContainsLdapInjection(input) &&
               !ContainsCommandInjection(input);
    }

    /// <summary>
    /// Validates and sanitizes input, throwing if dangerous patterns detected
    /// </summary>
    public static string ValidateAndSanitize(string? input, string parameterName)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        if (ContainsSqlInjection(input))
            throw new ArgumentException($"Potentially dangerous SQL content detected in {parameterName}");

        if (ContainsXss(input))
            throw new ArgumentException($"Potentially dangerous script content detected in {parameterName}");

        if (ContainsPathTraversal(input))
            throw new ArgumentException($"Invalid path content detected in {parameterName}");

        if (ContainsCommandInjection(input))
            throw new ArgumentException($"Potentially dangerous command content detected in {parameterName}");

        return Sanitize(input);
    }

    /// <summary>
    /// Validates email format without regex to prevent ReDoS
    /// </summary>
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // Simple validation without complex regex (ReDoS prevention)
        var atIndex = email.IndexOf('@');
        if (atIndex < 1 || atIndex == email.Length - 1)
            return false;

        var dotIndex = email.LastIndexOf('.');
        if (dotIndex < atIndex + 2 || dotIndex == email.Length - 1)
            return false;

        return email.Length <= 254 && !email.Contains(' ');
    }

    /// <summary>
    /// Truncates input to prevent buffer overflow attacks
    /// </summary>
    public static string TruncateInput(string? input, int maxLength)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return input.Length <= maxLength ? input : input[..maxLength];
    }
}
