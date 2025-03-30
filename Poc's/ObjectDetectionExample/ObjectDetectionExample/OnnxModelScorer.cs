using Microsoft.ML;
using Microsoft.ML.Data;
using ObjectDetectionExample.DataStructures;
using ObjectDetectionExample.YoloParser;

namespace ObjectDetectionExample
{
    class OnnxModelScorer(string? imagesFolder, string modelLocation, MLContext mlContext)
    {
        public struct ImageNetSettings
        {
            public const int ImageHeight = 416;
            public const int ImageWidth = 416;
        }

        public struct TinyYoloModelSettings
        {
            // input tensor name
            public const string ModelInput = "input_1:0"; 

            // output tensor names
            public const string ModelOutputBoxes = "Identity:0";
            public const string ModelOutputLabels = "Identity_1:0";
            public const string ModelOutputScores = "Identity_2:0";
        }
        private ITransformer LoadModel(string modelLocation)
        {
            Console.WriteLine("Read model");
            Console.WriteLine($"Model location: {modelLocation}");
            Console.WriteLine($"Default parameters: image size=({ImageNetSettings.ImageWidth},{ImageNetSettings.ImageHeight})");

            // Create IDataView from empty list to obtain input data schema
            var data = mlContext.Data.LoadFromEnumerable(new List<ImageNetData>());

            // Define scoring pipeline
            var pipeline = mlContext.Transforms.LoadImages(
                    outputColumnName: "image",
                    imageFolder: imagesFolder,
                    inputColumnName: nameof(ImageNetData.ImagePath))
                .Append(mlContext.Transforms.ResizeImages(
                    outputColumnName: "image",
                    imageWidth: 416,
                    imageHeight: 416,
                    inputColumnName: "image"))
                .Append(mlContext.Transforms.ExtractPixels(
                    outputColumnName: "image"))
                // Copy the "image" column to the expected input node name.
                .Append(mlContext.Transforms.CopyColumns(
                    outputColumnName: TinyYoloModelSettings.ModelInput,
                    inputColumnName: "image"))
                .Append(mlContext.Transforms.ApplyOnnxModel(
                    modelFile: modelLocation,
                    outputColumnNames: new[] { TinyYoloModelSettings.ModelOutputBoxes, TinyYoloModelSettings.ModelOutputLabels, TinyYoloModelSettings.ModelOutputScores },
                    inputColumnNames: new[] { TinyYoloModelSettings.ModelInput }));

            // Fit scoring pipeline
            var model = pipeline.Fit(data);

            return model;
        }

        private IEnumerable<OnnxPredictionOutput> PredictDataUsingModel(IDataView testData, ITransformer model)
        {
            Console.WriteLine($"Images location: {imagesFolder}");
            Console.WriteLine("");
            Console.WriteLine("=====Identify the objects in the images=====");
            Console.WriteLine("");

            var scoredData = model.Transform(testData);

            var boxes = scoredData.GetColumn<float[]>(TinyYoloModelSettings.ModelOutputBoxes).ToArray();
            var labels = scoredData.GetColumn<float[]>(TinyYoloModelSettings.ModelOutputLabels).ToArray();
            var scores = scoredData.GetColumn<float[]>(TinyYoloModelSettings.ModelOutputScores).ToArray();

            var results = new List<OnnxPredictionOutput>();
            for (int i = 0; i < boxes.Length; i++)
            {
                var prediction = new OnnxPredictionOutput
                {
                    Boxes = boxes[i],
                    Labels = labels[i],
                    Scores = scores[i]
                };
                results.Add(prediction);
            }

            return results;
        }

        public IEnumerable<OnnxPredictionOutput> Score(IDataView data)
        {
            var model = LoadModel(modelLocation);

            return PredictDataUsingModel(data, model);
        }
    }
}

