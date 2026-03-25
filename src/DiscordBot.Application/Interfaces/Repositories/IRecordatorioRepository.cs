using DiscordBot.Domain.Entities;

namespace DiscordBot.Application.Interfaces.Repositories;

public interface IRecordatorioRepository
{
    Task<List<RecordatorioConfig>> ObtenerActivosAsync();
}
