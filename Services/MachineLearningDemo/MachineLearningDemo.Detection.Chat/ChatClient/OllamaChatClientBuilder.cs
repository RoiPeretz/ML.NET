using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace MachineLearningDemo.Detection.Chat.ChatClient;

public class OllamaChatClientBuilder(
    IConfiguration configuration,
    ILogger<ChatClientBuilder> logger) : IChatClientBuilder
{
    private const string DefaultModel = "llava:7b";
    private const string DefaultEndpoint = "http://localhost:11434";

    public IChatClient Build(string connectionStringKey)
    {
        var config = configuration.GetConnectionString(connectionStringKey);

        //TODO: Microsoft.Extensions.AI is currently in preview - in the future avoid connection string manipulation
        var keyValuePairs = config?.Split(';')
            .Select(part => part.Split('='))
            .ToDictionary(split => split[0], split => split[1]);

        var endpoint = keyValuePairs?["Endpoint"] ?? DefaultModel;
        var chatModel = keyValuePairs?["Model"] ?? DefaultEndpoint;

        var ollamaClient = new OllamaChatClient(new Uri(endpoint), chatModel);
        var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));

        var client = new ChatClientBuilder(ollamaClient)
            .UseDistributedCache(cache)
            .UseFunctionInvocation()
            .UseOpenTelemetry(configure: c => c.EnableSensitiveData = true)
            .Build();


        logger.LogInformation("Created chat client for model {Model} at {Endpoint}", chatModel, endpoint);

        return client;
    }
}