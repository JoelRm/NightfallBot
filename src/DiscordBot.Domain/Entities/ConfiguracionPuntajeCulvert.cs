namespace DiscordBot.Domain.Entities;

public class ConfiguracionPuntajeCulvert
{
    public int IdConfiguracion { get; set; }
    public long PuntajeMinimo { get; set; }
    public long? PuntajeMaximo { get; set; }
    public int PuntosGanados { get; set; }
    public int MonedasGanadas { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}