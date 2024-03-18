using System.Security.Claims;
using AspNet.Security.OAuth.Discord;
using Blink3.API.Services;
using Blink3.Common.Caching.Extensions;
using Blink3.Common.Configuration;
using Blink3.Common.Configuration.Extensions;
using Blink3.DataAccess.Extensions;
using Discord.Rest;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
    .WriteTo.Console()
    .CreateLogger();

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Host.UseSerilog();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Add Application configurations
    builder.Services.AddAppConfiguration(builder.Configuration);
    BlinkConfiguration appConfig = builder.Services.GetAppConfiguration();

    // Add Data Access layer and cache provider
    builder.Services.AddDataAccess(appConfig);
    builder.Services.AddCaching(appConfig);

    // Add Discord Rest Client and startup service
    builder.Services.AddSingleton<DiscordRestClient>();
    builder.Services.AddHostedService<DiscordStartupService>();

    // Configure Authentication and Discord OAuth
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOAuth(DiscordAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.ClientId = appConfig.Discord.ClientId;
        options.ClientSecret = appConfig.Discord.ClientSecret;
        options.CallbackPath = new PathString("/api/auth/callback");

        options.AuthorizationEndpoint = DiscordAuthenticationDefaults.AuthorizationEndpoint;
        options.TokenEndpoint = DiscordAuthenticationDefaults.TokenEndpoint;
        options.UserInformationEndpoint = DiscordAuthenticationDefaults.UserInformationEndpoint;
        
        options.Scope.Add("identify");
        options.Scope.Add("guilds");
        
        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "username");
        options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "global_name");
        
        options.SaveTokens = true;
    });

    WebApplication app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();
    
    app.MapControllers();

    app.Run(); 
}
catch (Exception e)
{
    Log.Fatal(e, "Application start-up failed.");
}
finally
{
    Log.CloseAndFlush();
}