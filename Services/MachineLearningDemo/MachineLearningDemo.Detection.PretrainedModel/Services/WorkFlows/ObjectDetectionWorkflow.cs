using MachineLearningDemo.Core.EventBus.Abstractions;
using MachineLearningDemo.Core.EventBus.Events;
using MachineLearningDemo.Core.Models;
using MachineLearningDemo.Core.Persistence.Commands;
using MachineLearningDemo.Detection.PretrainedModel.Models;
using MachineLearningDemo.Detection.PretrainedModel.Services.WorkFlows.Tasks;
using MachineLearningDemo.Detection.PretrainedModel.YoloParser;
using Microsoft.ML;

namespace MachineLearningDemo.Detection.PretrainedModel.Services.WorkFlows;

internal interface IObjectDetectionWorkflow
{
    Task<ImageDetectionResult?> Detect(string fileName, ReadOnlyMemory<byte> image, string contentType);
}

internal class ObjectDetectionWorkflow(
    Settings settings,
    IEventBus eventBus,
    IModelLoaderTask modelLoaderTask,
    IYoloOutputParser yoloOutputParser,
    ILogger<ObjectDetectionWorkflow> logger,
    ISaveImageToTempFileTask saveImageToTempFileTask,
    IPredictDataUsingModelTask predictDataUsingModelTask,
    IAddImageDetectionResultCommand addImageDetectionResultCommand)
    : IObjectDetectionWorkflow
{
    public async Task<ImageDetectionResult?> Detect(string fileName, ReadOnlyMemory<byte> image, string contentType)
    {
        var detectionStarted = new DetectionStartedEvent
        {
            FileName = fileName,
            ModelName = settings.ModelName,
        };
        await eventBus.PublishAsync(detectionStarted);

        var tempFolderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempFolderPath);
        
        try
        {
            var mlContext = new MLContext();
            var filePath = saveImageToTempFileTask.Save(tempFolderPath, fileName, image, contentType);
            var modelScorer = modelLoaderTask.LoadModel(mlContext, tempFolderPath);

            var enumerableData = new[]
            {
                new ImageData(filePath, fileName)
            };

            var imageDataView = mlContext.Data.LoadFromEnumerable(enumerableData);
            var probabilities = predictDataUsingModelTask.Predict(imageDataView, modelScorer);

            var boundingBoxes =
                probabilities
                    .Select(probability => yoloOutputParser.ParseOutputs(probability))
                    .Select(boxes => yoloOutputParser.FilterBoundingBoxes(boxes, 5, .5F));

            var objectList = boundingBoxes.FirstOrDefault();
            var detectedObjects = Enumerable.Empty<DetectedObject>();
            if (objectList is not null)
            {
                detectedObjects = objectList.Select(@object =>
                    new DetectedObject(@object.Label, string.Empty, @object.Dimensions.ToString()));
            }

            var detectionTime = DateTime.UtcNow.Subtract(detectionStarted.CreationDate).TotalMilliseconds;

            var result = new ImageDetectionResult
            {
                FileName = fileName,
                DetectedObjects = detectedObjects,
                DetectionSource = "MachineLearningDemo.Detection.PretrainedModel " + settings.ModelName,
                DetectionTimeMilliseconds = detectionTime,
            };

            await addImageDetectionResultCommand.Add(result);

            var detectionEnded = new DetectionEndedEvent
            {
                FileName = fileName,
                DetectionTime = detectionTime,
                ModelName = settings.ModelName,
            };

            await eventBus.PublishAsync(detectionEnded);

            return result;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Detection using pretrained model failed");
        }
        finally
        {
            try
            {
                Directory.Delete(tempFolderPath, true);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to delete temp folder");
            }
        }

        return null;
    }
}