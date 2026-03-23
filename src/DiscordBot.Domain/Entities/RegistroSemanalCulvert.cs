namespace DiscordBot.Domain.Entities;

public class RegistroSemanalCulvert
{
    public int IdRegistroSemanal { get; set; }
    public int IdPersonaje { get; set; }
    public int Anio { get; set; }
    public int Semana { get; set; }
    public long CulvertScore { get; set; }
    public int PuntosGanados { get; set; }
    public int MonedasGanadas { get; set; }
    public bool Participa { get; set; }
    public string UsuarioRegistro { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public string? Observacion { get; set; }

    public Personaje Personaje { get; set; } = null!;
}