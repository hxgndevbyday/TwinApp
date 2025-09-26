using MongoDB.Bson;
using TwinApp.ProjectService.API.Repository;

namespace TwinApp.ProjectService.API.Models.Mappers.Pipelines;

public interface ISectionPipeline
{
    string SectionType { get; }

    Task<IEnumerable<BsonDocument>> ExtractAsync(BsonDocument root, CancellationToken ct);
    Task<IEnumerable<BsonDocument>> TransformAsync(IEnumerable<BsonDocument> extracted, CancellationToken ct);
    Task<IEnumerable<BsonDocument>> ProcessAsync(BsonDocument inputSection, CancellationToken ct);
}