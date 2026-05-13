using JobPortal.ApplicationService.Data;
using JobPortal.ApplicationService.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.FileProviders;

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

// DB Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

// Services
builder.Services.AddScoped<IApplicationService, ApplicationService>();
// Register LocalFileService
builder.Services.AddScoped<IFileService>(provider => 
    new LocalFileService(builder.Environment.ContentRootPath));

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
        Console.WriteLine($"[ApplicationService] Configuring Azure Service Bus. Connection String configured: {!string.IsNullOrWhiteSpace(connString)}");

        x.UsingAzureServiceBus((context, cfg) =>
        {
            cfg.Host(connString);
            cfg.DeployPublishTopology = false;
            cfg.ConfigureEndpoints(context);
        });
    }
});

// JWT
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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
        logger.LogInformation("Application database migrations applied.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to apply application database migrations.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");

// Enable Static Files so uploads can be accessed via URL
app.UseStaticFiles(); // For wwwroot

// Ensure the uploads folder is served even if not in wwwroot (optional but safer)
var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

// app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
