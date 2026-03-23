namespace DiscordBot.Domain.Entities;

public class PersonajeResumenDto
{
    public string NombrePersonaje { get; set; } = string.Empty;
    public int PuntosActuales { get; set; }
    public int MonedasActuales { get; set; }
    public int Participaciones { get; set; }
    public decimal PromedioCulvert { get; set; }
    public long MejorCulvert { get; set; }
}