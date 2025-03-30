using MachineLearningDemo.Core.ObjectStorage.Models;

namespace MachineLearningDemo.Core.ObjectStorage.Interfaces;

public interface IAssetsObjectStorageClient
{
    Task<IList<FileInfoModel>> GetAllAssetsFileInfoAsync();
    Task<FileModel?> GetAssetByNameAsync(string name);
    Task<bool> AddAssetAsync(FileModel file);
}