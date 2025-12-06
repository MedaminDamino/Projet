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
        var response = await _httpClient.PostAsJsonAsync("api/Account/login", loginModel);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result != null)
            {
                await _localStorage.SetItemAsync("token", result.Token);
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", result.Token.Replace("\"", ""));
                ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Token);
                return true;
            }
        }
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
        await _localStorage.RemoveItemAsync("token");
        ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
}
