using DiscordBot.Domain.DTOs;
using DiscordBot.Domain.Entities;

namespace DiscordBot.Application.Interfaces.Repositories;

public interface IPersonajeRepository
{
    Task<Personaje?> ObtenerPorNombreAsync(string nombrePersonaje);
    Task<PersonajeStatsDto?> ObtenerStatsPorNombreAsync(string nombrePersonaje);
    Task<List<PersonajeProgresoDto>> ObtenerProgresoPorNombreAsync(string nombrePersonaje);
    Task ActualizarImagenAsync(int idPersonaje, string imagenUrl);
    Task<bool> ExistePorNombreAsync(string nombrePersonaje);
    Task<Personaje> CrearAsync(Personaje personaje);
    Task<CulvertRegistroResultadoDto?> RegistrarCulvertSemanalAsync(
    string nombrePersonaje,
    long culvertScore,
    string usuarioRegistro,
    int anio,
    int semana,
    DateTime fechaRegistro);
    Task<List<RankingDto>> ObtenerTopPuntosAsync(int top = 10);
    Task<List<RankingDto>> ObtenerTopCulvertSemanalAsync(int anio, int semana, int top = 10);
    Task<PersonajeResumenDto?> ObtenerResumenPersonajeAsync(string nombrePersonaje);
    Task<RankingDto?> ObtenerRecordSemanalAsync(int anio, int semana);
    Task<List<string>> ObtenerFaltantesSemanaAsync(int anio, int semana);
    Task<List<GuildTiendaItemDto>> ObtenerItemsTiendaAsync();
    Task<CompraResultadoDto> ComprarItemAsync(string nombrePersonaje, string nombreItem, string usuarioCompra);
    Task<GuildStatsDto> ObtenerGuildStatsAsync();
    Task<string?> ObtenerProgresoPersonajeAsync(string nombrePersonaje);
    Task<List<PersonajeCulvertCeroDto>> ObtenerSinCulvertPorSemanaAsync(int anio, int semana);
}