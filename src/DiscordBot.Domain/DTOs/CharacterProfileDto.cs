namespace DiscordBot.Domain.DTOs;

public class CharacterProfileDto
{
    public string NombrePersonaje { get; set; } = string.Empty;
    public string? Clase { get; set; }
    public int? Level { get; set; }
    public string? ImagenUrl { get; set; }
    public bool Encontrado { get; set; }
}