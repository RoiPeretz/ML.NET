using Microsoft.Extensions.AI;

namespace MachineLearningDemo.Detection.Chat.ChatClient;

public class ChatClientWrapper(string modelName, IChatClient chatClient)
{
    public string ModelName => modelName;
    public IChatClient ChatClient => chatClient;
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWrappedChatClient(this IServiceCollection services, string modelName)
    {
        services.AddTransient(serviceProvider =>
        {
            var chatClient = serviceProvider.GetRequiredService<IChatClientBuilder>().Build(modelName);
            return new ChatClientWrapper(modelName, chatClient);
        });

        return services;
    }
}
