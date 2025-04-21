namespace MachineLearningDemo.Detection.PretrainedModel.Services.WorkFlows.Tasks;

internal interface ISaveImageToTempFileTask
{
    string Save(string tempFolder, string fileName, ReadOnlyMemory<byte> image, string contentType);
}

internal class SaveImageToTempFileTask : ISaveImageToTempFileTask
{
    public string Save(string tempFolder, string fileName, ReadOnlyMemory<byte> image, string contentType)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = contentType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/gif" => ".gif",
                "image/bmp" => ".bmp",
                _ => ".tmp"
            };
            fileName += extension;
        }

        var tempPath = Path.Combine(tempFolder, fileName);

        using var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None);
        stream.Write(image.Span);

        return tempPath;
    }
}