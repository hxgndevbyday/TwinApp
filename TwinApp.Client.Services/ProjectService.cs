using System.Threading.Tasks;

namespace TwinApp.Client.Services;

public class ProjectService
{
    private List<ProjectDto>? _projects;

    public string? ActiveProjectId { get; private set; }

    public bool HasActiveProject => ActiveProjectId is not null;

    public IReadOnlyList<ProjectDto>? Projects => _projects?.AsReadOnly();

    public async Task LoadProjectsAsync()
    {
        // Simulate network delay
        await Task.Delay(500);

        // Mock project list
        _projects = new List<ProjectDto>
        {
            new ProjectDto("1", "Factory A"),
            new ProjectDto("2", "Warehouse B"),
            new ProjectDto("3", "Plant C")
        };
    }

    public event Action? OnProjectChanged;
    public void OpenProject(string projectId)
    {
        ActiveProjectId = projectId;
        OnProjectChanged?.Invoke(); // notify subscribers

    }

    // Simple DTO
    public record ProjectDto(string Id, string Name);
}