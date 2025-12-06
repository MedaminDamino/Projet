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
        // Always try to add the token if it's missing
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
                    
                    // Debug logging for all requests (especially Review endpoints)
                    var uri = request.RequestUri?.ToString() ?? request.RequestUri?.OriginalString ?? "unknown";
                    var isReviewEndpoint = uri.Contains("/api/Review/", StringComparison.OrdinalIgnoreCase) || 
                                          uri.Contains("Review/", StringComparison.OrdinalIgnoreCase);
                    
                    if (isReviewEndpoint)
                    {
                        Console.WriteLine($"[AuthHeaderHandler] Added Bearer token to {request.Method} {uri}");
                        Console.WriteLine($"[AuthHeaderHandler] Token length: {cleanToken.Length}, First 20 chars: {cleanToken.Substring(0, Math.Min(20, cleanToken.Length))}...");
                    }
                }
                else
                {
                    var uri = request.RequestUri?.ToString() ?? request.RequestUri?.OriginalString ?? "unknown";
                    var isReviewEndpoint = uri.Contains("/api/Review/", StringComparison.OrdinalIgnoreCase) || 
                                          uri.Contains("Review/", StringComparison.OrdinalIgnoreCase);
                    if (isReviewEndpoint)
                    {
                        Console.WriteLine($"[AuthHeaderHandler] Token was empty after cleaning for {request.Method} {uri}");
                    }
                }
            }
            else
            {
                var uri = request.RequestUri?.ToString() ?? request.RequestUri?.OriginalString ?? "unknown";
                var isReviewEndpoint = uri.Contains("/api/Review/", StringComparison.OrdinalIgnoreCase) || 
                                      uri.Contains("Review/", StringComparison.OrdinalIgnoreCase);
                if (isReviewEndpoint)
                {
                    Console.WriteLine($"[AuthHeaderHandler] No token found in localStorage for {request.Method} {uri}");
                }
            }
        }
        else
        {
            var uri = request.RequestUri?.ToString() ?? request.RequestUri?.OriginalString ?? "unknown";
            var isReviewEndpoint = uri.Contains("/api/Review/", StringComparison.OrdinalIgnoreCase) || 
                                  uri.Contains("Review/", StringComparison.OrdinalIgnoreCase);
            if (isReviewEndpoint)
            {
                Console.WriteLine($"[AuthHeaderHandler] Authorization header already present for {request.Method} {uri}");
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
