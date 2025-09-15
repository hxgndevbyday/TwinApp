using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TwinApp.Client;
using TwinApp.Client.Graphics;
using TwinApp.Client.Services;
using TwinApp.Client.Shared;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<AuthServices>();
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<IGraphicService, BabylonGraphicService>();



// --- Add MSAL Authentication ---
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);

    // Optional: add scopes for your backend API
    // options.ProviderOptions.DefaultAccessTokenScopes.Add("api://268c834d-59bf-4b45-8891-86245beca87d/.default");
});


await builder.Build().RunAsync();