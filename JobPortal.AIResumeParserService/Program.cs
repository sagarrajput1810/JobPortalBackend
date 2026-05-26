using MassTransit;
using JobPortal.AIResumeParserService.Consumers;
using JobPortal.AIResumeParserService.Services;

var builder = WebApplication.CreateBuilder(args);
var allowedOrigins = (builder.Configuration["AllowedOrigins"] ?? "http://localhost:4200")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

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

// Register HttpClient for API Calls and ignore internal SSL errors (for ACA networking)
builder.Services.AddHttpClient("GeminiClient")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    });

// Register Gemini Service
builder.Services.AddScoped<IGeminiService, GeminiService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<JobAppliedConsumer>();

    if (builder.Environment.IsDevelopment())
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
            cfg.Host(rabbitHost, "/", h => {
                h.Username(builder.Configuration["RabbitMq:User"] ?? "guest");
                h.Password(builder.Configuration["RabbitMq:Pass"] ?? "guest");
            });

            cfg.ReceiveEndpoint("job-applied-event-ai", e =>
            {
                e.ConfigureConsumer<JobAppliedConsumer>(context);
            });
        });
    }
    else
    {
        var connString = (builder.Configuration["ServiceBus:ConnectionString"] ?? "").Trim().TrimEnd('/');
        Console.WriteLine($"[AIResumeParserService] Configuring Azure Service Bus. Connection String configured: {!string.IsNullOrWhiteSpace(connString)}");

        x.UsingAzureServiceBus((context, cfg) =>
        {
            cfg.Host(connString);

            // For Basic Tier: Disable topic creation
            cfg.DeployPublishTopology = false;

            cfg.ReceiveEndpoint("job-applied-event-ai", e =>
            {
                e.ConfigureConsumeTopology = false;
                e.ConfigureConsumer<JobAppliedConsumer>(context);
            });
        });
    }
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAngular");

// app.UseHttpsRedirection();
app.MapControllers();

await app.RunAsync();
