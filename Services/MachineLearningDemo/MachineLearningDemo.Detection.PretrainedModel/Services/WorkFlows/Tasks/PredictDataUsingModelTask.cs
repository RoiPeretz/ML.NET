using Microsoft.ML;
using Microsoft.ML.Data;

namespace MachineLearningDemo.Detection.PretrainedModel.Services.WorkFlows.Tasks;

internal interface IPredictDataUsingModelTask
{
    IEnumerable<float[]> Predict(IDataView testData, ITransformer model);
}

internal class PredictDataUsingModelTask : IPredictDataUsingModelTask
{
    public IEnumerable<float[]> Predict(IDataView testData, ITransformer model)
    {
        var scoredData = model.Transform(testData);

        var probabilities = scoredData.GetColumn<float[]>(TinyYoloModelSettings.ModelOutput);

        return probabilities;
    }
}