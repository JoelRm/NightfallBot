namespace DiscordBot.Domain.Entities;

public class GuildTiendaCompra
{
    public int IdCompra { get; set; }
    public int IdItem { get; set; }
    public int IdPersonaje { get; set; }
    public int Cantidad { get; set; }
    public int PuntosGastados { get; set; }
    public int MonedasGastadas { get; set; }
    public string UsuarioCompra { get; set; } = string.Empty;
    public DateTime FechaCompra { get; set; }

    public GuildTiendaItem Item { get; set; } = null!;
    public Personaje Personaje { get; set; } = null!;
}