using System.Text.Json;
using MongoDB.Bson;
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
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync(cancellationToken);

            // Parse the JSON as a BsonDocument
            var projectDoc = BsonDocument.Parse(json);

            // Safe extraction
            string GetString(BsonDocument doc, string key) =>
                doc.Contains(key) ? doc[key].ToString() : string.Empty;

            var dummyData = new BsonDocument
            {
                { "Id", GetString(projectDoc, "Id") },
                { "Name", GetString(projectDoc, "Name") },
                { "Type", GetString(projectDoc, "Type") },
                { "Version", GetString(projectDoc, "Version") }
            };

            // Persist this dummy entity
            await _repo.InsertEntityAsync(projectId, "DummyTopLevelEntity", dummyData);

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
