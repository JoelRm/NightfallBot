namespace DiscordBot.Domain.Entities;

public class GuildTiendaItem
{
    public int IdItem { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public string NombreItem { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int CostoPuntos { get; set; }
    public int CostoMonedas { get; set; }
    public int? Stock { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}