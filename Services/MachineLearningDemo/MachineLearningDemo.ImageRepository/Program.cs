using Asp.Versioning;
using MachineLearningDemo.ImageRepository.Apis;
using MachineLearningDemo.ImageRepository.Services;
using MachineLearningDemo.ImageRepository.Services.Workflows;
using MachineLearningDemo.Infrastructure.EventBus.RabbitMq;
using MachineLearningDemo.Infrastructure.ObjectStorage.Minio;
using MachineLearningDemo.ServiceDefaults;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddDistributedMemoryCache();

builder.AddEventBus("eventbus");
builder.Services.AddObjectStorage();

builder.Services.AddTransient<IIngestImageWorkflow, IngestImageWorkflow>();
builder.Services.AddTransient<IIngestionService, IngestionService>();

builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
});

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

app.UseExceptionHandler();

app.MapDefaultEndpoints();

app.NewVersionedApi("Ingestion").MapIngestionApiV1();

app.Run();