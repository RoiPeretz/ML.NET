using Microsoft.Extensions.DependencyInjection;

namespace MachineLearningDemo.Core.EventBus.Abstractions;

public interface IEventBusBuilder
{
    public IServiceCollection Services { get; }
}
