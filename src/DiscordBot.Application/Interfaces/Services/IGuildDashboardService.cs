namespace DiscordBot.Application.Interfaces.Services;

public interface IGuildDashboardService
{
    Task<string> GenerarDashboardAsync(int anio, int semana);
}