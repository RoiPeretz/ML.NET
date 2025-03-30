using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Hosting;

namespace MachineLearningDemo.Infrastructure.Persistence.Elasticsearch;

class ElasticSearchOnStartup(
    ElasticsearchClient elasticsearchClient,
    PersistenceLayerSettings settings)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await elasticsearchClient.Indices.CreateAsync(settings.IndexName, stoppingToken);
    }
}