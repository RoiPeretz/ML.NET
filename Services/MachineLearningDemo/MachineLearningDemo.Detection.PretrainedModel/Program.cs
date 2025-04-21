using Asp.Versioning;
using MachineLearningDemo.Core.EventBus.Events;
using MachineLearningDemo.Core.EventBus.Extensions;
using MachineLearningDemo.Core.Persistence.Commands;
using MachineLearningDemo.Detection.PretrainedModel;
using MachineLearningDemo.Detection.PretrainedModel.Apis;
using MachineLearningDemo.Detection.PretrainedModel.EventBus;
using MachineLearningDemo.Detection.PretrainedModel.Services;
using MachineLearningDemo.Detection.PretrainedModel.Services.WorkFlows;
using MachineLearningDemo.Detection.PretrainedModel.Services.WorkFlows.Tasks;
using MachineLearningDemo.Detection.PretrainedModel.YoloParser;
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
builder.AddPersistence(new PersistenceLayerSettings("elasticsearch", "pretrained-model-detection"));

builder.AddEventBus("eventbus")
    .AddSubscription<AddedToObjectStorageEvent, AddedToObjectStorageEventHandler>();

builder.Services.AddSingleton(new Settings());
builder.Services.AddTransient<IModelLoaderTask, ModelLoaderTask>();
builder.Services.AddTransient<IYoloOutputParser, YoloOutputParser>();
builder.Services.AddTransient<ISaveImageToTempFileTask, SaveImageToTempFileTask>();
builder.Services.AddTransient<IPredictDataUsingModelTask, PredictDataUsingModelTask>();
builder.Services.AddTransient<IObjectDetectionWorkflow, ObjectDetectionWorkflow>();
builder.Services.AddTransient<IModelImageObjectDetectionService, ModelImageObjectDetectionService>();

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
