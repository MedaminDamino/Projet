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
        Console.WriteLine("=== [CustomAuthStateProvider] GetAuthenticationStateAsync called ===");
        
        string? token = await _localStorage.GetItemAsStringAsync("token");

        Console.WriteLine($"[CustomAuthStateProvider] Token from storage: {(string.IsNullOrEmpty(token) ? "NULL/EMPTY" : $"Length={token.Length}, First 50 chars: {token.Substring(0, Math.Min(50, token.Length))}...")}");

        var identity = new ClaimsIdentity();
        _http.DefaultRequestHeaders.Authorization = null;

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                // Remove quotes if the token was stored with them
                var cleanToken = token.Trim('"');
                Console.WriteLine($"[CustomAuthStateProvider] Clean token length: {cleanToken.Length}");
                
                var claims = ParseClaimsFromJwt(cleanToken);
                Console.WriteLine($"[CustomAuthStateProvider] Parsed {claims.Count()} claims from JWT");
                
                identity = new ClaimsIdentity(claims, "jwt");
                Console.WriteLine($"[CustomAuthStateProvider] Identity created. IsAuthenticated: {identity.IsAuthenticated}");
                
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
        else
        {
            Console.WriteLine("[CustomAuthStateProvider] No token found - user is anonymous");
        }

        var user = new ClaimsPrincipal(identity);
        
        // Log all claims
        Console.WriteLine($"[CustomAuthStateProvider] Final ClaimsPrincipal:");
        Console.WriteLine($"  - IsAuthenticated: {user.Identity?.IsAuthenticated}");
        Console.WriteLine($"  - Name: {user.Identity?.Name}");
        Console.WriteLine($"  - AuthenticationType: {user.Identity?.AuthenticationType}");
        Console.WriteLine($"  - Total claims: {user.Claims.Count()}");
        
        foreach (var claim in user.Claims)
        {
            Console.WriteLine($"  - Claim: Type='{claim.Type}', Value='{claim.Value}'");
        }
        
        // Specifically check for role claims
        var roleClaims = user.Claims.Where(c => 
            c.Type == ClaimTypes.Role || 
            c.Type == "role" || 
            c.Type == "roles" ||
            c.Type.Contains("role", StringComparison.OrdinalIgnoreCase)).ToList();
        Console.WriteLine($"[CustomAuthStateProvider] Role-related claims found: {roleClaims.Count}");
        foreach (var rc in roleClaims)
        {
            Console.WriteLine($"  - Role claim: Type='{rc.Type}', Value='{rc.Value}'");
        }
        
        // Test IsInRole
        Console.WriteLine($"[CustomAuthStateProvider] user.IsInRole('SuperAdmin'): {user.IsInRole("SuperAdmin")}");
        Console.WriteLine($"[CustomAuthStateProvider] user.IsInRole('Admin'): {user.IsInRole("Admin")}");
        Console.WriteLine($"[CustomAuthStateProvider] user.IsInRole('User'): {user.IsInRole("User")}");
        
        Console.WriteLine("=== [CustomAuthStateProvider] GetAuthenticationStateAsync complete ===");

        var state = new AuthenticationState(user);
        return state;
    }

    public void NotifyUserAuthentication(string token)
    {
        Console.WriteLine("=== [CustomAuthStateProvider] NotifyUserAuthentication called ===");
        Console.WriteLine($"[CustomAuthStateProvider] Token length: {token.Length}");
        
        var cleanToken = token.Trim('"');
        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", cleanToken);
        
        var claims = ParseClaimsFromJwt(cleanToken);
        Console.WriteLine($"[CustomAuthStateProvider] Parsed {claims.Count()} claims");
        
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        
        Console.WriteLine($"[CustomAuthStateProvider] After login - IsInRole('SuperAdmin'): {authenticatedUser.IsInRole("SuperAdmin")}");
        
        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        NotifyAuthenticationStateChanged(authState);
        
        Console.WriteLine("=== [CustomAuthStateProvider] NotifyUserAuthentication complete ===");
    }

    public void NotifyUserLogout()
    {
        Console.WriteLine("=== [CustomAuthStateProvider] NotifyUserLogout called ===");
        _http.DefaultRequestHeaders.Authorization = null;
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = Task.FromResult(new AuthenticationState(anonymousUser));
        NotifyAuthenticationStateChanged(authState);
    }

    public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        Console.WriteLine("=== [ParseClaimsFromJwt] Starting JWT parsing ===");
        
        var claims = new List<Claim>();
        var parts = jwt.Split('.');
        
        Console.WriteLine($"[ParseClaimsFromJwt] JWT has {parts.Length} parts");
        
        if (parts.Length < 2)
        {
            Console.WriteLine("[ParseClaimsFromJwt] ERROR: JWT has less than 2 parts!");
            return claims;
        }

        var payload = parts[1];
        Console.WriteLine($"[ParseClaimsFromJwt] Payload (base64) length: {payload.Length}");
        
        byte[] jsonBytes;
        try
        {
            jsonBytes = ParseBase64WithoutPadding(payload);
            Console.WriteLine($"[ParseClaimsFromJwt] Decoded payload length: {jsonBytes.Length} bytes");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ParseClaimsFromJwt] ERROR decoding base64: {ex.Message}");
            return claims;
        }
        
        // Log raw JSON payload
        var jsonString = System.Text.Encoding.UTF8.GetString(jsonBytes);
        Console.WriteLine($"[ParseClaimsFromJwt] RAW JWT PAYLOAD JSON:");
        Console.WriteLine(jsonString);
        
        Dictionary<string, object>? keyValuePairs;
        try
        {
            keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ParseClaimsFromJwt] ERROR deserializing JSON: {ex.Message}");
            return claims;
        }

        if (keyValuePairs != null)
        {
            Console.WriteLine($"[ParseClaimsFromJwt] Found {keyValuePairs.Count} keys in JWT payload:");
            
            foreach (var kvp in keyValuePairs)
            {
                var originalType = kvp.Key;
                var claimType = MapClaimType(kvp.Key);
                var wasMapped = originalType != claimType;
                
                Console.WriteLine($"  - Key: '{originalType}' -> Mapped to: '{claimType}' (mapped: {wasMapped})");

                if (kvp.Value is JsonElement element)
                {
                    Console.WriteLine($"    JsonElement ValueKind: {element.ValueKind}");
                    
                    if (element.ValueKind == JsonValueKind.Array)
                    {
                        Console.WriteLine($"    Array with {element.GetArrayLength()} items:");
                        foreach (var item in element.EnumerateArray())
                        {
                            var value = item.ToString();
                            Console.WriteLine($"      - Array item: '{value}'");
                            if (!string.IsNullOrEmpty(value))
                            {
                                claims.Add(new Claim(claimType, value));
                                Console.WriteLine($"      -> Added claim: Type='{claimType}', Value='{value}'");
                            }
                        }
                    }
                    else
                    {
                        var value = element.ToString();
                        Console.WriteLine($"    Value: '{value}'");
                        if (!string.IsNullOrEmpty(value))
                        {
                            claims.Add(new Claim(claimType, value));
                            Console.WriteLine($"    -> Added claim: Type='{claimType}', Value='{value}'");
                        }
                    }
                }
                else if (kvp.Value != null)
                {
                    var value = kvp.Value.ToString();
                    Console.WriteLine($"    Non-JsonElement value: '{value}'");
                    if (!string.IsNullOrEmpty(value))
                    {
                        claims.Add(new Claim(claimType, value));
                        Console.WriteLine($"    -> Added claim: Type='{claimType}', Value='{value}'");
                    }
                }
            }
        }
        else
        {
            Console.WriteLine("[ParseClaimsFromJwt] keyValuePairs is null!");
        }

        Console.WriteLine($"=== [ParseClaimsFromJwt] Complete. Total claims: {claims.Count} ===");
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

