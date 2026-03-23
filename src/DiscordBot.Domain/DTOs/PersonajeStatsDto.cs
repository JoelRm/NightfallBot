namespace DiscordBot.Domain.DTOs;

public class PersonajeStatsDto
{
    public int IdPersonaje { get; set; }
    public string NombrePersonaje { get; set; } = string.Empty;
    public string? Clase { get; set; }
    public int Level { get; set; }
    public string? ImagenUrl { get; set; }
    public int PuntosActuales { get; set; }
    public int MonedasActuales { get; set; }
    public long PersonalBest { get; set; }
    public decimal PeriodAverage { get; set; }
    public int ParticipationCount { get; set; }
    public int TotalWeeks { get; set; }
    public decimal ParticipationRatio { get; set; }
    public long TotalCulvert { get; set; }
    public int RankingCulvert { get; set; }
}