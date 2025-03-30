using Microsoft.Extensions.AI;

namespace MachineLearningDemo.Detection.Chat.ChatClient;

public interface IChatClientBuilder
{
    IChatClient Build(string connectionStringKey);
}