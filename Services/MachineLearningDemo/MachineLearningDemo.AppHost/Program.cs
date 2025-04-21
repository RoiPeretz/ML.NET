using MachineLearningDemo.AppHost.ResourceBuilderExtensions.MinIo;

var builder = DistributedApplication.CreateBuilder(args);

#region Infrastructure
var cache = builder.AddRedis("cache");

var rabbitMq = builder.AddRabbitMQ("eventbus")
    .WithLifetime(ContainerLifetime.Persistent);

var minio = builder.AddMinIo("storage", options => options
        .WithPorts(apiPort: 9000, consolePort: 9001)
        .WithCredentials("minio", "minio123")
        .WithDataVolume("data-minio"))
    .WithLifetime(ContainerLifetime.Persistent);

var elasticsearch = builder.AddElasticsearch("elasticsearch")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);
#endregion

#region Chat Object Detction
var ollama = builder.AddOllama("ollama")
                    .WithOpenWebUI()
                    .WithDataVolume()
                    //.WithGPUSupport()
                    //.WithContainerRuntimeArgs("--gpus=all")
                    .WithLifetime(ContainerLifetime.Persistent);

var llava = ollama.AddModel("llava:13b");
//var janus = ollama.AddModel("erwan2/DeepSeek-Janus-Pro-7B-Vision-Encoder");

var detectionChat = builder.AddProject<Projects.MachineLearningDemo_Detection_Chat>("DetectionChat")
    .WithExternalHttpEndpoints()
    .WithReference(minio).WaitFor(minio)
    .WithReference(rabbitMq).WaitFor(rabbitMq)
    .WithReference(elasticsearch).WaitFor(elasticsearch)
    .WithReference(llava).WaitFor(llava);
//.WithReference(janus).WaitFor(janus)
#endregion

#region Pretrained Object Detection Model

var detectionPretrainedModel = builder
    .AddProject<Projects.MachineLearningDemo_Detection_PretrainedModel>("DetectionPretrainedModel")
    .WithExternalHttpEndpoints()
    .WithReference(minio).WaitFor(minio)
    .WithReference(rabbitMq).WaitFor(rabbitMq)
    .WithReference(elasticsearch).WaitFor(elasticsearch);
#endregion

var imageRepository = builder.AddProject<Projects.MachineLearningDemo_ImageRepository>("ImageRepository")
    .WithExternalHttpEndpoints()
    .WithReference(rabbitMq).WaitFor(rabbitMq)
    .WithReference(minio).WaitFor(minio);

builder.AddProject<Projects.MachineLearningDemo_BFF>("BFF")
    .WithReference(detectionChat)
    .WithReference(detectionPretrainedModel)
    .WithReference(imageRepository)
    .WithReference(rabbitMq).WaitFor(rabbitMq);

builder.Build().Run();
