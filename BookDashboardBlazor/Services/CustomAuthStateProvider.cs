using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace BookDashboardBlazor.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _http;

    public CustomAuthStateProvider(ILocalStorageService localStorage, HttpClient http)
    {
        _localStorage = localStorage;
        _http = http;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string? token = await _localStorage.GetItemAsStringAsync("token");

        var identity = new ClaimsIdentity();
        _http.DefaultRequestHeaders.Authorization = null;

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
            }
            catch
            {
                await _localStorage.RemoveItemAsync("token");
                identity = new ClaimsIdentity();
            }
        }

        var user = new ClaimsPrincipal(identity);
        var state = new AuthenticationState(user);

        return state;
    }

    public void NotifyUserAuthentication(string token)
    {
        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        NotifyAuthenticationStateChanged(authState);
    }

    public void NotifyUserLogout()
    {
        _http.DefaultRequestHeaders.Authorization = null;
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = Task.FromResult(new AuthenticationState(anonymousUser));
        NotifyAuthenticationStateChanged(authState);
    }

    public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        try
        {
            var parts = jwt.Split('.');
            if (parts.Length < 2)
            {
                return new List<Claim>();
            }
            var payload = parts[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            if (keyValuePairs == null)
            {
                return new List<Claim>();
            }
            return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value?.ToString() ?? ""));
        }
        catch
        {
            return new List<Claim>();
        }
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
