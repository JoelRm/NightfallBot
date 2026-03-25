using DiscordBot.Application.Interfaces.Repositories;
using DiscordBot.Domain.Entities;
using DiscordBot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Infrastructure.Repositories;

public class RecordatorioRepository : IRecordatorioRepository
{
    private readonly BotDbContext _context;

    public RecordatorioRepository(BotDbContext context)
    {
        _context = context;
    }

    public async Task<List<RecordatorioConfig>> ObtenerActivosAsync()
    {
        return await _context.RecordatoriosConfig
            .AsNoTracking()
            .Where(x => x.Activo)
            .ToListAsync();
    }
}
