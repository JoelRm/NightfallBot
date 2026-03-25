namespace DiscordBot.Domain.Entities;

public class RecordatorioConfig
{
    public int IdRecordatorio { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal CanalId { get; set; }
    public string? RolMencion { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public int DiaSemana { get; set; }
    public int Hora { get; set; }
    public int Minuto { get; set; }
    public string ZonaHoraria { get; set; } = "America/Lima";
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}
