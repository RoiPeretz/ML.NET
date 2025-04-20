using MachineLearningDemo.Core.ObjectStorage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Minio.DataModel.Args;
using Minio.Exceptions;
using Minio;

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

                var policyJson = $$"""
                                   
                                                   {
                                                       "Version": "2012-10-17",
                                                       "Statement": [
                                                           {
                                                               "Effect": "Allow",
                                                               "Principal": "*",
                                                               "Action": "s3:GetObject",
                                                               "Resource": "arn:aws:s3:::{{settings.AssetsBucketName}}/*"
                                                           }
                                                       ]
                                                   }
                                   """;

                var setPolicyArgs = new SetPolicyArgs()
                    .WithBucket(settings.AssetsBucketName)
                    .WithPolicy(policyJson);

                await minioClient.SetPolicyAsync(setPolicyArgs, stoppingToken);
            }
        }
        catch (MinioException e)
        {
            logger.LogError(e, "{name} bucket does not exist or creation failed!", settings.AssetsBucketName);
        }
    }
}