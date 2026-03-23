namespace DiscordBot.Domain.DTOs;

public class CulvertRegistroResultadoDto
{
    public string NombrePersonaje { get; set; } = string.Empty;
    public long CulvertScore { get; set; }
    public int PuntosGanados { get; set; }
    public int MonedasGanadas { get; set; }
    public int PuntosActuales { get; set; }
    public int MonedasActuales { get; set; }
    public int Anio { get; set; }
    public int Semana { get; set; }
    public bool FueActualizado { get; set; }
}