namespace DiscordBot.Domain.DTOs;

public class CompraResultadoDto
{
    public bool Ok { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string? NombrePersonaje { get; set; }
    public string? NombreItem { get; set; }
    public int PuntosRestantes { get; set; }
    public int MonedasRestantes { get; set; }
}