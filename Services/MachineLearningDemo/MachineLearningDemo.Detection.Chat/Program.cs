using Asp.Versioning;
using MachineLearningDemo.Core.EventBus.Events;
using MachineLearningDemo.Core.EventBus.Extensions;
using MachineLearningDemo.Detection.Chat.Apis;
using MachineLearningDemo.Detection.Chat.ChatClient;
using MachineLearningDemo.Detection.Chat.EventBus;
using MachineLearningDemo.Detection.Chat.Services;
using MachineLearningDemo.Detection.Chat.Services.WorkFlows;
using MachineLearningDemo.Detection.Chat.Services.WorkFlows.Tasks;
using MachineLearningDemo.Infrastructure.EventBus.RabbitMq;
using MachineLearningDemo.Infrastructure.ObjectStorage.Minio;
using MachineLearningDemo.Infrastructure.Persistence.Elasticsearch;
using MachineLearningDemo.ServiceDefaults;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
});

//TODO: Switch Swashbuckle to Microsoft.AspNetCore.OpenApi 
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddObjectStorage();
builder.AddPersistence(new PersistenceLayerSettings("elasticsearch", "chat-object-detection"));

builder.AddEventBus("eventbus")
    .AddSubscription<AddedToObjectStorageEvent, AddedToObjectStorageEventHandler>();

builder.Services.AddWrappedChatClient("ollama-llava");
//builder.Services.AddWrappedChatClient("ollama-janus");
builder.Services.AddTransient<IDetectObjectsTask, DetectObjectsTask>();
builder.Services.AddTransient<IChatClientBuilder, OllamaChatClientBuilder>();
builder.Services.AddTransient<IObjectDetectionWorkflow, ObjectDetectionWorkflow>();
builder.Services.AddTransient<IImageObjectDetectionService, ImageObjectDetectionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "/openapi/{documentName}.json";
    });
    app.MapScalarApiReference();
}

app.UseExceptionHandler();

app.MapDefaultEndpoints();

app.NewVersionedApi("Detection").MapDetectionApiV1(); 

app.Run();