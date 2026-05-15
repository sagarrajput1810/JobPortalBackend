using JobPortal.ApplicationService.Data;
using JobPortal.ApplicationService.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to allow up to 50MB for file uploads
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 52428800; // 50 MB
});

var allowedOrigins = (builder.Configuration["AllowedOrigins"] ?? "http://localhost:4200")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("Jwt:Key configuration is missing.");
}

// Add services to the container.
builder.Services.AddControllers();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
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

// Register File Service based on configuration
var azureStorageConnString = builder.Configuration["AzureStorage:ConnectionString"];
if (!string.IsNullOrWhiteSpace(azureStorageConnString))
{
    builder.Services.AddScoped<IFileService, AzureBlobStorageService>();
    Console.WriteLine("[ApplicationService] Using AzureBlobStorageService for file uploads.");
}
else
{
    builder.Services.AddScoped<IFileService>(sp => 
    {
        var env = sp.GetRequiredService<IWebHostEnvironment>();
        var logger = sp.GetRequiredService<ILogger<LocalFileService>>();
        return new LocalFileService(env.ContentRootPath, logger);
    });
    Console.WriteLine("[ApplicationService] Using LocalFileService for file uploads because AzureStorage:ConnectionString is missing.");
}

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
        var connString = (builder.Configuration["ServiceBus:ConnectionString"] ?? "").Trim().TrimEnd('/');
        Console.WriteLine($"[ApplicationService] Configuring Azure Service Bus. Connection String configured: {!string.IsNullOrWhiteSpace(connString)}");

        x.UsingAzureServiceBus((context, cfg) =>
        {
            cfg.Host(connString);
            
            // For Basic Tier: Disable topic creation
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
        await dbContext.Database.MigrateAsync();
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

// Ensure the resumes folder exists inside wwwroot/uploads
var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "uploads");
var resumesPath = Path.Combine(uploadsPath, "resumes");
if (!Directory.Exists(resumesPath))
{
    Directory.CreateDirectory(resumesPath);
}

// Enable Static Files so uploads can be accessed via URL
app.UseStaticFiles(); // Default for wwwroot

// Explicitly map /uploads to physical path to be 100% sure
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads",
    OnPrepareResponse = ctx =>
    {
        // Allow CORS for direct file access
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, OPTIONS");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "*");
        
        // Ensure browser treats it as a PDF and allows inline viewing
        if (ctx.File.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            ctx.Context.Response.Headers.ContentType = "application/pdf";
            ctx.Context.Response.Headers.ContentDisposition = "inline";
        }
    }
});

Console.WriteLine($"[ApplicationService] Static files configured. Serving /uploads from: {uploadsPath}");
Console.WriteLine($"[ApplicationService] WebRootPath: {app.Environment.WebRootPath ?? "NULL"}");
Console.WriteLine($"[ApplicationService] ContentRootPath: {app.Environment.ContentRootPath}");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
