using MachineLearningDemo.Core.Persistence.Commands;
using MachineLearningDemo.Core.Persistence.Queries;
using MachineLearningDemo.Infrastructure.Persistence.Elasticsearch.Commands;
using MachineLearningDemo.Infrastructure.Persistence.Elasticsearch.Queries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MachineLearningDemo.Infrastructure.Persistence.Elasticsearch;

public record PersistenceLayerSettings(string ConnectionName, string IndexName);

public static class HostApplicationBuilderExtension
{
    public static void AddPersistence(this IHostApplicationBuilder builder, PersistenceLayerSettings settings)
    {
        builder.Services.AddSingleton(settings);
        builder.Services.AddHostedService<ElasticSearchOnStartup>();
        builder.AddElasticsearchClient(connectionName: settings.ConnectionName);
        builder.Services.AddTransient<IAddImageDetectionResultCommand, AddImageDetectionResultCommand>();
        builder.Services.AddTransient<IQueryDetectionResultBySearchTerm, QueryDetectionResultBySearchTerm>();
    }
}