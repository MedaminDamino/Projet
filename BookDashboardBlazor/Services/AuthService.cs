using System.Net.Http.Json;
using Blazored.LocalStorage;
using BookDashboardBlazor.Models;
using Microsoft.AspNetCore.Components.Authorization;

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
                ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Token);
                return true;
            }
        }
        return false;
    }

    public async Task<bool> Register(RegisterModel registerModel)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Account/register", registerModel);
        return response.IsSuccessStatusCode;
    }

    public async Task Logout()
    {
        await _localStorage.RemoveItemAsync("token");
        ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
}
