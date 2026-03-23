using DiscordBot.Application.Interfaces.Repositories;
using DiscordBot.Application.Interfaces.Services;
using DiscordBot.Infrastructure.Configuration;
using DiscordBot.Infrastructure.Data;
using DiscordBot.Infrastructure.Persistence;
using DiscordBot.Infrastructure.Repositories;
using DiscordBot.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DatabaseSettings>(
            configuration.GetSection("DatabaseSettings"));

        services.AddSingleton<ConnectionFactory>();

        services.AddDbContext<BotDbContext>((serviceProvider, options) =>
        {
            var connectionFactory = serviceProvider.GetRequiredService<ConnectionFactory>();
            var connectionString = connectionFactory.GetConnectionString();

            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IPersonajeRepository, PersonajeRepository>();
        services.AddScoped<ICharacterImageService, MapleBotImageService>();
        services.AddScoped<IGuildDashboardService, GuildDashboardService>();

        return services;
    }
}