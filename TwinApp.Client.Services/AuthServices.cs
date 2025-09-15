using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace TwinApp.Client.Services;

public class AuthServices
{
    private readonly IAccessTokenProvider _tokenProvider;

    public AuthServices(IAccessTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    public async Task<bool> IsLoggedInAsync()
    {
        var result = await _tokenProvider.RequestAccessToken();
        return result.TryGetToken(out _);
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        var result = await _tokenProvider.RequestAccessToken();
        if (result.TryGetToken(out var token))
            return token.Value;

        return null;
    }
}