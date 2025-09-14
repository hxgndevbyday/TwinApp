using Microsoft.JSInterop;
using System.Reflection;
using TwinApp.Client.Shared;

namespace TwinApp.Client.Graphics;

public class BabylonGraphicService : IGraphicService, IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private bool _jsInjected;

    public BabylonGraphicService(IJSRuntime js)
    {
        _js = js;
    }

    /// <summary>
    /// Injects the embedded babylonGraphics.js into the browser if not already.
    /// </summary>
    private async Task EnsureJsInjectedAsync()
    {
        if (_jsInjected) return;

        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "TwinApp.Client.Graphics.wwwroot.js.babylonGraphics.js";

        await using var stream = assembly.GetManifestResourceStream(resourceName)
                                 ?? throw new Exception($"Embedded resource not found: {resourceName}");

        using var reader = new StreamReader(stream);
        var jsContent = await reader.ReadToEndAsync();

        // Inject JS into page
        await _js.InvokeVoidAsync("eval", jsContent);

        _jsInjected = true;
    }

    public async ValueTask InitializeAsync()
    {
        await EnsureJsInjectedAsync();

        // Call initBabylon on the global object
        await _js.InvokeVoidAsync("window.TwinAppGraphics.initBabylon", "renderCanvas");
    }

    public async ValueTask LoadProjectAsync(string projectId)
    {
        await EnsureJsInjectedAsync();
        await _js.InvokeVoidAsync("window.TwinAppGraphics.loadProject", projectId);
    }

    public async ValueTask AddAssetAsync(string assetId, string modelPath)
    {
        await EnsureJsInjectedAsync();
        await _js.InvokeVoidAsync("window.TwinAppGraphics.addAsset", assetId, modelPath);
    }

    public async ValueTask ClearSceneAsync()
    {
        await EnsureJsInjectedAsync();
        await _js.InvokeVoidAsync("window.TwinAppGraphics.clearScene");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}