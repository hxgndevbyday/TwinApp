using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using TwinApp.ProjectService.API.Models;
using TwinApp.ProjectService.API.Models.Mappers;
using TwinApp.ProjectService.API.Repository;
using TwinApp.ProjectService.API.Services;
using TwinApp.ProjectService.API.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClientApp",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// TODO: Add authentication, MongoDB, logging, etc.
builder.Services.AddSingleton<ProjectRepository>();
builder.Services.AddSingleton<IProjectProcessor, ProjectProcessor>();
// Register the queue as a hosted service
builder.Services.AddSingleton<ProjectProcessingQueue>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<ProjectProcessingQueue>());




var app = builder.Build();

app.UseCors("AllowClientApp");

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

// Upload the project
app.MapPost("/projects/upload", async (IFormFile file, ProjectRepository repo) =>
{
    if (file == null || file.Length == 0)
        return Results.BadRequest("Empty file.");

    var fileName = Path.GetFileName(file.FileName);

    // Convert JSON to BSON stream
    var bsonStream = ProjectMappers.FileToBsonDoc(file);

    // Upload BSON to GridFS
    var fsId = await repo.CreateGridFsBlobAsync(fileName, bsonStream);
    
    var project = new BfProgramMetadata(
        Id: Guid.NewGuid().ToString(),
        ProgramName: fileName,
        CreatedAt: DateTime.UtcNow,
        GridFsId:fsId
    );

    await repo.CreateAsync(project);

    return Results.Created($"/projects/{project.Id}", project);
    
}).DisableAntiforgery();


// Download the project
app.MapGet("/projects/{id}/download", async (string id, ProjectRepository repo) =>
{
    var project = await repo.GetByIdAsync(id);
    if (project == null)
        return Results.NotFound();
    
    var gridFsId = await repo.GetGridFsIdAsync(id);
    var bsonStream = await repo.GetGridFsBlobAsync(gridFsId);
    if (bsonStream == null)
        return Results.NotFound("File not found in GridFS.");

    // Convert BSON → JSON stream
    var jsonStream = ProjectMappers.BsonToJsonStreamAsync(bsonStream);

    return Results.File(jsonStream, "application/bf", project.ProgramName.Replace(".json", ".bf"));
});

// Trigger project processing
app.MapPost("/projects/{id}/process", async (string id,  [FromServices] ProjectRepository repo,  [FromServices] ProjectProcessingQueue queue) =>
{
    // Validate project exists
    var project = await repo.GetByIdAsync(id);
    if (project == null)
        return Results.NotFound($"Project {id} not found.");

    // Enqueue for background processing
    await queue.Enqueue(id);

    return Results.Accepted($"/projects/{id}/process", $"Project {id} enqueued for processing.");
});







app.Run();

