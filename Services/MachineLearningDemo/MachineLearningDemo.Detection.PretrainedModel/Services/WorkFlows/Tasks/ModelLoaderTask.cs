using MachineLearningDemo.Detection.PretrainedModel.Models;
using Microsoft.ML;

namespace MachineLearningDemo.Detection.PretrainedModel.Services.WorkFlows.Tasks;

public interface IModelLoaderTask
{
    ITransformer LoadModel(MLContext mlContext, string imageFolder);
}

public class ModelLoaderTask(
    Settings settings, 
    ILogger<ModelLoaderTask> logger) : IModelLoaderTask
{
    public ITransformer LoadModel(MLContext mlContext, string imageFolder)
    {
        logger.LogInformation("Read model");
        logger.LogInformation("Model location: {path}", settings.ModelFilePath);
        logger.LogInformation("Default parameters: image size=({ImageWidth},{ImageHeight})", ImageNetSettings.ImageWidth, ImageNetSettings.ImageHeight);
       
        // Create IDataView from empty list to obtain input data schema
        var data = mlContext.Data.LoadFromEnumerable(new List<ImageData>());

        // Define scoring pipeline
        var pipeline = mlContext.Transforms.LoadImages(
                outputColumnName: "image",
                imageFolder: imageFolder,
                inputColumnName: nameof(ImageData.ImagePath))
            .Append(mlContext.Transforms.ResizeImages(
                outputColumnName: "image",
                imageWidth: 416,
                imageHeight: 416,
                inputColumnName: "image"))
            .Append(mlContext.Transforms.ExtractPixels(
                outputColumnName: "image"))
            .Append(mlContext.Transforms.CopyColumns(
                outputColumnName: TinyYoloModelSettings.ModelInput,
                inputColumnName: "image"))
            .Append(mlContext.Transforms.ApplyOnnxModel(modelFile: settings.ModelFilePath, outputColumnNames: new[] { TinyYoloModelSettings.ModelOutput }, inputColumnNames: new[] { TinyYoloModelSettings.ModelInput }));

        var model = pipeline.Fit(data);

        return model;
    }
}
