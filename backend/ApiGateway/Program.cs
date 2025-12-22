using Shared.Infrastructure.Middleware;
using Shared.Infrastructure.HealthChecks;
using Shared.Infrastructure.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add YARP reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add production infrastructure
builder.Services.AddGlobalExceptionHandler();
builder.Services.AddApiRateLimiting();
builder.Services.AddServiceHealthChecks("ApiGateway");

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Transaction Dispute Portal - API Gateway", 
        Version = "v1",
        Description = "API Gateway for the Transaction Dispute Portal microservices"
    });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseExceptionHandler();
app.UseCorrelationId();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

// Map health check endpoints
app.MapServiceHealthChecks();

// API info endpoint
app.MapGet("/api", () => Results.Ok(new 
{ 
    name = "Transaction Dispute Portal API",
    version = "1.0.0",
    services = new[]
    {
        new { name = "Auth Service", path = "/api/auth" },
        new { name = "Transaction Service", path = "/api/transactions" },
        new { name = "Dispute Service", path = "/api/disputes" }
    },
    healthChecks = new[]
    {
        new { name = "Liveness", path = "/health/live" },
        new { name = "Readiness", path = "/health/ready" },
        new { name = "Full", path = "/health" }
    }
}))
    .WithName("ApiInfo")
    .WithOpenApi();

app.MapReverseProxy();

app.Run();
