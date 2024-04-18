using Blink3.API.Extensions;
using Blink3.API.Interfaces;
using Blink3.API.Services;
using Blink3.Core.Caching.Extensions;
using Blink3.Core.Configuration;
using Blink3.Core.Configuration.Extensions;
using Blink3.Core.Helpers;
using Blink3.DataAccess.Extensions;
using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Microsoft.AspNetCore.HttpOverrides;
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

    // Logging
    builder.Host.UseSerilog();
    
    // Problem details
    builder.Services.AddProblemDetails();

    // Controllers
    builder.Services.AddControllers().AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Converters.Add(new ULongToStringConverter());
    });

    // Swagger docs
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c => { c.EnableAnnotations(); });

    // Add Application configurations
    builder.Services.AddAppConfiguration(builder.Configuration);
    BlinkConfiguration appConfig = builder.Services.GetAppConfiguration();

    // Discord socket client
    builder.Services.AddDiscordHost((config, _) =>
    {
        config.SocketConfig = new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Verbose,
            MessageCacheSize = 0,
            GatewayIntents = GatewayIntents.Guilds
        };

        config.Token = appConfig.Discord.BotToken;
    });
    
    // Add forwarded headers
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardLimit = 3;
        options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    });

    List<string> origins = appConfig.ApiAllowedOrigins;
    origins.Add("https://*.discordsays.com");
    // Configure Cors
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy",
            corsPolicyBuilder => corsPolicyBuilder
                .WithOrigins(origins.ToArray())
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
    });

    // Add Data Access layer and cache provider
    builder.Services.AddDataAccess(appConfig);
    builder.Services.AddCaching(appConfig);
    builder.Services.AddSession();

    // For getting discord tokens
    builder.Services.AddHttpClient();
    builder.Services.AddSingleton<IDiscordTokenService, DiscordTokenService>();

    // Configure Authentication and Discord OAuth
    builder.Services.AddDiscordAuth(appConfig);

    WebApplication app = builder.Build();
    
    if (!app.Environment.IsDevelopment())
    {
        app.UseForwardedHeaders();
        app.UseHsts();
    }

    // Document API
    app.UseSwagger();
    app.UseSwaggerUI();

    // Force HTTPS
    app.UseHttpsRedirection();

    // Use Cors
    app.UseCors("CorsPolicy");

    app.UseSession();
    
    // Add authentication / authorization middleware
    app.UseAuthentication();
    app.UseAuthorization();

    // Map controller endpoints
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