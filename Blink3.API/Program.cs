using Blink3.API.Extensions;
using Blink3.API.Services;
using Blink3.Common.Caching.Extensions;
using Blink3.Common.Configuration;
using Blink3.Common.Configuration.Extensions;
using Blink3.DataAccess.Extensions;
using Discord.Rest;
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
    builder.Services.AddProblemDetails();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c => { c.EnableAnnotations(); });

    // Configure Cors
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy",
            corsPolicyBuilder => corsPolicyBuilder
                .WithOrigins("https://localhost:7041") // adjust with your client url
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
    });

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
    builder.Services.AddDiscordAuth(appConfig);

    WebApplication app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    // Use Cors
    app.UseCors("CorsPolicy");

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