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

var chatObjectDetection = builder.AddProject<Projects.MachineLearningDemo_Detection_Chat>("DetectionChat")
    .WithReference(minio).WaitFor(minio)
    .WithReference(rabbitMq).WaitFor(rabbitMq)
    .WithReference(elasticsearch).WaitFor(elasticsearch)
    .WithReference(llava).WaitFor(llava);
    //.WithReference(janus).WaitFor(janus)
#endregion

builder.AddProject<Projects.MachineLearningDemo_ImageRepository>("ImageRepository")
    .WithReference(rabbitMq).WaitFor(rabbitMq)
    .WithReference(minio).WaitFor(minio);

builder.Build().Run();
