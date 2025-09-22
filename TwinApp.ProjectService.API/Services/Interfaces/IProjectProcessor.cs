namespace TwinApp.ProjectService.API.Services.Interfaces;

public interface IProjectProcessor
{
    Task ProcessAsync(string projectId, CancellationToken cancellationToken = default);
}