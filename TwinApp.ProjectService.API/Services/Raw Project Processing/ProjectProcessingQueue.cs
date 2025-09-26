using System.Threading.Channels;
using TwinApp.ProjectService.API.Services.Interfaces;

namespace TwinApp.ProjectService.API.Services.Raw_Project_Processing;

public class ProjectProcessingQueue(IProjectProcessor processor) : BackgroundService
{
    private readonly SemaphoreSlim _concurrency = new SemaphoreSlim(3); // max 3 projects at a time
    private readonly Channel<string> _queue = Channel.CreateUnbounded<string>();

    public async Task Enqueue(string projectId) => await _queue.Writer.WriteAsync(projectId);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var projectId in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            await _concurrency.WaitAsync(stoppingToken);
            try
            {
                await processor.ProcessAsync(projectId, stoppingToken);
            }
            finally
            {
                _concurrency.Release();
            }
        }
    }
}
