using Blink3.Bot.Services;
using Blink3.Core.Caching.Extensions;
using Blink3.Core.Configuration;
using Blink3.Core.Configuration.Extensions;
using Blink3.Core.Interfaces;
using Blink3.Core.Services;
using Blink3.Core.Services.Generators;
using Blink3.DataAccess.Extensions;
using Blink3.DataAccess.Services;
using Discord;
using Discord.Addons.Hosting;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

if (!EF.IsDesignTime)
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
        .WriteTo.Console()
        .CreateLogger();

try
{
    HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddHttpClient();
    builder.Services.AddHttpClient<IDiscordAttachmentService, DiscordAttachmentService>();
    builder.Services.AddHttpClient<IWordsClientService, WordsClientService>();

    builder.Services.AddSerilog();

    builder.Services.AddAppConfiguration(builder.Configuration);
    BlinkConfiguration appConfig = builder.Services.GetAppConfiguration();

    builder.Services.AddDataAccess(appConfig);
    builder.Services.AddCaching(appConfig);

    builder.Services.AddHostedService<MigrationService>();
    builder.Services.AddHostedService<WordSeedService>();

    builder.Services.AddSingleton<IWordleGuessImageGenerator, WordleGuessImageGenerator>();
    builder.Services.AddScoped<IWordleGameService, WordleGameService>();

    builder.Services.AddDiscordHost((config, _) =>
    {
        config.SocketConfig = new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Verbose,
            AlwaysDownloadUsers = true,
            MessageCacheSize = 200,
            GatewayIntents = GatewayIntents.Guilds |
                             GatewayIntents.GuildMembers |
                             GatewayIntents.GuildMessages |
                             GatewayIntents.MessageContent |
                             GatewayIntents.DirectMessages
        };

        config.Token = appConfig.Discord.BotToken;
    });

    builder.Services.AddInteractionService((config, _) =>
    {
        config.DefaultRunMode = RunMode.Async;
        config.LogLevel = LogSeverity.Info;
        config.UseCompiledLambda = true;
    });

    builder.Services.AddHostedService<InteractionHandler>();
    builder.Services.AddHostedService<BotStatusService>();
    builder.Services.AddHostedService<TempVcCleanService>();

    IHost host = builder.Build();

    await host.RunAsync();
}
catch (Exception e)
{
    Log.Fatal(e, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}