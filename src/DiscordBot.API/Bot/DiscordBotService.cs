using Discord;
using Discord.WebSocket;
using DiscordBot.Application.Interfaces.Repositories;
using DiscordBot.Application.Interfaces.Services;
using DiscordBot.Domain.DTOs;
using DiscordBot.Domain.Entities;
using DiscordBot.Utilities.Helpers;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace DiscordBot.API.Bot;

public class DiscordBotService : BackgroundService
{
    private readonly DiscordSocketClient _client;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    public DiscordBotService(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        DiscordSocketClient client)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _client = client;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;
        _client.MessageReceived += MessageReceivedAsync;

        Console.WriteLine("=== BOT START ===");

        Console.WriteLine("TOKEN CONFIG: " + _configuration["Discord:Token"]);
        Console.WriteLine("TOKEN ENV: " + Environment.GetEnvironmentVariable("DISCORD__TOKEN"));

        Console.WriteLine(
            "COMMAND IDS ENV: " +
            Environment.GetEnvironmentVariable("DISCORD__COMMANDCHANNELIDS")
        );

        var token = _configuration["Discord:Token"];

        if (string.IsNullOrWhiteSpace(token))
        {
            token = Environment.GetEnvironmentVariable("DISCORD__TOKEN");
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            token = Environment.GetEnvironmentVariable("Discord__Token");
        }

        Console.WriteLine($"Config token exists: {!string.IsNullOrWhiteSpace(_configuration["Discord:Token"])}");
        Console.WriteLine($"Env DISCORD__TOKEN exists: {!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DISCORD__TOKEN"))}");
        Console.WriteLine($"Env Discord__Token exists: {!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("Discord__Token"))}");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("No se encontró el token de Discord");
        }

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private Task ReadyAsync()
    {
        Console.WriteLine($"Bot conectado como: {_client.CurrentUser}");
        return Task.CompletedTask;
    }

    private HashSet<ulong> ObtenerCanalesComandosIds()
    {
        var result = new HashSet<ulong>();

        var idsConfig = _configuration.GetSection("Discord:CommandChannelIds").Get<string[]>();
        if (idsConfig != null)
        {
            foreach (var id in idsConfig)
            {
                if (ulong.TryParse(id, out var channelId))
                    result.Add(channelId);
            }
        }

        var envUpper = Environment.GetEnvironmentVariable("DISCORD__COMMANDCHANNELIDS");
        if (!string.IsNullOrWhiteSpace(envUpper))
        {
            var partes = envUpper.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var parte in partes)
            {
                if (ulong.TryParse(parte, out var channelId))
                    result.Add(channelId);
            }
        }

        var envPascal = Environment.GetEnvironmentVariable("Discord__CommandChannelIds");
        if (!string.IsNullOrWhiteSpace(envPascal))
        {
            var partes = envPascal.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var parte in partes)
            {
                if (ulong.TryParse(parte, out var channelId))
                    result.Add(channelId);
            }
        }

        return result;
    }

    private async Task<bool> ValidarCanalComandoAsync(SocketMessage message)
    {
        var canalesPermitidos = ObtenerCanalesComandosIds();

        if (canalesPermitidos.Count == 0)
            return true;

        if (canalesPermitidos.Contains(message.Channel.Id))
            return true;

        var primerCanalId = canalesPermitidos.First();
        var mencionCanal = $"<#{primerCanalId}>";

        await message.Channel.SendMessageAsync(
            $"⚠️ Este comando solo puede usarse en {mencionCanal}.");

        return false;
    }

    private async Task MessageReceivedAsync(SocketMessage message)
    {
        try
        {
            if (message.Author.IsBot)
                return;

            var content = message.Content.Trim();

            if (string.IsNullOrWhiteSpace(content))
                return;

            var esComando = content.StartsWith("!");

            if (esComando && !await ValidarCanalComandoAsync(message))
                return;

            if (content.Equals("hola", StringComparison.OrdinalIgnoreCase))
            {
                await message.Channel.SendMessageAsync("Bot Activo");
                return;
            }

            if (content.Equals("!misroles", StringComparison.OrdinalIgnoreCase))
            {
                await ProcesarMisRolesAsync(message);
                return;
            }

            if (content.StartsWith("!buscar", StringComparison.OrdinalIgnoreCase))
            {
                await ProcesarBuscarAsync(message, content);
                return;
            }

            if (content.StartsWith("!registrar ", StringComparison.OrdinalIgnoreCase))
            {
                if (!await ValidarAdminAsync(message, "🚫 Solo Guild Master u Officers pueden registrar personajes."))
                    return;

                await ProcesarRegistroIndividualAsync(message, content);
                return;
            }

            if (content.StartsWith("!registrarmasivo", StringComparison.OrdinalIgnoreCase))
            {
                if (!await ValidarAdminAsync(message, "🚫 Solo Guild Master u Officers pueden registrar personajes masivamente."))
                    return;

                await ProcesarRegistroMasivoAsync(message, content);
                return;
            }

            if (content.StartsWith("!culvertmasivo", StringComparison.OrdinalIgnoreCase))
            {
                if (!await ValidarAdminAsync(message, "🚫 Solo Guild Master u Officers pueden registrar Culvert masivo."))
                    return;

                await ProcesarCulvertMasivoAsync(message, content);
                return;
            }

            if (content.StartsWith("!culvert ", StringComparison.OrdinalIgnoreCase))
            {
                if (!await ValidarAdminAsync(message, "🚫 Solo Guild Master u Officers pueden registrar Culvert."))
                    return;

                await ProcesarCulvertIndividualAsync(message, content);
                return;
            }

            if (content.Equals("!toppuntos", StringComparison.OrdinalIgnoreCase))
            {
                await ProcesarTopPuntosAsync(message);
                return;
            }

            if (content.Equals("!topculvert", StringComparison.OrdinalIgnoreCase))
            {
                await ProcesarTopCulvertAsync(message);
                return;
            }

            if (content.StartsWith("!puntos ", StringComparison.OrdinalIgnoreCase))
            {
                await ProcesarPuntosAsync(message, content);
                return;
            }

            if (content.Equals("!record", StringComparison.OrdinalIgnoreCase))
            {
                await ProcesarRecordAsync(message);
                return;
            }

            if (content.Equals("!faltantes", StringComparison.OrdinalIgnoreCase))
            {
                await ProcesarFaltantesAsync(message);
                return;
            }

            if (content.Equals("!tienda", StringComparison.OrdinalIgnoreCase))
            {
                await ProcesarTiendaAsync(message);
                return;
            }

            if (content.StartsWith("!comprar ", StringComparison.OrdinalIgnoreCase))
            {
                await ProcesarComprarAsync(message, content);
                return;
            }

            if (content.Equals("!guildstats", StringComparison.OrdinalIgnoreCase))
            {
                await ProcesarGuildStatsAsync(message);
                return;
            }

            if (content.StartsWith("!progreso", StringComparison.OrdinalIgnoreCase))
            {
                await ProcesarProgresoAsync(message, content);
                return;
            }

            if (content.Equals("!guildstatsimg", StringComparison.OrdinalIgnoreCase))
            {
                await ProcesarGuildStatsImgAsync(message);
                return;
            }

            if (content.StartsWith("!culvert0", StringComparison.OrdinalIgnoreCase))
            {
                await ProcesarCulvertCeroAsync(message, content);
                return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DiscordBotService] Error en MessageReceivedAsync: {ex}");

            try
            {
                await message.Channel.SendMessageAsync("Ocurrió un error al procesar el comando.");
            }
            catch
            {
            }
        }
    }

    private async Task<bool> ValidarAdminAsync(SocketMessage message, string? mensajeSinPermiso = null)
    {
        if (message.Author is not SocketGuildUser user)
        {
            await message.Channel.SendMessageAsync("🚫 Este comando solo puede usarse dentro del servidor.");
            return false;
        }

        var rolesUsuario = user.Roles.Select(r => r.Id);

        if (PermissionHelper.EsAdmin(rolesUsuario))
            return true;

        await message.Channel.SendMessageAsync(
            mensajeSinPermiso ?? "🚫 No tienes permisos para usar este comando.");
        return false;
    }

    private async Task ProcesarMisRolesAsync(SocketMessage message)
    {
        if (message.Author is not SocketGuildUser user)
        {
            await message.Channel.SendMessageAsync("Este comando solo funciona dentro del servidor.");
            return;
        }

        var roles = user.Roles
            .OrderByDescending(r => r.Position)
            .Select(r => $"• {r.Name} → `{r.Id}`")
            .ToList();

        if (!roles.Any())
        {
            await message.Channel.SendMessageAsync("No se encontraron roles.");
            return;
        }

        var esAdmin = PermissionHelper.EsAdmin(user.Roles.Select(r => r.Id));

        var texto =
            $"👤 **Roles de {user.DisplayName}**\n\n" +
            string.Join("\n", roles) +
            $"\n\n🔐 Nivel: **{(esAdmin ? "ADMIN" : "MIEMBRO")}**";

        await message.Channel.SendMessageAsync(texto);
    }

    private async Task ProcesarBuscarAsync(SocketMessage message, string content)
    {
        var partes = content.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        if (partes.Length < 2)
        {
            await message.Channel.SendMessageAsync("Uso: !buscar nombre_personaje");
            return;
        }

        var nombreIngresado = partes[1].Trim();

        using var scope = _serviceProvider.CreateScope();

        var repo = scope.ServiceProvider.GetRequiredService<IPersonajeRepository>();
        var imageService = scope.ServiceProvider.GetRequiredService<ICharacterImageService>();

        var stats = await repo.ObtenerStatsPorNombreAsync(nombreIngresado);

        if (stats is null)
        {
            await message.Channel.SendMessageAsync($"No encontré a **{nombreIngresado}**.");
            return;
        }

        var progreso = await repo.ObtenerProgresoPorNombreAsync(nombreIngresado);

        string? imagenUrl = stats.ImagenUrl;

        if (string.IsNullOrWhiteSpace(imagenUrl))
        {
            imagenUrl = await imageService.ObtenerImagenPersonajeAsync(stats.NombrePersonaje);

            if (!string.IsNullOrWhiteSpace(imagenUrl))
            {
                await repo.ActualizarImagenAsync(stats.IdPersonaje, imagenUrl);
            }
        }

        var embed = new EmbedBuilder()
            .WithTitle(stats.NombrePersonaje)
            .WithDescription($"{stats.NombrePersonaje} es Level {stats.Level} {stats.Clase}")
            .AddField("Personal Best", stats.PersonalBest.ToString("N0"), true)
            .AddField("Period Average", stats.PeriodAverage.ToString("N2"), true)
            .AddField("Participation", $"{stats.ParticipationCount}/{stats.TotalWeeks}", true)
            .AddField("Ratio", $"{stats.ParticipationRatio:N2}%", true)
            .AddField("Puntos actuales", stats.PuntosActuales.ToString("N0"), true)
            .AddField("Ranking", $"#{stats.RankingCulvert}", true)
            .AddField("Total Culvert", stats.TotalCulvert.ToString("N0"), true)
            .WithColor(Color.Blue)
            .WithFooter(footer =>
            {
                footer.Text = $"Consultado por {message.Author.Username}";
            });

        const string defaultImage = "https://placehold.co/256x256/png?text=No+Image";

        embed.WithThumbnailUrl(
            !string.IsNullOrWhiteSpace(imagenUrl)
                ? imagenUrl
                : defaultImage);

        Stream? chartStream = null;

        if (progreso.Any())
        {
            var chartUrl = ChartHelper.BuildCulvertProgressBarChartUrl(
                progreso,
                stats.NombrePersonaje);

            if (!string.IsNullOrWhiteSpace(chartUrl))
            {
                chartStream = await ChartHelper.DownloadChartAsync(chartUrl);

                if (chartStream != null)
                {
                    embed.WithImageUrl("attachment://culvert-chart.png");
                }
            }
        }

        if (chartStream != null)
        {
            chartStream.Position = 0;
            await message.Channel.SendFileAsync(
                chartStream,
                "culvert-chart.png",
                embed: embed.Build());
        }
        else
        {
            await message.Channel.SendMessageAsync(embed: embed.Build());
        }
    }

    private async Task ProcesarRegistroIndividualAsync(SocketMessage message, string content)
    {
        var payload = content["!registrar".Length..].Trim();
        var parsed = ParsearLineaRegistro(payload);

        if (!parsed.EsValido || string.IsNullOrWhiteSpace(parsed.Nombre))
        {
            await message.Channel.SendMessageAsync("Uso: `!registrar Nombre` o `!registrar Nombre \"Clase\" Level`");
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPersonajeRepository>();
        var profileService = scope.ServiceProvider.GetRequiredService<ICharacterImageService>();

        var existe = await repo.ExistePorNombreAsync(parsed.Nombre);

        if (existe)
        {
            await message.Channel.SendMessageAsync($"El personaje **{parsed.Nombre}** ya existe.");
            return;
        }

        CharacterProfileDto? perfil = null;

        if (string.IsNullOrWhiteSpace(parsed.Clase) || !parsed.Level.HasValue)
        {
            perfil = await profileService.ObtenerPerfilPersonajeAsync(parsed.Nombre);

            if (perfil is null || !perfil.Encontrado)
            {
                await message.Channel.SendMessageAsync(
                    $"❌ No se encontró información automática para **{parsed.Nombre}**. " +
                    $"Puedes registrarlo manualmente con: `!registrar {parsed.Nombre} \"Clase\" Level`");
                return;
            }
        }

        var nombreFinal = perfil?.NombrePersonaje?.Trim();
        if (string.IsNullOrWhiteSpace(nombreFinal))
            nombreFinal = parsed.Nombre.Trim();

        var claseFinal = !string.IsNullOrWhiteSpace(parsed.Clase)
            ? parsed.Clase!.Trim()
            : perfil?.Clase?.Trim();

        var levelFinal = parsed.Level ?? perfil?.Level;

        if (string.IsNullOrWhiteSpace(claseFinal) || !levelFinal.HasValue)
        {
            await message.Channel.SendMessageAsync(
                $"❌ No se pudo completar clase/level para **{parsed.Nombre}** automáticamente. " +
                $"Regístralo manualmente con: `!registrar {parsed.Nombre} \"Clase\" Level`");
            return;
        }

        var personaje = new Personaje
        {
            NombrePersonaje = nombreFinal,
            Clase = claseFinal,
            Level = levelFinal.Value,
            ImagenUrl = perfil?.ImagenUrl,
            PuntosActuales = 0,
            MonedasActuales = 0,
            Activo = true,
            FechaCreacion = DateTime.UtcNow,
            FechaActualizacion = DateTime.UtcNow
        };

        personaje = await repo.CrearAsync(personaje);

        if (!string.IsNullOrWhiteSpace(perfil?.ImagenUrl))
        {
            await repo.ActualizarImagenAsync(personaje.IdPersonaje, perfil.ImagenUrl!);
        }

        var respuesta =
            $"✅ Personaje registrado:\n" +
            $"**Nombre:** {personaje.NombrePersonaje}\n" +
            $"**Clase:** {personaje.Clase}\n" +
            $"**Level:** {personaje.Level}";

        respuesta += !string.IsNullOrWhiteSpace(perfil?.ImagenUrl)
            ? "\n🖼 Imagen guardada."
            : "\n⚠ Sin imagen automática.";

        await message.Channel.SendMessageAsync(respuesta);
    }

    private async Task ProcesarRegistroMasivoAsync(SocketMessage message, string content)
    {
        var payload = content["!registrarmasivo".Length..].Trim();

        if (string.IsNullOrWhiteSpace(payload))
        {
            await message.Channel.SendMessageAsync(
                "Uso:\n" +
                "`!registrarmasivo`\n" +
                "`Nombre`\n" +
                "`Nombre|Clase|Level`\n" +
                "`Nombre`\n" +
                "`Nombre|Clase|Level`");
            return;
        }

        var lineas = payload
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        if (!lineas.Any())
        {
            await message.Channel.SendMessageAsync("No se encontraron líneas para procesar.");
            return;
        }

        var mensajeProceso = await message.Channel.SendMessageAsync(
            $"⏳ Se iniciará el registro masivo de **{lineas.Count}** personaje(s)..."
        );

        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPersonajeRepository>();
        var profileService = scope.ServiceProvider.GetRequiredService<ICharacterImageService>();

        var registrados = new List<string>();
        var registradosConImagen = new List<string>();
        var noRegistrados = new List<string>();

        foreach (var linea in lineas)
        {
            try
            {
                var parsed = ParsearLineaRegistro(linea);

                if (!parsed.EsValido || string.IsNullOrWhiteSpace(parsed.Nombre))
                {
                    noRegistrados.Add($"{linea} → formato inválido");
                    continue;
                }

                var existe = await repo.ExistePorNombreAsync(parsed.Nombre);

                if (existe)
                {
                    noRegistrados.Add($"{parsed.Nombre} → ya existe");
                    continue;
                }

                CharacterProfileDto? perfil = null;

                if (string.IsNullOrWhiteSpace(parsed.Clase) || !parsed.Level.HasValue)
                {
                    perfil = await profileService.ObtenerPerfilPersonajeAsync(parsed.Nombre);

                    if (perfil is null || !perfil.Encontrado)
                    {
                        noRegistrados.Add($"{parsed.Nombre} → no encontrado automáticamente");
                        continue;
                    }
                }

                var nombreFinal = perfil?.NombrePersonaje?.Trim();
                if (string.IsNullOrWhiteSpace(nombreFinal))
                    nombreFinal = parsed.Nombre.Trim();

                var claseFinal = !string.IsNullOrWhiteSpace(parsed.Clase)
                    ? parsed.Clase!.Trim()
                    : perfil?.Clase?.Trim();

                var levelFinal = parsed.Level ?? perfil?.Level;

                if (string.IsNullOrWhiteSpace(claseFinal) || !levelFinal.HasValue)
                {
                    noRegistrados.Add($"{parsed.Nombre} → faltó clase o level");
                    continue;
                }

                var personaje = new Personaje
                {
                    NombrePersonaje = nombreFinal,
                    Clase = claseFinal,
                    Level = levelFinal.Value,
                    ImagenUrl = perfil?.ImagenUrl,
                    PuntosActuales = 0,
                    MonedasActuales = 0,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow,
                    FechaActualizacion = DateTime.UtcNow
                };

                personaje = await repo.CrearAsync(personaje);
                registrados.Add(personaje.NombrePersonaje);

                if (!string.IsNullOrWhiteSpace(perfil?.ImagenUrl))
                {
                    await repo.ActualizarImagenAsync(personaje.IdPersonaje, perfil.ImagenUrl!);
                    registradosConImagen.Add(personaje.NombrePersonaje);
                }
            }
            catch (Exception ex)
            {
                noRegistrados.Add($"{linea} → error: {ex.Message}");
            }
        }

        var respuesta = $"✅ Registro masivo finalizado.\n";
        respuesta += $"📌 Registrados: **{registrados.Count}**\n";
        respuesta += $"🖼 Con imagen: **{registradosConImagen.Count}**\n";
        respuesta += $"❌ No registrados: **{noRegistrados.Count}**";

        if (registrados.Any())
        {
            respuesta += "\n\n**Registrados:**\n" +
                        string.Join("\n", registrados.Select(x => $"• {x}"));
        }

        if (noRegistrados.Any())
        {
            respuesta += "\n\n**No registrados:**\n" +
                        string.Join("\n", noRegistrados.Select(x => $"• {x}"));
        }

        await mensajeProceso.ModifyAsync(m => m.Content = respuesta);
    }

    private async Task ProcesarCulvertIndividualAsync(SocketMessage message, string content)
    {
        var payload = content["!culvert".Length..].Trim();
        var partes = payload.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        if (partes.Length < 2)
        {
            await message.Channel.SendMessageAsync("Uso: `!culvert Nombre 180000`");
            return;
        }

        var nombre = partes[0].Trim();

        if (!long.TryParse(partes[1].Trim(), out var culvertScore))
        {
            await message.Channel.SendMessageAsync("El Culvert debe ser numérico.");
            return;
        }

        var (anio, semana, fechaUtc, fechaPeru) = WeekHelper.ObtenerSemanaActualPeru();

        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPersonajeRepository>();

        var resultado = await repo.RegistrarCulvertSemanalAsync(
            nombre,
            culvertScore,
            message.Author.Username,
            anio,
            semana,
            fechaUtc);

        if (resultado is null)
        {
            await message.Channel.SendMessageAsync($"No encontré a **{nombre}**.");
            return;
        }

        var accion = resultado.FueActualizado ? "actualizado" : "registrado";

        await message.Channel.SendMessageAsync(
            $"✅ Culvert {accion} para **{resultado.NombrePersonaje}**\n" +
            $"**Semana:** {resultado.Semana}/{resultado.Anio}\n" +
            $"**Score:** {resultado.CulvertScore:N0}\n" +
            $"**Puntos ganados:** {resultado.PuntosGanados}\n" +
            $"**Monedas ganadas:** {resultado.MonedasGanadas}\n" +
            $"**Puntos actuales:** {resultado.PuntosActuales}\n" +
            $"**Monedas actuales:** {resultado.MonedasActuales}");
    }

    private async Task ProcesarCulvertMasivoAsync(SocketMessage message, string content)
    {
        var payload = content["!culvertmasivo".Length..].Trim();

        if (string.IsNullOrWhiteSpace(payload))
        {
            await message.Channel.SendMessageAsync(
                "Uso:\n" +
                "`!culvertmasivo`\n" +
                "`Fõxy|180000`\n" +
                "`Annz|210000`\n" +
                "`Michi|0`");
            return;
        }

        var lineas = payload
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        var (anio, semana, fechaUtc, fechaPeru) = WeekHelper.ObtenerSemanaActualPeru();

        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPersonajeRepository>();

        var procesados = new List<string>();
        var noProcesados = new List<string>();

        foreach (var linea in lineas)
        {
            try
            {
                var partes = linea.Split('|', StringSplitOptions.TrimEntries);

                if (partes.Length < 2)
                {
                    noProcesados.Add($"{linea} → formato inválido");
                    continue;
                }

                var nombre = partes[0].Trim();

                if (!long.TryParse(partes[1].Trim(), out var culvertScore))
                {
                    noProcesados.Add($"{nombre} → culvert inválido");
                    continue;
                }

                var resultado = await repo.RegistrarCulvertSemanalAsync(
                    nombre,
                    culvertScore,
                    message.Author.Username,
                    anio,
                    semana,
                    fechaUtc);

                if (resultado is null)
                {
                    noProcesados.Add($"{nombre} → personaje no encontrado");
                    continue;
                }

                procesados.Add(
                    $"{resultado.NombrePersonaje} → {resultado.CulvertScore:N0} | +{resultado.PuntosGanados} pts | +{resultado.MonedasGanadas} monedas");
            }
            catch (Exception ex)
            {
                var detalle = ex.InnerException?.Message ?? ex.Message;
                noProcesados.Add($"{linea} → error: {detalle}");
            }
        }

        var respuesta = $"✅ Culverts procesados: **{procesados.Count}**\n**Semana:** {semana}/{anio}";

        if (procesados.Any())
        {
            respuesta += "\n\n**Procesados:**\n" +
                        string.Join("\n", procesados.Select(x => $"• {x}"));
        }

        if (noProcesados.Any())
        {
            respuesta += "\n\n❌ **No procesados:**\n" +
                        string.Join("\n", noProcesados.Select(x => $"• {x}"));
        }

        await message.Channel.SendMessageAsync(respuesta);
    }

    private async Task ProcesarTopPuntosAsync(SocketMessage message)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPersonajeRepository>();

        var ranking = await repo.ObtenerTopPuntosAsync(10);

        if (!ranking.Any())
        {
            await message.Channel.SendMessageAsync("No hay datos para mostrar.");
            return;
        }

        var texto = "🏆 **Top Puntos**\n\n" +
                    string.Join("\n", ranking.Select(x => $"{x.Posicion}. **{x.NombrePersonaje}** — {x.Valor:N0} pts"));

        await message.Channel.SendMessageAsync(texto);
    }

    private async Task ProcesarTopCulvertAsync(SocketMessage message)
    {
        var (anio, semana, fechaUtc, fechaPeru) = WeekHelper.ObtenerSemanaActualPeru();

        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPersonajeRepository>();

        var ranking = await repo.ObtenerTopCulvertSemanalAsync(anio, semana, 10);

        if (!ranking.Any())
        {
            await message.Channel.SendMessageAsync($"No hay registros de Culvert para la semana {semana}/{anio}.");
            return;
        }

        var texto = $"🔥 **Top Culvert Semana {semana}/{anio}**\n\n" +
                    string.Join("\n", ranking.Select(x => $"{x.Posicion}. **{x.NombrePersonaje}** — {x.Valor:N0}"));

        await message.Channel.SendMessageAsync(texto);
    }

    private async Task ProcesarPuntosAsync(SocketMessage message, string content)
    {
        var nombre = content["!puntos".Length..].Trim();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            await message.Channel.SendMessageAsync("Uso: `!puntos NombrePersonaje`");
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPersonajeRepository>();

        var resumen = await repo.ObtenerResumenPersonajeAsync(nombre);

        if (resumen is null)
        {
            await message.Channel.SendMessageAsync($"No encontré a **{nombre}**.");
            return;
        }

        var texto =
            $"📊 **{resumen.NombrePersonaje}**\n" +
            $"• Puntos actuales: **{resumen.PuntosActuales:N0}**\n" +
            $"• Monedas actuales: **{resumen.MonedasActuales:N0}**\n" +
            $"• Participaciones: **{resumen.Participaciones}**\n" +
            $"• Promedio Culvert: **{resumen.PromedioCulvert:N2}**\n" +
            $"• Mejor Culvert: **{resumen.MejorCulvert:N0}**";

        await message.Channel.SendMessageAsync(texto);
    }

    private async Task ProcesarRecordAsync(SocketMessage message)
    {
        var (anio, semana, fechaUtc, fechaPeru) = WeekHelper.ObtenerSemanaActualPeru();

        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPersonajeRepository>();

        var record = await repo.ObtenerRecordSemanalAsync(anio, semana);

        if (record is null)
        {
            await message.Channel.SendMessageAsync($"No hay record para la semana {semana}/{anio}.");
            return;
        }

        await message.Channel.SendMessageAsync(
            $"🏆 **Record Semana {semana}/{anio}**\n" +
            $"🥇 **{record.NombrePersonaje}** — {record.Valor:N0}");
    }

    private async Task ProcesarFaltantesAsync(SocketMessage message)
    {
        var (anio, semana, fechaUtc, fechaPeru) = WeekHelper.ObtenerSemanaActualPeru();

        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPersonajeRepository>();

        var faltantes = await repo.ObtenerFaltantesSemanaAsync(anio, semana);

        if (!faltantes.Any())
        {
            await message.Channel.SendMessageAsync($"✅ Todos registraron Culvert en la semana {semana}/{anio}.");
            return;
        }

        var texto = $"⚠ **Faltantes Semana {semana}/{anio}**\n\n" +
                    string.Join("\n", faltantes.Select(x => $"• {x}"));

        await message.Channel.SendMessageAsync(texto);
    }

    private async Task ProcesarTiendaAsync(SocketMessage message)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPersonajeRepository>();

        var items = await repo.ObtenerItemsTiendaAsync();

        if (!items.Any())
        {
            await message.Channel.SendMessageAsync("La tienda está vacía.");
            return;
        }



        var grupos = items.GroupBy(x => x.Categoria);

        var bloques = grupos.Select(g =>
        {
            var lineas = g.Select(x =>
            {
                var stockTexto = x.Stock.HasValue ? $" | Stock: {x.Stock}" : "";

                var descripcionTexto =
                    !string.IsNullOrWhiteSpace(x.Descripcion) && x.Descripcion != "0"
                    ? $" |  📝 {x.Descripcion}"
                    : "";

                return $"• **{x.NombreItem}** — {x.CostoPuntos} pts" +
                       $"{(x.CostoMonedas > 0 ? $" | {x.CostoMonedas} monedas" : "")}" +
                       $"{stockTexto}" +
                       $"{descripcionTexto}";
            });

            return $"**{g.Key}**\n" + string.Join("\n", lineas);
        });

        var texto = "🛒 **Nightfall Shop**\n\n" + string.Join("\n\n", bloques);

        await message.Channel.SendMessageAsync(texto);
    }

    private async Task ProcesarComprarAsync(SocketMessage message, string content)
    {
        var payload = content["!comprar".Length..].Trim();

        if (string.IsNullOrWhiteSpace(payload) || !payload.Contains('|'))
        {
            await message.Channel.SendMessageAsync("Uso: `!comprar NombrePersonaje | Nombre del Item`");
            return;
        }

        var partes = payload.Split('|', 2, StringSplitOptions.TrimEntries);

        if (partes.Length < 2)
        {
            await message.Channel.SendMessageAsync("Uso: `!comprar NombrePersonaje | Nombre del Item`");
            return;
        }

        var nombrePersonaje = partes[0].Trim();
        var nombreItem = partes[1].Trim();

        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPersonajeRepository>();

        var resultado = await repo.ComprarItemAsync(nombrePersonaje, nombreItem, message.Author.Username);

        if (!resultado.Ok)
        {
            await message.Channel.SendMessageAsync($"❌ {resultado.Mensaje}");
            return;
        }

        await message.Channel.SendMessageAsync(
            $"✅ Compra realizada\n" +
            $"• Personaje: **{resultado.NombrePersonaje}**\n" +
            $"• Item: **{resultado.NombreItem}**\n" +
            $"• Puntos restantes: **{resultado.PuntosRestantes:N0}**\n" +
            $"• Monedas restantes: **{resultado.MonedasRestantes:N0}**");
    }

    private async Task ProcesarGuildStatsAsync(SocketMessage message)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPersonajeRepository>();

        var stats = await repo.ObtenerGuildStatsAsync();

        var texto =
            $"📈 **Guild Stats**\n" +
            $"• Total personajes: **{stats.TotalPersonajes:N0}**\n" +
            $"• Total participaciones: **{stats.TotalParticipaciones:N0}**\n" +
            $"• Total Culvert acumulado: **{stats.TotalCulvert:N0}**\n" +
            $"• Promedio Culvert: **{stats.PromedioCulvert:N2}**";

        await message.Channel.SendMessageAsync(texto);
    }

    private async Task ProcesarGuildStatsImgAsync(SocketMessage message)
    {
        var (anio, semana, _, _) = WeekHelper.ObtenerSemanaActualPeru();

        using var scope = _serviceProvider.CreateScope();
        var dashboardService = scope.ServiceProvider.GetRequiredService<IGuildDashboardService>();

        var imagePath = await dashboardService.GenerarDashboardAsync(anio, semana);

        await message.Channel.SendFileAsync(
            imagePath,
            $"📊 Nightfall Dashboard — Semana {semana}/{anio}");
    }

    private async Task ProcesarCulvertCeroAsync(SocketMessage message, string content)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPersonajeRepository>();

        int minimoSemanas = 3;

        var partes = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Permite: !culvert0 3  => mostrar solo los que llevan 3 o más semanas seguidas en 0
        if (partes.Length == 2 && int.TryParse(partes[1], out var minParam) && minParam > 0)
        {
            minimoSemanas = minParam;
        }

        var lista = await repo.ObtenerRachaCulvertCeroAsync(minimoSemanas);

        if (!lista.Any())
        {
            await message.Channel.SendMessageAsync(
                $"✅ No hay personajes con {minimoSemanas} o más semanas seguidas sin hacer Culvert.");
            return;
        }

        var lineas = lista.Select(x =>
        {
            var textoSemanas = x.SemanasConsecutivas == 1 ? "1 semana" : $"{x.SemanasConsecutivas} semanas";

            return $"• **{x.Nombre}** - No realizó culvert desde hace {textoSemanas}";
        });

        var texto = string.Join("\n", lineas);

        if (texto.Length > 3800)
            texto = texto[..3800] + "\n...";

        var embed = new EmbedBuilder()
            .WithTitle("⚠️ Personajes sin culvert por varias semanas")
            .WithDescription(texto)
            .WithColor(Color.Orange)
            .WithFooter($"Total: {lista.Count} | Filtro: {minimoSemanas}+ semanas seguidas")
            .Build();

        await message.Channel.SendMessageAsync(embed: embed);
    }

    private static RegistroInput ParsearLineaRegistro(string linea)
    {
        if (string.IsNullOrWhiteSpace(linea))
            return RegistroInput.Invalido();

        if (linea.Contains('|'))
        {
            var partes = linea.Split('|', StringSplitOptions.TrimEntries);

            if (partes.Length == 1)
            {
                return new RegistroInput
                {
                    EsValido = true,
                    Nombre = partes[0].Trim()
                };
            }

            if (partes.Length >= 3)
            {
                int? levelValue = null;

                if (int.TryParse(partes[2], out var parsedLevel))
                    levelValue = parsedLevel;

                return new RegistroInput
                {
                    EsValido = true,
                    Nombre = partes[0].Trim(),
                    Clase = partes[1].Trim(),
                    Level = levelValue
                };
            }

            return RegistroInput.Invalido();
        }

        var matches = Regex.Matches(linea, "\"([^\"]+)\"|([^\\s]+)");
        var valores = matches
            .Select(m => m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        if (valores.Count == 1)
        {
            return new RegistroInput
            {
                EsValido = true,
                Nombre = valores[0].Trim()
            };
        }

        if (valores.Count >= 3 && int.TryParse(valores[2], out var level))
        {
            return new RegistroInput
            {
                EsValido = true,
                Nombre = valores[0].Trim(),
                Clase = valores[1].Trim(),
                Level = level
            };
        }

        return RegistroInput.Invalido();
    }

    private async Task ProcesarProgresoAsync(SocketMessage message, string content)
    {
        var partes = content.Split(" ", 2);

        if (partes.Length < 2)
        {
            await message.Channel.SendMessageAsync("Uso: !progreso NombrePersonaje");
            return;
        }

        var nombre = partes[1].Trim();

        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPersonajeRepository>();
        var resultado = await repo.ObtenerProgresoPersonajeAsync(nombre);

        if (resultado == null)
        {
            await message.Channel.SendMessageAsync($"❌ Personaje '{nombre}' no encontrado.");
            return;
        }

        await message.Channel.SendMessageAsync(resultado);
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.StopAsync();
        await _client.LogoutAsync();
        await base.StopAsync(cancellationToken);
    }

    private class RegistroInput
    {
        public bool EsValido { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Clase { get; set; }
        public int? Level { get; set; }

        public static RegistroInput Invalido() => new()
        {
            EsValido = false
        };
    }
}