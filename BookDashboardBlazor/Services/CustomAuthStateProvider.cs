using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace BookDashboardBlazor.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _http;

    // Map JWT claim type names to standard .NET ClaimTypes
    private static readonly Dictionary<string, string> ClaimTypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "role", ClaimTypes.Role },
        { "roles", ClaimTypes.Role },
        { JwtRegisteredClaimNames.Sub, ClaimTypes.NameIdentifier },
        { "nameid", ClaimTypes.NameIdentifier },
        { "name", ClaimTypes.Name },
        { "unique_name", ClaimTypes.Name },
        { "email", ClaimTypes.Email },
        { "given_name", ClaimTypes.GivenName },
        { "family_name", ClaimTypes.Surname },
    };

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
                // Remove quotes if the token was stored with them
                var cleanToken = token.Trim('"');
                var claims = ParseClaimsFromJwt(cleanToken);
                identity = new ClaimsIdentity(claims, "jwt");
                
                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", cleanToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CustomAuthStateProvider] ERROR parsing token: {ex.Message}");
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
        var cleanToken = token.Trim('"');
        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", cleanToken);
        
        var claims = ParseClaimsFromJwt(cleanToken);
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        
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
        var claims = new List<Claim>();
        var parts = jwt.Split('.');

        if (parts.Length < 2)
        {
            return claims;
        }

        var payload = parts[1];
        
        byte[] jsonBytes;
        try
        {
            jsonBytes = ParseBase64WithoutPadding(payload);
        }
        catch (Exception)
        {
            return claims;
        }
        
        Dictionary<string, object>? keyValuePairs;
        try
        {
            keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
        }
        catch (Exception)
        {
            return claims;
        }

        if (keyValuePairs != null)
        {
            foreach (var kvp in keyValuePairs)
            {
                var claimType = MapClaimType(kvp.Key);

                if (kvp.Value is JsonElement element)
                {
                    if (element.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in element.EnumerateArray())
                        {
                            var value = item.ToString();
                            if (!string.IsNullOrEmpty(value))
                            {
                                claims.Add(new Claim(claimType, value));
                            }
                        }
                    }
                    else
                    {
                        var value = element.ToString();
                        if (!string.IsNullOrEmpty(value))
                        {
                            claims.Add(new Claim(claimType, value));
                        }
                    }
                }
                else if (kvp.Value != null)
                {
                    var value = kvp.Value.ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        claims.Add(new Claim(claimType, value));
                    }
                }
            }
        }

        return claims;
    }

    /// <summary>
    /// Maps JWT claim type names to standard .NET ClaimTypes.
    /// This is critical for role authorization to work correctly.
    /// </summary>
    private static string MapClaimType(string jwtClaimType)
    {
        if (ClaimTypeMap.TryGetValue(jwtClaimType, out var mappedType))
        {
            return mappedType;
        }
        return jwtClaimType;
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

