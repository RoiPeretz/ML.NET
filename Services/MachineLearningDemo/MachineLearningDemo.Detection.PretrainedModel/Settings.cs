namespace MachineLearningDemo.Detection.PretrainedModel;

public struct ImageNetSettings
{
    public const int ImageHeight = 416;
    public const int ImageWidth = 416;
}

public struct TinyYoloModelSettings
{
    public const string ModelInput = "image";
    public const string ModelOutput = "grid";
}

public class Settings
{
    public string ModelFilePath { get; }
    public string OutputFolder { get; set; }
    public string ImagesFolder { get; set; }
    public string ModelName { get; } = "Yolov2";

    public Settings()
    {
        const string assetsRelativePath = @"../../../Assets";
        const string modelsRelativePath = @"../../../Models";

        var assetsPath = GetAbsolutePath(assetsRelativePath);
        ModelFilePath = Path.Combine(modelsRelativePath, ModelName, "TinyYolo2_model.onnx");
        ImagesFolder = Path.Combine(assetsPath, "images");
        OutputFolder = Path.Combine(assetsPath, "output");
    }

    private string GetAbsolutePath(string relativePath)
    {
        var dataRoot = new FileInfo(typeof(Program).Assembly.Location);
        var assemblyFolderPath = dataRoot.Directory?.FullName;

        if (assemblyFolderPath is null) return string.Empty;
        
        var fullPath = Path.Combine(assemblyFolderPath, relativePath);
        return fullPath;
    }
}