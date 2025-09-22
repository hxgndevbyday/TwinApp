using System.Net.Http.Json;
using System.Threading.Tasks;
using TwinApp.ProjectService.Shared.DTOs;

namespace TwinApp.Client.Services;

public class ProjectService
{
    private readonly HttpClient _httpClient;
    private List<ProjectDto>? _projects;
    public event Action? OnProjectsUpdated;
    public ProjectService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public string? ActiveProjectId { get; private set; }
    public bool HasActiveProject => ActiveProjectId is not null;
    public IReadOnlyList<ProjectDto>? Projects => _projects?.AsReadOnly();

    /// <summary>
    /// Uploads a new project file and refreshes the project list.
    /// </summary>
    public async Task<ProjectDto?> UploadProjectAsync(Stream fileStream, string fileName)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(fileStream), "file", fileName);

        var response = await _httpClient.PostAsync("/projects/upload", content);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Upload failed: {response.StatusCode}");
            return null;
        }

        var created = await response.Content.ReadFromJsonAsync<BfProgramMetadataDto>();
        if (created != null)
        {
            var dto = new ProjectDto(created.Id, created.ProgramName, created.CreatedAt);
            _projects ??= new List<ProjectDto>();
            _projects.Add(dto);
            return dto;
        }

        return null;
    }
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
                OnProjectsUpdated?.Invoke();
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

    // DTOs
    public record ProjectDto(string Id, string Name, DateTime CreatedAt);
    private record BfProgramMetadataDto(string Id, string ProgramName, DateTime CreatedAt);
}