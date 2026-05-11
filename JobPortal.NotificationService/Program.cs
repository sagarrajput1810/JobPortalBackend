using MassTransit;
using JobPortal.NotificationService.Consumers;
using JobPortal.NotificationService.Services;
using JobPortal.NotificationService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
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
            policy.WithOrigins(builder.Configuration["AllowedOrigins"] ?? "*")
                  .AllowAnyHeader()
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
            cfg.ConfigureEndpoints(context);
        });
    }
    else
    {
        x.UsingAzureServiceBus((context, cfg) =>
        {
            cfg.Host(builder.Configuration["ServiceBus:ConnectionString"]);
            
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

            cfg.ConfigureEndpoints(context);
        });
    }
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
