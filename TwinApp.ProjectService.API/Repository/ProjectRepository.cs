using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using TwinApp.ProjectService.API.Models;
using TwinApp.ProjectService.Shared.DTOs;

namespace TwinApp.ProjectService.API.Repository;

using MongoDB.Driver;

public class ProjectRepository : IProjectRepository
{
    private readonly IMongoCollection<BfProgramMetadata> _projects;
    private readonly GridFSBucket _gridFs;
    
    private readonly IMongoCollection<ProjectEntity> _entities;
    
    public ProjectRepository(IConfiguration config)
    {
        var mongoSettings = config.GetSection("MongoDb");
        var client = new MongoClient(mongoSettings["ConnectionString"]);
        var database = client.GetDatabase(mongoSettings["DatabaseName"]);
        
        _projects = database.GetCollection<BfProgramMetadata>(mongoSettings["ProjectsCollection"]);
        _gridFs = new GridFSBucket(database);
        
        _entities = database.GetCollection<ProjectEntity>("ProjectEntitiesCollection");
    }

    #region Process Projects

    public async Task InsertEntityAsync(string projectId, string name, BsonDocument data, ObjectId gridFsId,string? parentId = null)
    {
        var entity = new ProjectEntity
        (
            Id: Guid.NewGuid().ToString(),
            ProjectId: projectId,
            Name: name,
            Data: data,
            ParentId: parentId ?? string.Empty,
            GridFsId: gridFsId
        );
    
        await _entities.InsertOneAsync(entity);
    }

    public async Task<List<ProjectEntity>> GetEntitiesByProjectAsync(string projectId)
    {
        return await _entities.Find(e => e.ProjectId == projectId).ToListAsync();
    }


    #endregion Process Projects 
    public async Task<List<BfProgramMetadataDto>> GetAllAsync()
    {
        var readProjects=await _projects.Find(_ => true).ToListAsync();
        
        var result = new List<BfProgramMetadataDto>();
        foreach (var project in readProjects)
        {
            result.Add(new BfProgramMetadataDto(project.Id, project.ProgramName, project.CreatedAt));
        }
        return result;
    }

    public async Task<BfProgramMetadataDto?> GetByIdAsync(string id)
    {
        var readProjects = await _projects.Find(p => p.Id == id).FirstOrDefaultAsync();
        
        var result = new BfProgramMetadataDto(readProjects.Id, readProjects.ProgramName, readProjects.CreatedAt);
        return result;
    }

    public async Task<ObjectId> GetGridFsIdAsync(string id)
    {
        var readProjects = await _projects.Find(p => p.Id == id).FirstOrDefaultAsync();

        return readProjects.GridFsId;
    }

    public async Task CreateAsync(BfProgramMetadata project) =>
        await _projects.InsertOneAsync(project);
    
    public async Task<ObjectId> CreateGridFsBlobAsync(string fileName, Stream fileStream)
    {
        // Upload file to GridFS and return id
        return await _gridFs.UploadFromStreamAsync(fileName, fileStream);
    }
    public async Task<Stream?> GetGridFsBlobAsync(ObjectId gridFsId)
    {
        try
        {
            var stream = new MemoryStream();
            await _gridFs.DownloadToStreamAsync(gridFsId, stream);
            stream.Position = 0;
            return stream;
        }
        catch
        {
            return null;
        }
    }

}

