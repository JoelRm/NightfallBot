using System.Net;
using System.Text.RegularExpressions;
using DiscordBot.Application.Interfaces.Services;
using DiscordBot.Domain.DTOs;
using Microsoft.Playwright;

namespace DiscordBot.Infrastructure.Services;

public class MapleBotImageService : ICharacterImageService
{
    public async Task<string?> ObtenerImagenPersonajeAsync(string nombrePersonaje)
    {
        var perfil = await ObtenerPerfilPersonajeAsync(nombrePersonaje);
        return perfil?.ImagenUrl;
    }

    public async Task<CharacterProfileDto?> ObtenerPerfilPersonajeAsync(string nombrePersonaje)
    {
        if (string.IsNullOrWhiteSpace(nombrePersonaje))
            return null;

        var nombreReal = nombrePersonaje.Trim();
        var encoded = WebUtility.UrlEncode(nombreReal);
        var url = $"https://maplebot.io/character/{encoded}?region=NA";

        Console.WriteLine($"[MapleBot] Buscando perfil: {url}");

        try
        {
            using var playwright = await Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

            var page = await browser.NewPageAsync(new BrowserNewPageOptions
            {
                UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 " +
                            "(KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36"
            });

            string? imageUrl = null;

            page.Response += (_, response) =>
            {
                var responseUrl = response.Url;

                if (responseUrl.Contains("cdn.maplebot.io/images/", StringComparison.OrdinalIgnoreCase) &&
                    responseUrl.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    imageUrl ??= responseUrl;
                }
            };

            await page.GotoAsync(url, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 20000
            });

            // Esperar a que deje de mostrar el loading inicial
            await page.WaitForTimeoutAsync(4000);

            var bodyText = await page.InnerTextAsync("body");
            Console.WriteLine($"[MapleBot] BODY TEXT:\n{bodyText}");

            if (bodyText.Contains("Loading character data...", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("[MapleBot] La página siguió en loading.");
                return new CharacterProfileDto
                {
                    NombrePersonaje = nombreReal,
                    Encontrado = false
                };
            }

            if (bodyText.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
                bodyText.Contains("no character", StringComparison.OrdinalIgnoreCase) ||
                bodyText.Contains("no results", StringComparison.OrdinalIgnoreCase))
            {
                return new CharacterProfileDto
                {
                    NombrePersonaje = nombreReal,
                    Encontrado = false
                };
            }

            // Intento por texto renderizado
            int? level = null;
            string? clase = null;

            var levelMatch = Regex.Match(bodyText, @"Level\s+(\d+)", RegexOptions.IgnoreCase);
            if (levelMatch.Success && int.TryParse(levelMatch.Groups[1].Value, out var parsedLevel))
            {
                level = parsedLevel;
            }

            var classMatch = Regex.Match(
                bodyText,
                @"LEVEL\s*\n\s*\d+\s*\n\s*([A-Za-zÀ-ÿ0-9\s\-\+]+)",
                RegexOptions.IgnoreCase);

            if (classMatch.Success)
            {
                clase = classMatch.Groups[1].Value.Trim();
            }

            if (!string.IsNullOrWhiteSpace(clase))
            {
                clase = clase
                    .Replace("•", "")
                    .Replace("(NA)", "")
                    .Trim();
            }

            // Intento extra: leer todo el HTML renderizado por si aparece una imagen en DOM
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                var html = await page.ContentAsync();

                var matchImage = Regex.Match(
                    html,
                    @"https://cdn\.maplebot\.io/images/[^\s""'<>]+\.png",
                    RegexOptions.IgnoreCase);

                if (matchImage.Success)
                {
                    imageUrl = matchImage.Value;
                }
            }

            Console.WriteLine($"[MapleBot] Clase detectada: {clase ?? "(null)"}");
            Console.WriteLine($"[MapleBot] Level detectado: {level?.ToString() ?? "(null)"}");
            Console.WriteLine($"[MapleBot] Imagen detectada: {imageUrl ?? "(null)"}");

            // REGRA IMPORTANTE:
            // Encontrado = true solo si logra al menos clase y level.
            if (string.IsNullOrWhiteSpace(clase) || !level.HasValue)
            {
                return new CharacterProfileDto
                {
                    NombrePersonaje = nombreReal,
                    Clase = clase,
                    Level = level,
                    ImagenUrl = imageUrl,
                    Encontrado = false
                };
            }

            return new CharacterProfileDto
            {
                NombrePersonaje = nombreReal,
                Clase = clase,
                Level = level,
                ImagenUrl = imageUrl,
                Encontrado = true
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MapleBot] Error obteniendo perfil de {nombreReal}: {ex}");
            return new CharacterProfileDto
            {
                NombrePersonaje = nombreReal,
                Encontrado = false
            };
        }
    }
}