using BookDashboardBlazor.Core.Services.Api;
using BookDashboardBlazor.Core.Services.Interfaces;
using BookDashboardBlazor.Core.Services.State;

namespace BookDashboardBlazor.Shared.Extensions;

/// <summary>
/// Extension methods for service registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all application services.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // API Services
        services.AddScoped<IBookService, BookApiService>();
        services.AddScoped<IAuthorService, AuthorApiService>();
        services.AddScoped<IGenreService, GenreApiService>();

        // State Management
        services.AddSingleton<AppStateService>();

        return services;
    }

    /// <summary>
    /// Registers authentication and authorization services.
    /// </summary>
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
    {
        services.AddAuthorizationCore();
        
        // Note: AuthenticationStateProvider and AuthService will be registered here
        // once we move them to the new structure
        
        return services;
    }
}
