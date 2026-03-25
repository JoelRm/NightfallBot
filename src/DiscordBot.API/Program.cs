using Discord;
using Discord.WebSocket;
using DiscordBot.API.Bot;
using DiscordBot.Infrastructure;
using System.Collections;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.SetBasePath(AppContext.BaseDirectory);
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables();

builder.Configuration.AddJsonFile(
    $"appsettings.{builder.Environment.EnvironmentName}.json",
    optional: true,
    reloadOnChange: true);

Console.WriteLine("=== ENV TEST START ===");

foreach (DictionaryEntry env in Environment.GetEnvironmentVariables())
{
    var key = env.Key?.ToString() ?? string.Empty;

    if (key.Contains("DISCORD", StringComparison.OrdinalIgnoreCase) ||
        key.Contains("TOKEN", StringComparison.OrdinalIgnoreCase) ||
        key.Contains("TEST", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine($"{key}=***");
    }
}

Console.WriteLine($"PROGRAM - Discord:Token exists? {!string.IsNullOrWhiteSpace(builder.Configuration["Discord:Token"])}");
Console.WriteLine($"PROGRAM - DISCORD__TOKEN exists? {!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DISCORD__TOKEN"))}");
Console.WriteLine($"PROGRAM - Discord__Token exists? {!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("Discord__Token"))}");
Console.WriteLine($"PROGRAM - TEST_VAR exists? {!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TEST_VAR"))}");
Console.WriteLine("=== ENV TEST END ===");

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.Guilds |
                     GatewayIntents.GuildMessages |
                     GatewayIntents.MessageContent
}));

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<DiscordBotService>();
builder.Services.AddHostedService<CulvertReminderService>();

var app = builder.Build();
await app.RunAsync();