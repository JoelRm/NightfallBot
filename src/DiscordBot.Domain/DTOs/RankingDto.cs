namespace DiscordBot.Domain.DTOs;

public class RankingDto
{
    public int Posicion { get; set; }
    public string NombrePersonaje { get; set; } = string.Empty;
    public long Valor { get; set; }
}