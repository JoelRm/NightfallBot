using DiscordBot.Domain.DTOs;
using System.Text.Json;
using static System.Net.WebRequestMethods;

public static class ChartHelper
{
    private static readonly HttpClient _http = new HttpClient();
    public static string BuildCulvertProgressBarChartUrl(
        IEnumerable<PersonajeProgresoDto> progreso,
        string nombrePersonaje)
    {
        if (progreso == null)
            return string.Empty;

        var ordered = progreso
            .OrderBy(x => x.Anio)
            .ThenBy(x => x.Semana)
            .ToList();

        if (!ordered.Any())
            return string.Empty;

        var first = ordered.First();
        var last = ordered.Last();

        // Si tiene menos de 10 registros históricos, anclamos la ventana en la primera semana real
        // para que se vean barras grises al inicio.
        bool tieneMenosDe10Registros = ordered.Count < 10;

        int startAnio;
        int startSemana;

        if (tieneMenosDe10Registros)
        {
            // Queremos que la primera semana real quede más a la derecha,
            // así aparecen huecos grises al inicio.
            (startAnio, startSemana) = RestarSemanas(first.Anio, first.Semana, 10 - ordered.Count);
        }
        else
        {
            // Ventana normal: últimas 10 semanas calendario desde la última real
            (startAnio, startSemana) = RestarSemanas(last.Anio, last.Semana, 9);
        }

        var lookup = ordered.ToDictionary(
            x => $"{x.Anio}-{x.Semana}",
            x => x);

        var ultimas = new List<PersonajeProgresoDto>();

        int anio = startAnio;
        int semana = startSemana;

        for (int i = 0; i < 10; i++)
        {
            var key = $"{anio}-{semana}";

            if (lookup.TryGetValue(key, out var found))
            {
                ultimas.Add(found);
            }
            else
            {
                ultimas.Add(new PersonajeProgresoDto
                {
                    Anio = anio,
                    Semana = semana,
                    CulvertScore = 0
                });
            }

            (anio, semana) = SumarUnaSemana(anio, semana);
        }

        var labels = ultimas
            .Select(x => $"{x.Semana}/{x.Anio}")
            .ToList();

        var maxRealValue = ultimas
            .Where(x => x.CulvertScore > 0)
            .Select(x => x.CulvertScore)
            .DefaultIfEmpty(100)
            .Max();

        var emptyBarValue = Math.Max(1, (int)(maxRealValue * 0.03));

        var values = ultimas
            .Select(x => x.CulvertScore == 0 ? emptyBarValue : x.CulvertScore)
            .ToList();

        var backgroundColors = ultimas
            .Select(x => x.CulvertScore == 0
                ? "rgba(107, 114, 128, 0.55)"
                : "rgba(147, 51, 234, 0.80)")
            .ToList();

        var borderColors = ultimas
            .Select(x => x.CulvertScore == 0
                ? "rgba(156, 163, 175, 0.9)"
                : "rgba(168, 85, 247, 1)")
            .ToList();

        var chartConfig = new
        {
            type = "bar",
            data = new
            {
                labels,
                datasets = new object[]
                {
                    new
                    {
                        label = "Culvert Score",
                        data = values,
                        backgroundColor = backgroundColors,
                        borderColor = borderColors,
                        borderWidth = 2,
                        borderRadius = 6
                    }
                }
            },
            options = new
            {
                responsive = true,
                animation = false,
                plugins = new
                {
                    legend = new
                    {
                        labels = new
                        {
                            color = "#D1D5DB"
                        }
                    },
                    title = new
                    {
                        display = true,
                        text = $"{nombrePersonaje} - últimas 10 semanas",
                        color = "#E5E7EB",
                        font = new
                        {
                            size = 18,
                            weight = "bold"
                        }
                    }
                },
                scales = new
                {
                    x = new
                    {
                        ticks = new
                        {
                            color = "#D1D5DB",
                            maxRotation = 0,
                            minRotation = 0
                        },
                        grid = new
                        {
                            color = "rgba(255,255,255,0.08)"
                        }
                    },
                    y = new
                    {
                        beginAtZero = true,
                        ticks = new
                        {
                            color = "#D1D5DB"
                        },
                        grid = new
                        {
                            color = "rgba(255,255,255,0.08)"
                        }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(chartConfig);
        var version = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        return $"https://quickchart.io/chart?width=900&height=420&backgroundColor=%232b2d31&c={Uri.EscapeDataString(json)}&v={version}";
    }

    private static (int anio, int semana) RestarSemanas(int anio, int semana, int cantidad)
    {
        for (int i = 0; i < cantidad; i++)
        {
            semana--;
            if (semana <= 0)
            {
                semana = 52;
                anio--;
            }
        }

        return (anio, semana);
    }

    private static (int anio, int semana) SumarUnaSemana(int anio, int semana)
    {
        semana++;
        if (semana > 52)
        {
            semana = 1;
            anio++;
        }

        return (anio, semana);
    }
    public static async Task<Stream?> DownloadChartAsync(string chartUrl)
    {
        try
        {
            var bytes = await _http.GetByteArrayAsync(chartUrl);
            return new MemoryStream(bytes);
        }
        catch
        {
            return null;
        }
    }
}