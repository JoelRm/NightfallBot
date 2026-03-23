namespace DiscordBot.Domain.DTOs
{
    public class PersonajeCulvertCeroDto
    {
        public int IdPersonaje { get; set; }
        public string Nombre { get; set; } = "";
        public int Level { get; set; }
        public string Clase { get; set; } = "";
        public int PuntosActuales { get; set; }
    }
}
