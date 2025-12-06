using System.Net.Http.Json;
using Blazored.LocalStorage;
using BookDashboardBlazor.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;

namespace BookDashboardBlazor.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly ILocalStorageService _localStorage;

    public AuthService(HttpClient httpClient, AuthenticationStateProvider authStateProvider, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _authStateProvider = authStateProvider;
        _localStorage = localStorage;
    }

    public async Task<bool> Login(LoginModel loginModel)
    {
        Console.WriteLine($"=== [AuthService] Login called for user: {loginModel.Username} ===");
        
        var response = await _httpClient.PostAsJsonAsync("api/Account/login", loginModel);
        
        Console.WriteLine($"[AuthService] Login response status: {response.StatusCode}");
        
        // Log raw response for debugging
        var rawResponse = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"[AuthService] Raw login response:");
        Console.WriteLine(rawResponse);
        
        if (response.IsSuccessStatusCode)
        {
            // Re-read the response (need to create a new response since we already read it)
            var result = System.Text.Json.JsonSerializer.Deserialize<LoginResponse>(rawResponse, 
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (result != null && !string.IsNullOrEmpty(result.Token))
            {
                Console.WriteLine($"[AuthService] Token received. Length: {result.Token.Length}");
                Console.WriteLine($"[AuthService] Token (first 100 chars): {result.Token.Substring(0, Math.Min(100, result.Token.Length))}...");
                
                await _localStorage.SetItemAsync("token", result.Token);
                Console.WriteLine("[AuthService] Token saved to localStorage");
                
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", result.Token.Replace("\"", ""));
                    
                Console.WriteLine("[AuthService] Calling NotifyUserAuthentication...");
                ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Token);
                
                Console.WriteLine("=== [AuthService] Login complete - returning true ===");
                return true;
            }
            else
            {
                Console.WriteLine("[AuthService] ERROR: Could not deserialize login response or token is empty!");
            }
        }
        else
        {
            Console.WriteLine($"[AuthService] Login failed with status: {response.StatusCode}");
        }
        
        Console.WriteLine("=== [AuthService] Login complete - returning false ===");
        return false;
    }

    public async Task<RegistrationResult> Register(RegisterModel registerModel)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Account/register", registerModel);
        if (response.IsSuccessStatusCode)
        {
            return new RegistrationResult
            {
                Success = true,
                Message = "Registration successful"
            };
        }

        ApiResponse<List<string>>? apiError = null;
        List<string> errors = new();
        string message = "Registration failed";

        try
        {
            apiError = await response.Content.ReadFromJsonAsync<ApiResponse<List<string>>>();
            if (apiError != null)
            {
                message = string.IsNullOrWhiteSpace(apiError.Message) ? message : apiError.Message;
                errors = apiError.Data ?? new List<string>();
            }
        }
        catch
        {
            // If the response isn't JSON, fall back to raw text
            var raw = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(raw))
            {
                errors.Add(raw);
            }
        }

        if (!errors.Any())
        {
            var raw = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(raw))
            {
                errors.Add(raw);
            }
        }

        return new RegistrationResult
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }

    public async Task Logout()
    {
        Console.WriteLine("=== [AuthService] Logout called ===");
        await _localStorage.RemoveItemAsync("token");
        ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
        _httpClient.DefaultRequestHeaders.Authorization = null;
        Console.WriteLine("=== [AuthService] Logout complete ===");
    }
}
