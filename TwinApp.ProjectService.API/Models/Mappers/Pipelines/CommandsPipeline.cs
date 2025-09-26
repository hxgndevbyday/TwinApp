using MongoDB.Bson;
using TwinApp.ProjectService.API.Repository;

namespace TwinApp.ProjectService.API.Models.Mappers.Pipelines;

public class CommandsPipeline() : ISectionPipeline
{
    public string SectionType => "Commands";

    public Task<IEnumerable<BsonDocument>> ExtractAsync(BsonDocument root, CancellationToken ct)
    {
        var results = new List<BsonDocument>();
        var creationTime = DateTime.UtcNow;

        // Expecting root["Children"]["$values"]
        if (root.TryGetValue("Children", out var children) &&
            children.IsBsonDocument &&
            children.AsBsonDocument.TryGetValue("$values", out var values) &&
            values.IsBsonArray)
        {
            foreach (var child in values.AsBsonArray)
            {
                if (child is not BsonDocument cmd) continue;

                var entityType = cmd["$type"].AsString.Split(',')[0].Split('.').Last();

                var doc = new BsonDocument
                {
                    { "_id", Guid.NewGuid().ToString() },
                    { "EntityType", entityType },
                    { "ParentId", root.Contains("_id") ? root["_id"] : BsonNull.Value },
                    { "ProjectId", root.Contains("ProjectId") ? root["ProjectId"] : BsonNull.Value },
                    { "Name", cmd.Contains("Name") ? cmd["Name"].AsString : entityType },
                    { "Components", cmd }, // keep full command doc for now
                    { "CreatedAt", creationTime }
                };

                results.Add(doc);

                // Recurse if this command has its own children
                if (cmd.TryGetValue("Children", out var grandChildren) &&
                    grandChildren.IsBsonDocument &&
                    grandChildren.AsBsonDocument.TryGetValue("$values", out var gcValues) &&
                    gcValues.IsBsonArray)
                {
                    var nested = ExtractAsync(cmd, ct).Result;
                    results.AddRange(nested);
                }
            }
        }

        return Task.FromResult<IEnumerable<BsonDocument>>(results);
    }

    public Task<IEnumerable<BsonDocument>> TransformAsync(IEnumerable<BsonDocument> extracted, CancellationToken ct)
    {
        // Optional normalization step, e.g. force a consistent EntityType naming
        var transformed = extracted.Select(doc =>
        {
            if (doc["EntityType"] == "GroupCommand")
                doc["EntityType"] = "CommandGroup"; // normalize naming
            return doc;
        });

        return Task.FromResult(transformed);
    }

    public async Task<IEnumerable<BsonDocument>> ProcessAsync(BsonDocument inputSection, CancellationToken ct)
    {
        return await TransformAsync(await ExtractAsync(inputSection, ct), ct);
    }
}
