using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using TwinApp.ProjectService.API.Models.Mappers;
using TwinApp.ProjectService.API.Services.Interfaces;
using TwinApp.ProjectService.Shared.DTOs;

namespace TwinApp.ProjectService.API.Services;

using MongoDB.Driver;
using TwinApp.ProjectService.API.Repository;

public class ProjectProcessor : IProjectProcessor
{
    private readonly ProjectRepository _repo;

    public ProjectProcessor(ProjectRepository repo)
    {
        _repo = repo;
    }

    public async Task ProcessAsync(string projectId, CancellationToken cancellationToken = default)
    {
        // Get the file from GridFS
        var gridFsId = await _repo.GetGridFsIdAsync(projectId);
        await using var stream = await _repo.GetGridFsBlobAsync(gridFsId);

        if (stream != null)
        {
            // Ensure the stream is at the beginning
            if (stream.CanSeek)
                stream.Position = 0;

            // Deserialize BSON directly to BsonDocument
            var projectDoc = BsonSerializer.Deserialize<BsonDocument>(stream);

            var (ecsEntities, metadataDocs) = ProjectMappers.DocumentToProjectEntities(projectDoc);

            // Step 1: insert top-level ECS entities
            foreach (var section in ecsEntities)
                await _repo.InsertEntityAsync(projectId, section["EntityType"].AsString, section, gridFsId);

            // Step 2: extract nested ECS entities (e.g., each machine)
            foreach (var section in ecsEntities)
            {
                var nested = ProjectMappers.ExtractNestedEntities(section);
                foreach (var entity in nested)
                    await _repo.InsertEntityAsync(projectId, entity["EntityType"].AsString, entity, gridFsId);
            }

            // Step 3: insert metadata
            foreach (var section in metadataDocs)
                await _repo.InsertEntityAsync(projectId, section["SectionType"].AsString, section, gridFsId);

            Console.WriteLine($"Inserted dummy entity for project {projectId}");

            // Simulate some work if needed
            await Task.Delay(500, cancellationToken);
        }
        else
        {
            Console.WriteLine($"Project file {projectId} not found in GridFS.");
        }
    }



}
