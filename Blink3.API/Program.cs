using System.Net;
using Blink3.API.Extensions;
using Blink3.API.Interfaces;
using Blink3.API.Services;
using Blink3.Core.Caching.Extensions;
using Blink3.Core.Configuration;
using Blink3.Core.Configuration.Extensions;
using Blink3.Core.Extensions;
using Blink3.Core.Helpers;
using Blink3.DataAccess.Extensions;
using Discord;
using Discord.Rest;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    // Add Application configurations
    builder.Services.AddAppConfiguration(builder.Configuration);
    BlinkConfiguration appConfig = builder.Services.GetAppConfiguration();

    // Add Logging
    builder.Services.AddBlinkLogging(builder.Configuration, "Blink3.API");
    
    Log.Information("Blink API Starting...");
    
    // Problem details
    builder.Services.AddProblemDetails();

    // Controllers
    builder.Services.AddControllers().AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Converters.Add(new ULongToStringConverter());
        options.SerializerSettings.Converters.Add(new NullableULongToStringConverter());
    });

    // Swagger docs
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c => { c.EnableAnnotations(); });
    
    // Discord bot client
    builder.Services.AddSingleton<DiscordRestClient>(_ =>
    {
        DiscordRestClient client = new();
        client.LoginAsync(TokenType.Bot, appConfig.Discord.BotToken).Wait();
        return client;
    });

    // Discord user client
    builder.Services.AddScoped<Func<DiscordRestClient>>(_ => { return () => new DiscordRestClient(); });

    // Add forwarded headers
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        
        options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("172.16.0.0"), 12));
    });

    List<string> origins = appConfig.ApiAllowedOrigins;
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

    // Encryption service
    string? encryptionKey = Environment.GetEnvironmentVariable("ENCRYPTION_KEY");
    builder.Services.AddSingleton<IEncryptionService>(_ => new EncryptionService(encryptionKey));

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