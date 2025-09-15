namespace TwinApp.ProjectService.API.Repository;

using MongoDB.Driver;

public class ProjectRepository
{
    private readonly IMongoCollection<BfProject> _projects;

    public ProjectRepository(IConfiguration config)
    {
        var mongoSettings = config.GetSection("MongoDb");
        var client = new MongoClient(mongoSettings["ConnectionString"]);
        var database = client.GetDatabase(mongoSettings["DatabaseName"]);
        _projects = database.GetCollection<BfProject>(mongoSettings["ProjectsCollection"]);
    }

    public async Task<List<BfProject>> GetAllAsync() =>
        await _projects.Find(_ => true).ToListAsync();

    public async Task<BfProject?> GetByIdAsync(string id) =>
        await _projects.Find(p => p.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(BfProject project) =>
        await _projects.InsertOneAsync(project);
}
