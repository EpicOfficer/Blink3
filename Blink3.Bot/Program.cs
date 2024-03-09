using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDiscordShardedHost((config, _) =>
{
    config.SocketConfig = new DiscordSocketConfig
    {
        LogLevel = LogSeverity.Verbose,
        AlwaysDownloadUsers = true,
        MessageCacheSize = 200,
        GatewayIntents = GatewayIntents.AllUnprivileged
    };
    
    config.Token = builder.Configuration["Token"]!;
});

builder.Services.AddInteractionService((config, _) =>
{
    config.LogLevel = LogSeverity.Info;
    config.UseCompiledLambda = true;
});

var host = builder.Build();

await host.RunAsync();