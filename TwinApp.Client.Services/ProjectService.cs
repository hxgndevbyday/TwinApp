using System.Net.Http.Json;
using System.Threading.Tasks;
using TwinApp.ProjectService.Shared.DTOs;

namespace TwinApp.Client.Services;

public class ProjectService
{
    private readonly HttpClient _httpClient;
    private List<ProjectDto>? _projects;

    public ProjectService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public string? ActiveProjectId { get; private set; }
    public bool HasActiveProject => ActiveProjectId is not null;
    public IReadOnlyList<ProjectDto>? Projects => _projects?.AsReadOnly();

    /// <summary>
    /// Loads projects metadata from the API.
    /// </summary>
    public async Task LoadProjectsAsync()
    {
        try
        {
            // Call your API to get project metadata
            var projectsFromApi = await _httpClient.GetFromJsonAsync<List<BfProgramMetadataDto>>("/projects");

            if (projectsFromApi != null)
            {
                // Map to DTO for UI use
                _projects = projectsFromApi
                    .Select(p => new ProjectDto(p.Id, p.ProgramName, p.CreatedAt))
                    .ToList();
            }
            else
            {
                _projects = new List<ProjectDto>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading projects: {ex.Message}");
            _projects = new List<ProjectDto>();
        }
    }

    public event Action? OnProjectChanged;
    public void OpenProject(string projectId)
    {
        ActiveProjectId = projectId;
        OnProjectChanged?.Invoke(); // notify subscribers

    }

    // Simple DTO
    // DTOs
    public record ProjectDto(string Id, string Name, DateTime CreatedAt);

    private record BfProgramMetadataDto(string Id, string ProgramName, DateTime CreatedAt);
}