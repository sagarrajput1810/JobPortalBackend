using MassTransit;
using JobPortal.NotificationService.Consumers;
using JobPortal.NotificationService.Services;
using JobPortal.NotificationService.Data;
using JobPortal.Shared.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var allowedOrigins = builder.Configuration["AllowedOrigins"];
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("Jwt:Key configuration is missing.");
}

// Add services to the container.
builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

// Token validation mechanism
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
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

// Register Email Service
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddMassTransit(x =>
{
    // Adding all Consumers
    x.AddConsumer<UserRegisteredConsumer>();
    x.AddConsumer<JobAppliedConsumer>();
    x.AddConsumer<ApplicationStatusUpdatedConsumer>();

    if (builder.Environment.IsDevelopment())
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
            cfg.Host(rabbitHost, "/", h => {
                h.Username(builder.Configuration["RabbitMq:User"] ?? "guest");
                h.Password(builder.Configuration["RabbitMq:Pass"] ?? "guest");
            });

            cfg.ReceiveEndpoint("user-registered-event", e =>
            {
                e.ConfigureConsumer<UserRegisteredConsumer>(context);
            });

            cfg.ReceiveEndpoint("job-applied-event-notification", e =>
            {
                e.ConfigureConsumer<JobAppliedConsumer>(context);
            });

            cfg.ReceiveEndpoint("application-status-updated-event-notification", e =>
            {
                e.ConfigureConsumer<ApplicationStatusUpdatedConsumer>(context);
            });
        });
    }
    else
    {
        var connString = builder.Configuration["ServiceBus:ConnectionString"];
        Console.WriteLine($"[NotificationService] Configuring Azure Service Bus. Connection String length: {connString?.Length ?? 0}");
        
        x.UsingAzureServiceBus((context, cfg) =>
        {
            Console.WriteLine("[NotificationService] Inside UsingAzureServiceBus configuration...");
            cfg.Host(connString);
            
            // For Basic Tier: Disable topic creation
            cfg.DeployPublishTopology = false;

            cfg.ReceiveEndpoint("user-registered-event", e =>
            {
                e.ConfigureConsumeTopology = false;
                e.ConfigureConsumer<UserRegisteredConsumer>(context);
            });

            cfg.ReceiveEndpoint("job-applied-event-notification", e =>
            {
                e.ConfigureConsumeTopology = false;
                e.ConfigureConsumer<JobAppliedConsumer>(context);
            });

            cfg.ReceiveEndpoint("application-status-updated-event-notification", e =>
            {
                e.ConfigureConsumeTopology = false;
                e.ConfigureConsumer<ApplicationStatusUpdatedConsumer>(context);
            });

            // Removed cfg.ConfigureEndpoints(context) to prevent automatic topic/queue creation conflicts on Basic Tier
        });
    }
});

var app = builder.Build();

Console.WriteLine("[NotificationService] Application Build complete. Starting app...");

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        dbContext.Database.Migrate();
        logger.LogInformation("Notification database migrations applied.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to apply notification database migrations.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");

// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
