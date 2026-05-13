using JobPortal.SearchService.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using JobPortal.SearchService.Data;

var builder = WebApplication.CreateBuilder(args);
var allowedOrigins = builder.Configuration["AllowedOrigins"];

// Add services to the container.
builder.Services.AddControllers();

// Add DbContext
builder.Services.AddDbContext<SearchDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ISearchService, SearchService>();

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

builder.Services.AddMassTransit(x =>
{
    // Register the consumer
    x.AddConsumer<JobPortal.SearchService.Consumers.JobEventsConsumer>();

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
        Console.WriteLine($"[SearchService] Configuring Azure Service Bus. Connection String configured: {!string.IsNullOrWhiteSpace(connString)}");

        x.UsingAzureServiceBus((context, cfg) =>
        {
            cfg.Host(connString);

            // Azure Service Bus Basic tier does not support topics/subscriptions.
            cfg.DeployPublishTopology = false;
            
            cfg.ReceiveEndpoint("job-created-event-search", e =>
            {
                e.ConfigureConsumeTopology = false;
                e.ConfigureConsumer<JobPortal.SearchService.Consumers.JobEventsConsumer>(context);
            });

            cfg.ReceiveEndpoint("job-updated-event-search", e =>
            {
                e.ConfigureConsumeTopology = false;
                e.ConfigureConsumer<JobPortal.SearchService.Consumers.JobEventsConsumer>(context);
            });

            cfg.ReceiveEndpoint("job-deleted-event-search", e =>
            {
                e.ConfigureConsumeTopology = false;
                e.ConfigureConsumer<JobPortal.SearchService.Consumers.JobEventsConsumer>(context);
            });
        });
    }
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
