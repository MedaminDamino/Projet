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
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
