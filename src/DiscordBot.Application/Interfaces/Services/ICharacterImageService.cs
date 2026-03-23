using DiscordBot.Domain.DTOs;

namespace DiscordBot.Application.Interfaces.Services;

public interface ICharacterImageService
{
    Task<string?> ObtenerImagenPersonajeAsync(string nombrePersonaje);
    Task<CharacterProfileDto?> ObtenerPerfilPersonajeAsync(string nombrePersonaje);
}