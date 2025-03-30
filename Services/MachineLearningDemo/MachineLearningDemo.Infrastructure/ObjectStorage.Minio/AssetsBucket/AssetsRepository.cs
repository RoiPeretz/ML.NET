using MachineLearningDemo.Core.ObjectStorage;
using MachineLearningDemo.Core.ObjectStorage.Interfaces;
using MachineLearningDemo.Core.ObjectStorage.Models;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;

namespace MachineLearningDemo.Infrastructure.ObjectStorage.Minio.AssetsBucket;

internal class AssetsObjectStorageClient(
    IMinioClient minioClient,
    ObjectStorageSettings settings,
    ILogger<AssetsObjectStorageClient> logger) : IAssetsObjectStorageClient
{
    public async Task<IList<FileInfoModel>> GetAllAssetsFileInfoAsync()
    {
        var result = new List<FileInfoModel>();
       
        try
        {
            var listArgs = new ListObjectsArgs()
                .WithBucket(settings.AssetsBucketName);

            var observable = minioClient.ListObjectsEnumAsync(listArgs);

            await foreach (var item in observable)
            {
                if (item.IsDir) continue;

                result.Add(await item.ToFileInfoDto(minioClient, settings.AssetsBucketName));
            }
        }
        catch (Exception e)
        {
           logger.LogError(e, "Object storage get all as file info failed");
        }
        
        return result;
    }

    public async Task<FileModel?> GetAssetByNameAsync(string name)
    {
        var memoryStream = new MemoryStream();

        try
        {

            var item = await minioClient.GetObjectAsync(new GetObjectArgs()
                .WithBucket(settings.AssetsBucketName)
                .WithObject(name)
                .WithCallbackStream(stream => { stream.CopyTo(memoryStream); }));

            return item.ToFileDto(memoryStream, minioClient, settings.AssetsBucketName);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Object storage get file by name failed");
        }
        finally
        {
            await memoryStream.DisposeAsync();
        }

        return null;
    }

    public async Task<bool> AddAssetAsync(FileModel file)
    {
        MemoryStream? memoryStream = null;
        try
        {
            if (file.Data is null) return false;

            memoryStream = new MemoryStream(file.Data); 

            await minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(settings.AssetsBucketName)
                .WithObject(file.FileName)
                .WithStreamData(memoryStream)
                .WithObjectSize(memoryStream.Length)
                .WithContentType(file.ContentType)); 
        }
        catch (Exception e)
        {
            logger.LogError(e, "Object storage get file by name failed");
            return false;
        }
        finally
        {
            if (memoryStream != null) await memoryStream.DisposeAsync();
        }

        return true;
    }
}