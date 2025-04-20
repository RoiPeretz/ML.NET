using System.Drawing;
using System.Drawing.Drawing2D;
using Microsoft.ML;
using ObjectDetection.DataStructures;
using ObjectDetection.YoloParser;
using ObjectDetectionExample;


var assetsRelativePath = @"../../../../../../Assets";
var modelsRelativePath = @"../../../../../../Models";

string assetsPath = GetAbsolutePath(assetsRelativePath);
var modelFilePath = Path.Combine(modelsRelativePath, "Yolov2", "TinyYolo2_model.onnx");
var imagesFolder = Path.Combine(assetsPath, "images");
var outputFolder = Path.Combine(assetsPath, "output");

// Initialize MLContext
MLContext mlContext = new MLContext();

try
{
    // Load Data
    IEnumerable<ImageNetData> images = ImageNetData.ReadFromFile(imagesFolder);
    IDataView imageDataView = mlContext.Data.LoadFromEnumerable(images);

    // Create instance of model scorer
    var modelScorer = new OnnxModelScorer(imagesFolder, modelFilePath, mlContext);

    // Use model to score data
    var probabilities = modelScorer.Score(imageDataView);

    // Post-process model output
    YoloOutputParser parser = new YoloOutputParser();

    // Update the LINQ query to correctly access the `Boxes` property of `OnnxPredictionOutput`
    // which is a `float[]` and matches the expected input type for `ParseOutputs`.

    var boundingBoxes =
        probabilities
        .Select(probability => parser.ParseOutputs(probability)) // Access the `Boxes` property
        .Select(boxes => parser.FilterBoundingBoxes(boxes, 5, .5F));

    // Draw bounding boxes for detected objects in each of the images
    for (var i = 0; i < images.Count(); i++)
    {
        string imageFileName = images.ElementAt(i).Label;
        IList<YoloBoundingBox> detectedObjects = boundingBoxes.ElementAt(i);

        DrawBoundingBox(imagesFolder, outputFolder, imageFileName, detectedObjects);

        LogDetectedObjects(imageFileName, detectedObjects);
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}

Console.WriteLine("========= End of Process..Hit any Key ========");

string GetAbsolutePath(string relativePath)
{
    FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
    string assemblyFolderPath = _dataRoot.Directory.FullName;

    string fullPath = Path.Combine(assemblyFolderPath, relativePath);

    return fullPath;
}

void DrawBoundingBox(string inputImageLocation, string outputImageLocation, string imageName, IList<YoloBoundingBox> filteredBoundingBoxes)
{
    using var image = System.Drawing.Image.FromFile(Path.Combine(inputImageLocation, imageName));

    var originalImageHeight = image.Height;
    var originalImageWidth = image.Width;

    foreach (var box in filteredBoundingBoxes)
    {
        // Get Bounding Box Dimensions  
        var x = (int)Math.Max(box.Dimensions.X, 0);
        var y = (int)Math.Max(box.Dimensions.Y, 0);
        var width = (int)Math.Min(originalImageWidth - x, box.Dimensions.Width);
        var height = (int)Math.Min(originalImageHeight - y, box.Dimensions.Height);

        // Resize to match the image dimensions  
        x = originalImageWidth * x / OnnxModelScorer.ImageNetSettings.ImageWidth;
        y = originalImageHeight * y / OnnxModelScorer.ImageNetSettings.ImageHeight;
        width = originalImageWidth * width / OnnxModelScorer.ImageNetSettings.ImageWidth;
        height = originalImageHeight * height / OnnxModelScorer.ImageNetSettings.ImageHeight;

        // Bounding Box Text  
        var text = $"{box.Label} ({(box.Confidence * 100):0}%)";

        using var thumbnailGraphic = Graphics.FromImage(image);

        thumbnailGraphic.CompositingQuality = CompositingQuality.HighQuality;
        thumbnailGraphic.SmoothingMode = SmoothingMode.HighQuality;
        thumbnailGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;

        // Define Text Options  
        var drawFont = new System.Drawing.Font("Arial", 12, FontStyle.Bold);
        var size = thumbnailGraphic.MeasureString(text, drawFont);
        var fontBrush = new SolidBrush(Color.Black);
        var atPoint = new Point(x, y - (int)size.Height - 1);

        // Define Bounding Box options  
        var pen = new Pen(box.BoxColor, 3.2f);
        var colorBrush = new SolidBrush(box.BoxColor);

        // Draw text on image  
        thumbnailGraphic.FillRectangle(colorBrush, x, y - (int)size.Height - 1, (int)size.Width, (int)size.Height);
        thumbnailGraphic.DrawString(text, drawFont, fontBrush, atPoint);

        // Draw bounding box on image  
        thumbnailGraphic.DrawRectangle(pen, x, y, width, height);
    }

    // Ensure the output directory exists  
    if (!Directory.Exists(outputImageLocation))
    {
        Directory.CreateDirectory(outputImageLocation);
    }

    // Save the image with bounding boxes  
    image.Save(Path.Combine(outputImageLocation, imageName));
}

void LogDetectedObjects(string imageName, IList<YoloBoundingBox> boundingBoxes)
{
    Console.WriteLine($".....The objects in the image {imageName} are detected as below....");

    foreach (var box in boundingBoxes)
    {
        Console.WriteLine($"{box.Label} and its Confidence score: {box.Confidence}");
    }

    Console.WriteLine("");
}
