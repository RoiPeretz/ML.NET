using MachineLearningDemo.Core.ObjectStorage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace MachineLearningDemo.Infrastructure.ObjectStorage.Minio.AssetsBucket;

internal class AssetsBucketCreator(
    IMinioClient minioClient,
    ObjectStorageSettings settings,
    ILogger<AssetsBucketCreator> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var bucketExistsArgs = new BucketExistsArgs()
                .WithBucket(settings.AssetsBucketName);
            
            var found = await minioClient.BucketExistsAsync(bucketExistsArgs, stoppingToken);
            if (found is false)
            {
                var bucketArgs = new MakeBucketArgs()
                    .WithBucket(settings.AssetsBucketName);

                await minioClient.MakeBucketAsync(bucketArgs, stoppingToken);
            }
        }
        catch (MinioException e)
        {
            logger.LogError(e, "{name} bucket does not exists or creation failed!", settings.AssetsBucketName);
        }
    }
}

