namespace DiscordBot.Domain.Entities;

public class Personaje
{
    public int IdPersonaje { get; set; }
    public string NombrePersonaje { get; set; } = string.Empty;
    public string? Clase { get; set; }
    public int Level { get; set; }
    public string? ImagenUrl { get; set; }
    public int PuntosActuales { get; set; }
    public int MonedasActuales { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }

    public ICollection<RegistroSemanalCulvert> RegistrosSemanales { get; set; } = new List<RegistroSemanalCulvert>();
    public ICollection<MovimientoPuntos> MovimientosPuntos { get; set; } = new List<MovimientoPuntos>();
    public ICollection<MovimientoMonedas> MovimientosMonedas { get; set; } = new List<MovimientoMonedas>();
}