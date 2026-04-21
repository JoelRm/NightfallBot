using DiscordBot.Domain.DTOs;
using System.Globalization;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;

public static class ChartHelper
{
    private static readonly HttpClient _http = new HttpClient();
    public static string BuildCulvertProgressBarChartConfig(
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

        bool tieneMenosDe10Registros = ordered.Count < 10;

        int startAnio;
        int startSemana;

        if (tieneMenosDe10Registros)
        {
            (startAnio, startSemana) = RestarSemanas(first.Anio, first.Semana, 10 - ordered.Count);
        }
        else
        {
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

        var emptyBarValue = Math.Max(1, (int)(maxRealValue * 0.012));

        var values = ultimas
            .Select(x => x.CulvertScore == 0 ? emptyBarValue : x.CulvertScore)
            .ToList();

        var backgroundColors = ultimas
            .Select(x => x.CulvertScore == 0
                ? "rgba(75, 85, 99, 0.55)"
                : "rgba(139, 92, 246, 0.92)")
            .ToList();

        var borderColors = ultimas
            .Select(x => x.CulvertScore == 0
                ? "rgba(148, 163, 184, 0.75)"
                : "rgba(196, 181, 253, 1)")
            .ToList();

        var formattedLabels = ultimas
            .Select(x => x.CulvertScore == 0 ? "" : FormatCompact(x.CulvertScore))
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

                    // 🎯 SOLO ARRIBA
                    borderRadius = new
                    {
                        topLeft = 10,
                        topRight = 10,
                        bottomLeft = 0,
                        bottomRight = 0
                    },
                    borderSkipped = "bottom",

                    // 🎨 FORMA MÁS PRO
                    barThickness = 42,
                    maxBarThickness = 42,
                    categoryPercentage = 0.78,
                    barPercentage = 0.92,

                    datalabels = new
                    {
                        display = true,
                        anchor = "end",
                        align = "top",
                        offset = 8,
                        color = "#F9FAFB",
                        font = new
                        {
                            size = 12,
                            weight = "bold"
                        },
                        formatter = "__DATALABELS_FORMATTER__"
                    }
                }
        }
            },
            options = new
            {
                responsive = true,
                animation = false,
                layout = new
                {
                    padding = new
                    {
                        top = 30,
                        right = 16,
                        left = 16,
                        bottom = 8
                    }
                },
                plugins = new
                {
                    legend = new
                    {
                        display = true,
                        labels = new
                        {
                            color = "#E5E7EB",
                            boxWidth = 14,
                            boxHeight = 14,
                            padding = 18,
                            font = new
                            {
                                size = 12,
                                weight = "bold"
                            }
                        }
                    },
                    title = new
                    {
                        display = true,
                        text = $"{nombrePersonaje} · últimas 10 semanas",
                        color = "#F9FAFB",
                        font = new
                        {
                            size = 20,
                            weight = "bold"
                        },
                        padding = new
                        {
                            top = 8,
                            bottom = 18
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
                            minRotation = 0,
                            font = new
                            {
                                size = 11,
                                weight = "bold"
                            }
                        },
                        grid = new
                        {
                            display = false
                        },
                        border = new
                        {
                            color = "rgba(255,255,255,0.10)"
                        }
                    },
                    y = new
                    {
                        beginAtZero = true,
                        grace = "18%",
                        ticks = new
                        {
                            color = "#D1D5DB",
                            padding = 8,
                            font = new
                            {
                                size = 11
                            },
                            callback = "__Y_TICKS_CALLBACK__"
                        },
                        grid = new
                        {
                            color = "rgba(255,255,255,0.06)",
                            lineWidth = 1
                        },
                        border = new
                        {
                            color = "rgba(255,255,255,0.10)"
                        }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(chartConfig);

        json = json.Replace(
            "\"__DATALABELS_FORMATTER__\"",
            $"function(value, context) {{ var labels = {JsonSerializer.Serialize(formattedLabels)}; return labels[context.dataIndex]; }}"
        );

        json = json.Replace(
            "\"__Y_TICKS_CALLBACK__\"",
            "function(value) { if (value >= 1000000) return (value/1000000).toFixed(1).replace('.', ',') + 'M'; if (value >= 1000) return (value/1000).toFixed(1).replace('.', ',') + 'k'; return value; }"
        );

        return json;
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
    public static async Task<Stream?> DownloadChartAsync(string chartConfigJson)
    {
        try
        {
            var payload =
            "{"
            + "\"width\":900,"
            + "\"height\":420,"
            + "\"backgroundColor\":\"#2b2d31\","
            + "\"format\":\"png\","
            + "\"version\":\"4\","
            + "\"chart\":" + JsonSerializer.Serialize(chartConfigJson)
            + "}";

            using var content = new StringContent(payload, Encoding.UTF8, "application/json");
            using var response = await _http.PostAsync("https://quickchart.io/chart", content);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ChartHelper] Error HTTP {(int)response.StatusCode}: {body}");
                return null;
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();

            if (bytes == null || bytes.Length == 0)
            {
                Console.WriteLine("[ChartHelper] Imagen vacía.");
                return null;
            }

            return new MemoryStream(bytes);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChartHelper] Exception: {ex}");
            return null;
        }
    }
    private static string FormatCompact(long value)
    {
        if (value >= 1_000_000)
            return (value / 1_000_000.0).ToString("0.#", CultureInfo.InvariantCulture).Replace(".", ",") + "M";

        if (value >= 1_000)
            return (value / 1_000.0).ToString("0.#", CultureInfo.InvariantCulture).Replace(".", ",") + "k";

        return value.ToString(CultureInfo.InvariantCulture);
    }
}