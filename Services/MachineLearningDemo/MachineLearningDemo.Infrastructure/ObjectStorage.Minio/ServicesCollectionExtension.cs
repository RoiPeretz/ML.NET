using MachineLearningDemo.Core.ObjectStorage;
using MachineLearningDemo.Core.ObjectStorage.Interfaces;
using MachineLearningDemo.Infrastructure.ObjectStorage.Minio.AssetsBucket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace MachineLearningDemo.Infrastructure.ObjectStorage.Minio;

public static class ServicesCollectionExtension
{
    public static void AddObjectStorage(this IServiceCollection services)
    {
        var configuration = services.BuildServiceProvider().GetService<IConfiguration>();
        if (configuration is null)
            throw new InvalidOperationException("Configuration is null");

        services.AddMinio(options =>
        {
            var info = ConnectionInfo.ParseMinioConnectionString(configuration["ConnectionStrings:storage"]);

            options.WithEndpoint(info.Endpoint);
            options.WithCredentials(info.AccessKey, info.SecretKey);
            options.WithSSL(false);
        });

        services.AddHostedService<AssetsBucketCreator>();
        services.AddSingleton(new ObjectStorageSettings());
        services.AddTransient<IAssetsObjectStorageClient, AssetsObjectStorageClient>();
    }
}