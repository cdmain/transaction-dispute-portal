using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TransactionService.Data;
using TransactionService.Services;
using Shared.Infrastructure.Middleware;
using Shared.Infrastructure.HealthChecks;
using Shared.Infrastructure.RateLimiting;
using Shared.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Transaction Service API", 
        Version = "v1",
        Description = "Microservice for managing customer transactions"
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure SQLite
builder.Services.AddDbContext<TransactionDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=transactions.db"));

// Configure Redis caching (optional - falls back to in-memory if not available)
var redisConnection = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "TransactionService:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

// Configure JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] 
    ?? throw new InvalidOperationException("JWT Secret is not configured. Set Jwt:Secret in configuration.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "DisputePortal";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "DisputePortalUsers";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Register services
builder.Services.AddScoped<ITransactionService, TransactionServiceImpl>();

// Add production infrastructure
builder.Services.AddGlobalExceptionHandler();
builder.Services.AddApiRateLimiting();
builder.Services.AddServiceHealthChecks<TransactionDbContext>("TransactionService");

// Configure Secure CORS
builder.Services.AddSecureCors(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseExceptionHandler();

// Security middleware - OWASP 2025 compliant
app.UseApiSecurityHeaders();
app.UseRequestValidation();

app.UseCorrelationId();
app.UseRateLimiter();

app.UseSwagger();
app.UseSwaggerUI();

app.UseSecureCors(app.Environment);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Map health check endpoints
app.MapServiceHealthChecks();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
    context.Database.EnsureCreated();
    DataSeeder.Seed(context);
}

app.Run();
