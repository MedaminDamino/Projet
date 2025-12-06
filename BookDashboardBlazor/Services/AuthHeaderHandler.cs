using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace BookDashboardBlazor.Services;

/// <summary>
/// Ensures every outgoing API request carries the bearer token stored in local storage.
/// </summary>
public class AuthHeaderHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;

    public AuthHeaderHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Only add if missing
        if (request.Headers.Authorization == null)
        {
            var token = await _localStorage.GetItemAsStringAsync("token");
            if (!string.IsNullOrWhiteSpace(token))
            {
                // Remove quotes and trim whitespace
                var cleanToken = token.Replace("\"", "").Trim();
                if (!string.IsNullOrWhiteSpace(cleanToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cleanToken);
                    
                    // Debug logging for Review endpoints
                    if (request.RequestUri?.ToString().Contains("/api/Review/") == true)
                    {
                        Console.WriteLine($"[AuthHeaderHandler] Added Bearer token to {request.Method} {request.RequestUri}");
                        Console.WriteLine($"[AuthHeaderHandler] Token length: {cleanToken.Length}, First 20 chars: {cleanToken.Substring(0, Math.Min(20, cleanToken.Length))}...");
                    }
                }
                else
                {
                    if (request.RequestUri?.ToString().Contains("/api/Review/") == true)
                    {
                        Console.WriteLine($"[AuthHeaderHandler] Token was empty after cleaning for {request.Method} {request.RequestUri}");
                    }
                }
            }
            else
            {
                if (request.RequestUri?.ToString().Contains("/api/Review/") == true)
                {
                    Console.WriteLine($"[AuthHeaderHandler] No token found in localStorage for {request.Method} {request.RequestUri}");
                }
            }
        }
        else
        {
            if (request.RequestUri?.ToString().Contains("/api/Review/") == true)
            {
                Console.WriteLine($"[AuthHeaderHandler] Authorization header already present for {request.Method} {request.RequestUri}");
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
