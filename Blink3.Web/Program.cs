using Blink3.Web;
using Blink3.Web.Configuration;
using Blink3.Web.Configuration.Extensions;
using Blink3.Web.Extensions;
using Blink3.Web.Interfaces;
using Blink3.Web.Repositories;
using Blink3.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddBlazorBootstrap();

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddAppConfiguration(builder.Configuration);
AppOptions appConfig = builder.Services.GetAppConfiguration();

builder.Services.AddTransient<CookieHandler>();
builder.Services.AddScoped(sp => sp
        .GetRequiredService<IHttpClientFactory>()
        .CreateClient("API"))
    .AddHttpClient("API", client => client.BaseAddress = new Uri(appConfig.ApiAddress))
    .AddHttpMessageHandler<CookieHandler>();

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

builder.Services.AddScoped<ITodoHttpRepository, TodoHttpRepository>();
builder.Services.AddScoped<IBlinkGuildHttpRepository, BlinkGuildHttpRepository>();

builder.Services.AddScoped<IBlinkGuildConfigService, BlinkGuildConfigService>();
builder.Services.AddScoped<IDiscordGuildService, DiscordGuildService>();

await builder.Build().RunAsync();