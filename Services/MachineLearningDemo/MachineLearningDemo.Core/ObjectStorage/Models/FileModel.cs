namespace MachineLearningDemo.Core.ObjectStorage.Models;

public class FileModel
{
    public string FileName { get; set; } = string.Empty;
    public long Size { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public byte[]? Data { get; set; }
}