using MongoDB.Bson;
using TwinApp.ProjectService.API.Models;
using TwinApp.ProjectService.Shared.DTOs;

namespace TwinApp.ProjectService.API.Repository;

public interface IProjectRepository
{
    Task InsertEntityAsync(string projectId, string name, BsonDocument data, ObjectId gridFsId,string? parentId = null);
    Task<List<ProjectEntity>> GetEntitiesByProjectAsync(string projectId);
    Task<List<BfProgramMetadataDto>> GetAllAsync();
    Task<BfProgramMetadataDto?> GetByIdAsync(string id);
    Task CreateAsync(BfProgramMetadata project);
}