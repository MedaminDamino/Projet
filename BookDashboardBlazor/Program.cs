using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BookDashboardBlazor;
using BookDashboardBlazor.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<AuthHeaderHandler>();
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AuthHeaderHandler>();
    handler.InnerHandler = new HttpClientHandler();
    return new HttpClient(handler)
    {
        BaseAddress = new Uri("http://localhost:5069/")
    };
});
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<BookPaginationService>();
builder.Services.AddScoped<BookDiscoverService>();
builder.Services.AddScoped<ReadingListService>();

await builder.Build().RunAsync();
