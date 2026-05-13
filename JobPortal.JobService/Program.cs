using JobPortal.JobService.Data;
using JobPortal.JobService.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var allowedOrigins = builder.Configuration["AllowedOrigins"];
var jwtKey = builder.Configuration["Jwt:Key"] 
    ?? throw new InvalidOperationException("Jwt:Key configuration is missing.");

// Add services to the container.
builder.Services.AddControllers();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            if (string.IsNullOrWhiteSpace(allowedOrigins) || allowedOrigins == "*")
            {
                policy.AllowAnyOrigin();
            }
            else
            {
                policy.WithOrigins(allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
            }

            policy.AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// Register DB Context
builder.Services.AddDbContext<JobDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

// Register Business Services
builder.Services.AddScoped<IJobService, JobService>();

builder.Services.AddMassTransit(x =>
{
    if (builder.Environment.IsDevelopment())
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
            cfg.Host(rabbitHost, "/", h => {
                h.Username(builder.Configuration["RabbitMq:User"] ?? "guest");
                h.Password(builder.Configuration["RabbitMq:Pass"] ?? "guest");
            });
            cfg.ConfigureEndpoints(context);
        });
    }
    else
    {
        var connString = builder.Configuration["ServiceBus:ConnectionString"];
        Console.WriteLine($"[JobService] Configuring Azure Service Bus. Connection String configured: {!string.IsNullOrWhiteSpace(connString)}");

        x.UsingAzureServiceBus((context, cfg) =>
        {
            cfg.Host(connString);
            cfg.DeployPublishTopology = false;
            cfg.ConfigureEndpoints(context);
        });
    }
});

// JWT Authentication Configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
    options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            )
        };
    });

builder.Services.AddAuthorization();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<JobDbContext>();
        dbContext.Database.Migrate();
        logger.LogInformation("Job database migrations applied.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to apply job database migrations.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");

// app.UseHttpsRedirection();

app.UseAuthentication(); // Ensure Authentication is before Authorization
app.UseAuthorization();

app.MapControllers();

app.Run();
