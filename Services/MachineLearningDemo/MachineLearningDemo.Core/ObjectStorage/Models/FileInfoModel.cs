namespace MachineLearningDemo.Core.ObjectStorage.Models;

public class FileInfoModel
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public ulong Size { get; set; }
    public string Url { get; set; } = string.Empty;
}