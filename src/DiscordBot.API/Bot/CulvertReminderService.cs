using Discord.WebSocket;
using DiscordBot.Application.Interfaces.Repositories;

namespace DiscordBot.API.Bot;

public class CulvertReminderService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DiscordSocketClient _client;
    private DateTime? _ultimoEnvio;

    public CulvertReminderService(
        IServiceProvider serviceProvider,
        DiscordSocketClient client)
    {
        _serviceProvider = serviceProvider;
        _client = client;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("[CulvertReminder] Servicio de recordatorios iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await VerificarYEnviarRecordatoriosAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CulvertReminder] Error: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task VerificarYEnviarRecordatoriosAsync()
    {
        if (_client.ConnectionState != Discord.ConnectionState.Connected)
            return;

        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRecordatorioRepository>();

        var recordatorios = await repo.ObtenerActivosAsync();

        foreach (var config in recordatorios)
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(config.ZonaHoraria);
                var ahoraLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);

                var coincideDia = (int)ahoraLocal.DayOfWeek == config.DiaSemana;
                var coincideHora = ahoraLocal.Hour == config.Hora;
                var coincideMinuto = ahoraLocal.Minute == config.Minuto;

                if (!coincideDia || !coincideHora || !coincideMinuto)
                    continue;

                // Evitar enviar más de una vez en el mismo minuto
                var inicioMinuto = new DateTime(
                    ahoraLocal.Year, ahoraLocal.Month, ahoraLocal.Day,
                    ahoraLocal.Hour, ahoraLocal.Minute, 0);

                if (_ultimoEnvio.HasValue && _ultimoEnvio.Value == inicioMinuto)
                    continue;

                var canalId = (ulong)config.CanalId;
                var canal = _client.GetChannel(canalId) as ISocketMessageChannel;

                if (canal is null)
                {
                    Console.WriteLine($"[CulvertReminder] Canal {canalId} no encontrado para '{config.Nombre}'.");
                    continue;
                }

                var mensaje = config.Mensaje;

                if (!string.IsNullOrWhiteSpace(config.RolMencion))
                {
                    mensaje = $"{config.RolMencion} {mensaje}";
                }

                await canal.SendMessageAsync(mensaje);
                _ultimoEnvio = inicioMinuto;

                Console.WriteLine($"[CulvertReminder] Recordatorio '{config.Nombre}' enviado al canal {canalId}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CulvertReminder] Error procesando '{config.Nombre}': {ex.Message}");
            }
        }
    }
}
