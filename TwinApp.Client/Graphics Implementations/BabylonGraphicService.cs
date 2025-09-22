using Microsoft.JSInterop;
using System.Reflection;
using TwinApp.Client.Shared;

namespace TwinApp.Client.Graphics_Implementations;

public class BabylonGraphicService : IGraphicService, IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private bool _jsInjected;
    private bool _initialized;

    public BabylonGraphicService(IJSRuntime js)
    {
        _js = js;
    }

    /// <summary>
    /// Injects the embedded babylonGraphics.js into the browser if not already.
    /// </summary>
    private async Task EnsureJsInjectedAsync()
    {
        // if (_jsInjected) return;
        //
        // var assembly = Assembly.GetExecutingAssembly();
        // var resourceName = "wwwroot/js/babylonGraphics.js";
        //
        // await using var stream = assembly.GetManifestResourceStream(resourceName)
        //                          ?? throw new Exception($"Embedded resource not found: {resourceName}");
        //
        // using var reader = new StreamReader(stream);
        // var jsContent = await reader.ReadToEndAsync();
        //
        // // Inject JS into page
        // await _js.InvokeVoidAsync("eval", jsContent);

        _jsInjected = true;
    }

    public async ValueTask InitializeAsync()
    {
        if (_initialized) return;
        await EnsureJsInjectedAsync();

        // Call JS init
        await _js.InvokeVoidAsync("window.TwinAppGraphics.initBabylon", "renderCanvas");
        _initialized = true;    }

    public async ValueTask LoadProjectAsync(string projectId)
    {
        await EnsureJsInjectedAsync();
        if (!_initialized)
            throw new InvalidOperationException("Babylon engine not initialized yet");
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