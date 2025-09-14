namespace TwinApp.Client.Shared;

public interface IGraphicService
{
    ValueTask InitializeAsync();
    ValueTask LoadProjectAsync(string projectId);
    ValueTask AddAssetAsync(string assetId, string modelPath);
    ValueTask ClearSceneAsync();
}
