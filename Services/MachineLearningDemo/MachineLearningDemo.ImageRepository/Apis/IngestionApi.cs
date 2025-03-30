using MachineLearningDemo.Core.ObjectStorage.Models;
using MachineLearningDemo.ImageRepository.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MachineLearningDemo.ImageRepository.Apis;

public static class IngestionApi
{
    public static RouteGroupBuilder MapIngestionApiV1(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("api/ingest").HasApiVersion(1.0);

        api.MapPut("/", ImageIngestAsync).DisableAntiforgery(); 

        return api;
    }

    internal static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> ImageIngestAsync(
        IFormFile file,
        [FromServices] IIngestionService service)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        
        var fileDto = new FileModel
        {
            FileName = Guid.NewGuid() + Path.GetExtension(file.FileName),
            Size = file.Length,
            ContentType = file.ContentType,
            Data = memoryStream.ToArray()
        };

        var result = await service.IngestImageAsync(fileDto);
        
        if (result is false)
        {
            return TypedResults.Problem(detail: "Image ingestion failed.", statusCode: 500);
        }

        return TypedResults.Ok();
    }

}