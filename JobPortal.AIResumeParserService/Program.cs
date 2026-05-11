using MassTransit;
using JobPortal.AIResumeParserService.Consumers;
using JobPortal.AIResumeParserService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

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

// Register HttpClient for API Calls
builder.Services.AddHttpClient();

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
            cfg.ConfigureEndpoints(context);
        });
    }
    else
    {
        x.UsingAzureServiceBus((context, cfg) =>
        {
            cfg.Host(builder.Configuration["ServiceBus:ConnectionString"]);
            
            cfg.ReceiveEndpoint("job-applied-event-ai", e =>
            {
                e.ConfigureConsumer<JobAppliedConsumer>(context);
            });

            cfg.ConfigureEndpoints(context);
        });
    }
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAngular");

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
