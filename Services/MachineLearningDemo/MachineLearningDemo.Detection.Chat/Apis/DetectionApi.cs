using MachineLearningDemo.Core.Models;
using MachineLearningDemo.Core.Persistence.Queries;
using MachineLearningDemo.Detection.Chat.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MachineLearningDemo.Detection.Chat.Apis;

public static class DetectionApi
{
    public static RouteGroupBuilder MapDetectionApiV1(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("api/detect").HasApiVersion(1.0);

        api.MapPut("/", ImageObjectDetectionAsync).DisableAntiforgery();
        api.MapGet("/", GetDetectionResults);

        return api;
    }
    
    internal static async Task<Results<Ok<ImageDetectionResult[]>, BadRequest<string>, ProblemHttpResult>> ImageObjectDetectionAsync(
        IFormFile file,
        [FromServices] IImageObjectDetectionService service)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        
        var data = new ReadOnlyMemory<byte>(memoryStream.ToArray());
        var name = Guid.NewGuid() + Path.GetExtension(file.FileName);

        var result = await service.Detect(name, data);
        
        if (result is null)
        {
            return TypedResults.Problem(detail: "Image object detection failed.", statusCode: 500);
        }

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<ImageDetectionResult[]>, BadRequest<string>, ProblemHttpResult>> GetDetectionResults(
        string searchTerm,
        [FromServices] IQueryDetectionResultBySearchTerm queryDetectionResultBySearchTerm)
    {
        var result = await queryDetectionResultBySearchTerm.Query(searchTerm);

        return TypedResults.Ok(result.ToArray());
    }

}