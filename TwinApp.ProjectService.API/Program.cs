using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using TwinApp.ProjectService.API.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// TODO: Add authentication, MongoDB, logging, etc.
builder.Services.AddSingleton<ProjectRepository>();






var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Health
app.MapGet("/health", () => Results.Ok("ProjectService API is running."));

// Get all projects
app.MapGet("/projects", async (ProjectRepository repo) =>
{
    var projects = await repo.GetAllAsync();
    return Results.Ok(projects);
});

// Get single project
app.MapGet("/projects/{id}", async (string id, ProjectRepository repo) =>
{
    var project = await repo.GetByIdAsync(id);
    return project is not null ? Results.Ok(project) : Results.NotFound();
});

// Create project
app.MapPost("/projects", async (BfProject newProject, ProjectRepository repo) =>
{
    var project = newProject with { CreatedAt = DateTime.UtcNow };
    await repo.CreateAsync(project);
    return Results.Created($"/projects/{project.Id}", project);
});






app.Run();

// Record definition
public record BfProject(
    [property: BsonId, BsonRepresentation(BsonType.String)] string Id,
    string Name,
    string OwnerUserId,
    DateTime CreatedAt,
    string ContentJson // raw .bf JSON for now
);