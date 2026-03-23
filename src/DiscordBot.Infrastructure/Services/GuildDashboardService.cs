using DiscordBot.Application.Interfaces.Repositories;
using DiscordBot.Application.Interfaces.Services;
using SkiaSharp;

namespace DiscordBot.Infrastructure.Services;

public class GuildDashboardService : IGuildDashboardService
{
    private readonly IPersonajeRepository _personajeRepository;

    public GuildDashboardService(IPersonajeRepository personajeRepository)
    {
        _personajeRepository = personajeRepository;
    }

    public async Task<string> GenerarDashboardAsync(int anio, int semana)
    {
        var stats = await _personajeRepository.ObtenerGuildStatsAsync();
        var topPuntos = await _personajeRepository.ObtenerTopPuntosAsync(5);
        var topCulvert = await _personajeRepository.ObtenerTopCulvertSemanalAsync(anio, semana, 5);
        var faltantes = await _personajeRepository.ObtenerFaltantesSemanaAsync(anio, semana);

        const int width = 1400;
        const int height = 900;

        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);

        canvas.Clear(new SKColor(16, 18, 24));

        var titlePaint = new SKPaint
        {
            Color = SKColors.White,
            TextSize = 38,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
        };

        var subTitlePaint = new SKPaint
        {
            Color = new SKColor(180, 190, 210),
            TextSize = 22,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Arial")
        };

        var cardTitlePaint = new SKPaint
        {
            Color = SKColors.White,
            TextSize = 24,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
        };

        var textPaint = new SKPaint
        {
            Color = new SKColor(225, 230, 240),
            TextSize = 20,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Arial")
        };

        var smallPaint = new SKPaint
        {
            Color = new SKColor(180, 190, 210),
            TextSize = 18,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Arial")
        };

        var accentPaint = new SKPaint
        {
            Color = new SKColor(88, 166, 255),
            IsAntialias = true
        };

        var greenPaint = new SKPaint
        {
            Color = new SKColor(46, 204, 113),
            IsAntialias = true
        };

        var cardPaint = new SKPaint
        {
            Color = new SKColor(27, 31, 42),
            IsAntialias = true
        };

        canvas.DrawText("Nightfall Dashboard", 40, 60, titlePaint);
        canvas.DrawText($"Semana {semana}/{anio}", 40, 95, subTitlePaint);

        DrawCard(canvas, 40, 130, 300, 120, cardPaint);
        DrawCard(canvas, 370, 130, 300, 120, cardPaint);
        DrawCard(canvas, 700, 130, 300, 120, cardPaint);
        DrawCard(canvas, 1030, 130, 300, 120, cardPaint);

        canvas.DrawText("Personajes", 60, 170, smallPaint);
        canvas.DrawText(stats.TotalPersonajes.ToString("N0"), 60, 215, titlePaint);

        canvas.DrawText("Participaciones", 390, 170, smallPaint);
        canvas.DrawText(stats.TotalParticipaciones.ToString("N0"), 390, 215, titlePaint);

        canvas.DrawText("Total Culvert", 720, 170, smallPaint);
        canvas.DrawText(stats.TotalCulvert.ToString("N0"), 720, 215, titlePaint);

        canvas.DrawText("Promedio", 1050, 170, smallPaint);
        canvas.DrawText(stats.PromedioCulvert.ToString("N0"), 1050, 215, titlePaint);

        DrawCard(canvas, 40, 290, 620, 540, cardPaint);
        DrawCard(canvas, 700, 290, 620, 260, cardPaint);
        DrawCard(canvas, 700, 570, 620, 260, cardPaint);

        canvas.DrawText("Top Puntos", 60, 330, cardTitlePaint);
        canvas.DrawText("Top Culvert Semanal", 720, 330, cardTitlePaint);
        canvas.DrawText("Faltantes", 720, 610, cardTitlePaint);

        DrawHorizontalBars(
            canvas,
            topPuntos.Select(x => x.NombrePersonaje).ToList(),
            topPuntos.Select(x => (double)x.Valor).ToList(),
            60, 360, 560,
            accentPaint, textPaint, smallPaint);

        DrawHorizontalBars(
            canvas,
            topCulvert.Select(x => x.NombrePersonaje).ToList(),
            topCulvert.Select(x => (double)x.Valor).ToList(),
            720, 360, 560,
            greenPaint, textPaint, smallPaint);

        float faltantesY = 650;
        var faltantesMostrar = faltantes.Take(8).ToList();

        if (!faltantesMostrar.Any())
        {
            canvas.DrawText("Todos registraron esta semana", 720, faltantesY, textPaint);
        }
        else
        {
            foreach (var nombre in faltantesMostrar)
            {
                canvas.DrawText($"• {nombre}", 720, faltantesY, textPaint);
                faltantesY += 30;
            }
        }

        var footerPaint = new SKPaint
        {
            Color = new SKColor(120, 130, 145),
            TextSize = 16,
            IsAntialias = true
        };

        canvas.DrawText("Generated by NightfallBot", 40, 875, footerPaint);

        var outputPath = Path.Combine(Path.GetTempPath(), $"guild_dashboard_{anio}_{semana}.png");

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(outputPath);
        data.SaveTo(stream);

        return outputPath;
    }

    private static void DrawCard(SKCanvas canvas, float x, float y, float width, float height, SKPaint paint)
    {
        var rect = new SKRoundRect(new SKRect(x, y, x + width, y + height), 18, 18);
        canvas.DrawRoundRect(rect, paint);
    }

    private static void DrawHorizontalBars(
        SKCanvas canvas,
        List<string> labels,
        List<double> values,
        float x,
        float y,
        float width,
        SKPaint barPaint,
        SKPaint textPaint,
        SKPaint smallPaint)
    {
        if (!labels.Any() || !values.Any())
        {
            canvas.DrawText("Sin datos", x, y + 30, textPaint);
            return;
        }

        var max = values.Max();
        if (max <= 0) max = 1;

        const float rowHeight = 70;
        const float labelWidth = 170;
        const float valueWidth = 90;

        for (int i = 0; i < labels.Count; i++)
        {
            var rowY = y + (i * rowHeight);
            var barX = x + labelWidth;
            var barY = rowY + 12;
            var barAreaWidth = width - labelWidth - valueWidth - 20;
            var barWidth = (float)(values[i] / max) * barAreaWidth;

            var bgPaint = new SKPaint
            {
                Color = new SKColor(44, 52, 68),
                IsAntialias = true
            };

            canvas.DrawText(labels[i], x, rowY + 25, textPaint);

            canvas.DrawRoundRect(
                new SKRoundRect(new SKRect(barX, barY, barX + barAreaWidth, barY + 24), 8, 8),
                bgPaint);

            canvas.DrawRoundRect(
                new SKRoundRect(new SKRect(barX, barY, barX + barWidth, barY + 24), 8, 8),
                barPaint);

            canvas.DrawText(values[i].ToString("N0"), barX + barAreaWidth + 15, rowY + 30, smallPaint);
        }
    }
}