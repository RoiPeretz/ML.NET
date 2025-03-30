using MachineLearningDemo.Core.ObjectStorage.Models;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;

namespace MachineLearningDemo.Infrastructure.ObjectStorage.Minio;

internal static class MinIoExtensions
{
    public static async Task<FileInfoModel> ToFileInfoDto(this Item item, IMinioClient minioClient, string bucketName)
    {
        return new FileInfoModel
        {
            FileName = item.Key,
            Size = item.Size,
            ContentType = item.ContentType,
            Url = await minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(item.Key)
                .WithExpiry(3600)) // 1-hour access URL
        };
    }

    public static FileModel ToFileDto(this ObjectStat item, MemoryStream ms, IMinioClient minioClient, string bucketName)
    {
        return new FileModel
        {
            FileName = item.ObjectName,
            Size = item.Size,
            ContentType = item.ContentType,
            Data = ms.ToArray()
        };
    }
}