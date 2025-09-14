using Microsoft.JSInterop;
using TwinApp.Client.Shared;

namespace TwinApp.Client.Graphics;

public class BabylonGraphicService : IGraphicService
{
    private readonly IJSRuntime _js;

    public BabylonGraphicService(IJSRuntime js)
    {
        _js = js;
    }

    public ValueTask InitializeAsync()
        => _js.InvokeVoidAsync("initBabylon");

    public ValueTask LoadProjectAsync(string projectId)
        => _js.InvokeVoidAsync("loadProject", projectId);

    public ValueTask AddAssetAsync(string assetId, string modelPath)
        => _js.InvokeVoidAsync("addAsset", assetId, modelPath);

    public ValueTask ClearSceneAsync()
        => _js.InvokeVoidAsync("clearScene");
}