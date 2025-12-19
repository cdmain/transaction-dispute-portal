var builder = WebApplication.CreateBuilder(args);

// Add YARP reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithOpenApi();

// API info endpoint
app.MapGet("/api", () => Results.Ok(new 
{ 
    name = "Transaction Dispute Portal API",
    version = "1.0.0",
    services = new[]
    {
        new { name = "Transaction Service", path = "/api/transactions" },
        new { name = "Dispute Service", path = "/api/disputes" }
    }
}))
    .WithName("ApiInfo")
    .WithOpenApi();

app.MapReverseProxy();

app.Run();
