using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TwinApp.Client;
using TwinApp.Client.Graphics_Implementations;
using TwinApp.Client.Services;
using TwinApp.Client.Shared;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.Services.AddScoped(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var projectServiceUrl = config["ProjectServiceUrl"];
    return new HttpClient
    {
        BaseAddress = new Uri(projectServiceUrl)
    };
});

builder.Services.AddScoped<AuthServices>();
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<IGraphicService, BabylonGraphicService>();



// --- Add MSAL Authentication ---
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    options.ProviderOptions.LoginMode = "quiet";
    // Optional: add scopes for your backend API
    // options.ProviderOptions.DefaultAccessTokenScopes.Add("api://268c834d-59bf-4b45-8891-86245beca87d/.default");
});


await builder.Build().RunAsync();