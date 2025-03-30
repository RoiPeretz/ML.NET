
# ML Demo

## Prerequisites

1) Before cloning this repo git lfs hould be installed - [[Install]](https://git-lfs.com/)

2) ML Demo backend services are running Ollam. If a model needs GPU's support uncomment the following lines at Services\MachineLearningDemo\MachineLearningDemo.AppHost\Program.cs - 

```csharp 
var ollama = builder.AddOllama("ollama")
                    .WithOpenWebUI()
                    .WithDataVolume()
                    //.WithGPUSupport()
                    //.WithContainerRuntimeArgs("--gpus=all")
                    .WithLifetime(ContainerLifetime.Persistent);
``` 

##



