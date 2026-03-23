namespace DiscordBot.Domain.Entities;

public class MovimientoPuntos
{
    public int IdMovimientoPuntos { get; set; }
    public int IdPersonaje { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string UsuarioRegistro { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }

    public Personaje Personaje { get; set; } = null!;
}