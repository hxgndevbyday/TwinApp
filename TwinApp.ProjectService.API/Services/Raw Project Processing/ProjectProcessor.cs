using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using TwinApp.ProjectService.API.Models.Mappers;
using TwinApp.ProjectService.API.Models.Mappers.Pipelines;
using TwinApp.ProjectService.API.Repository;
using TwinApp.ProjectService.API.Services.Interfaces;

namespace TwinApp.ProjectService.API.Services.Raw_Project_Processing;

public class ProjectProcessor(ProjectRepository repo) : IProjectProcessor
{
    public async Task ProcessAsync(string projectId, CancellationToken cancellationToken = default)
    {
        // Get the file from GridFS
        var gridFsId = await repo.GetGridFsIdAsync(projectId);
        await using var stream = await repo.GetGridFsBlobAsync(gridFsId);

        if (stream != null)
        {
            // Ensure the stream is at the beginning
            if (stream.CanSeek)
                stream.Position = 0;

            // Deserialize BSON directly to BsonDocument
            var projectDoc = BsonSerializer.Deserialize<BsonDocument>(stream);

            var (ecsEntities, metadataDocs) = ProjectMappers.DocumentToProjectEntities(projectDoc);

            // // Step 1: insert top-level ECS entities
            // foreach (var section in ecsEntities)
            //     await repo.InsertEntityAsync(projectId, section["EntityType"].AsString, section, gridFsId);

            // Step 2: extract nested ECS entities (e.g., each machine)
            foreach (var section in ecsEntities)
            {
                // var nested = ProjectMappers.ExtractNestedEntities(section);
                
                foreach (var entity in ecsEntities)
                {
                    // foreach (var x in await ProcessSectionAsync(entity, cancellationToken) ?? [])
                    // {
                    //     if (!x.Any())
                    //         continue;
                    //     await repo.InsertEntityAsync(projectId, section["EntityType"].AsString, entity, gridFsId);
                    // }
                }
            }

            // foreach (var section in ecsEntities)
            // {
            //     foreach (var entity in await ProcessSectionAsync(section, cancellationToken) ?? [])
            //     {
            //         if (!entity.Any())
            //             continue;
            //         await repo.InsertEntityAsync(projectId, section["EntityType"].AsString, entity, gridFsId);
            //     }
            // }
            // // Step 3: insert metadata
            // foreach (var section in metadataDocs)
            //     await repo.InsertEntityAsync(projectId, section["SectionType"].AsString, section, gridFsId);
            //
            // Console.WriteLine($"Inserted dummy entity for project {projectId}");

            // Simulate some work if needed
            await Task.Delay(500, cancellationToken);
        }
        else
        {
            Console.WriteLine($"Project file {projectId} not found in GridFS.");
        }
    }
    
    private static async Task<IEnumerable<BsonDocument>?> ProcessSectionAsync(BsonDocument ecsEntity, CancellationToken ct)
    {
        var sectionType = ecsEntity.GetValue("SectionType").AsString;
        
        var pipeline = PipelineRegistry.GetPipeline(sectionType);
        if (pipeline == null)
        {
            Console.WriteLine($"No pipeline registered for section {sectionType}");
            return null;
        }
        
        return await pipeline.ProcessAsync(ecsEntity, ct);
    }


}
