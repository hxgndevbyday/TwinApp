using MongoDB.Bson;

namespace TwinApp.ProjectService.API.Models;

public class ProjectEntity
{
    public ObjectId Id { get; set; }
    public string ProjectId { get; set; } = null!;
    public string ParentId { get; set; } = null!; // for hierarchical structure
    public string Name { get; set; } = null!;
    public BsonDocument Data { get; set; } = null!;
}
