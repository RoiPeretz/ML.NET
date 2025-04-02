using Asp.Versioning;
using MachineLearningDemo.BFF;
using MachineLearningDemo.BFF.EventBus;
using MachineLearningDemo.BFF.Hubs;
using MachineLearningDemo.BFF.Services;
using MachineLearningDemo.Core.EventBus.Events;
using MachineLearningDemo.Core.EventBus.Extensions;
using MachineLearningDemo.Infrastructure.EventBus.RabbitMq;
using MachineLearningDemo.ServiceDefaults;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddDistributedMemoryCache();

builder.AddEventBus("eventbus")
    .AddSubscription<IngestionStartedEvent, IngestionStatusEventHandler>()
    .AddSubscription<AddedToObjectStorageEvent, IngestionStatusEventHandler>()
    .AddSubscription<DetectionStartedEvent, IngestionStatusEventHandler>()
    .AddSubscription<DetectionEndedEvent, IngestionStatusEventHandler>();

builder.Services.AddHttpClient<IImageRepositoryClientGen, ImageRepositoryClientGen>(
    static client => client.BaseAddress = new Uri("https+http://ImageRepository"));

builder.Services.AddSignalR(o =>
{
    o.EnableDetailedErrors = true;
    o.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB
});

builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        corsPolicyBuilder =>
        {
            corsPolicyBuilder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddSingleton<IIngestionStatusService, IngestionStatusService>();

//TODO: Switch Swashbuckle to Microsoft.AspNetCore.OpenApi 
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "/openapi/{documentName}.json";
    });
    app.MapScalarApiReference();
}

app.UseCors();

app.UseExceptionHandler();

app.MapDefaultEndpoints();

app.MapHub<DetectionHub>("/Detect");

app.Run();