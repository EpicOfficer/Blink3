using Blink3.Core.Caching.Extensions;
using Blink3.Core.Configuration;
using Blink3.Core.Configuration.Extensions;
using Blink3.Core.Extensions;
using Blink3.DataAccess.Extensions;
using Blink3.Scout.Services;
using Discord;
using Discord.Rest;
using Hangfire;
using Hangfire.PostgreSql;
using Serilog;
using Serilog.Events;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// Add Application configurations
builder.Services.AddAppConfiguration(builder.Configuration);
BlinkConfiguration appConfig = builder.Services.GetAppConfiguration();

// Logging
builder.Services.AddBlinkLogging(builder.Configuration, "Blink3.Scout");

Log.Information("Blink Scout Starting...");

// Add Hangfire
builder.Services.AddHangfire(config =>
{
    config.UsePostgreSqlStorage(c =>
        c.UseNpgsqlConnection(appConfig.ConnectionStrings.DefaultConnection));
});
builder.Services.AddHangfireServer();

// Discord bot client
builder.Services.AddSingleton<DiscordRestClient>(_ =>
{
    DiscordRestClient client = new();
    client.LoginAsync(TokenType.Bot, appConfig.Discord.BotToken).Wait();
    return client;
});

// Data
builder.Services.AddDataAccess(appConfig);
builder.Services.AddCaching(appConfig);
builder.Services.AddHttpClient();

builder.Services.AddHostedService<StartupService>();

try
{
    IHost host = builder.Build();
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed.");
}
finally
{
    Log.CloseAndFlush();
}