using DiscordBot.Application.Interfaces.Repositories;
using DiscordBot.Domain.DTOs;
using DiscordBot.Domain.Entities;
using DiscordBot.Infrastructure.Data;
using DiscordBot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Infrastructure.Repositories;

public class PersonajeRepository : IPersonajeRepository
{
    private readonly BotDbContext _context;

    public PersonajeRepository(BotDbContext context)
    {
        _context = context;
    }

    public async Task<Personaje?> ObtenerPorNombreAsync(string nombrePersonaje)
    {
        return await _context.Personajes
            .FromSqlRaw(
                "SELECT * FROM personaje WHERE unaccent(lower(nombre_personaje)) = unaccent(lower({0}))",
                nombrePersonaje)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<PersonajeStatsDto?> ObtenerStatsPorNombreAsync(string nombrePersonaje)
    {
        var personaje = await ObtenerPorNombreAsync(nombrePersonaje);

        if (personaje is null)
            return null;

        var registros = await _context.RegistrosSemanalesCulvert
            .AsNoTracking()
            .Where(x => x.IdPersonaje == personaje.IdPersonaje)
            .OrderBy(x => x.Anio)
            .ThenBy(x => x.Semana)
            .ToListAsync();

        var participationCount = registros.Count(x => x.Participa);
        var totalWeeks = registros.Count;
        var personalBest = registros.Any() ? registros.Max(x => x.CulvertScore) : 0;
        var periodAverage = registros.Any() ? registros.Average(x => x.CulvertScore) : 0;
        var participationRatio = totalWeeks > 0
            ? Math.Round((decimal)participationCount * 100 / totalWeeks, 2)
            : 0;

        var totalCulvert = registros.Sum(x => x.CulvertScore);

        var rankingData = await _context.RegistrosSemanalesCulvert
            .AsNoTracking()
            .GroupBy(x => x.IdPersonaje)
            .Select(g => new
            {
                IdPersonaje = g.Key,
                TotalCulvert = g.Sum(x => x.CulvertScore)
            })
            .OrderByDescending(x => x.TotalCulvert)
            .ToListAsync();

        var rankingCulvert = rankingData
            .Select((x, index) => new
            {
                x.IdPersonaje,
                Ranking = index + 1
            })
            .FirstOrDefault(x => x.IdPersonaje == personaje.IdPersonaje)?.Ranking ?? 0;

        return new PersonajeStatsDto
        {
            IdPersonaje = personaje.IdPersonaje,
            NombrePersonaje = personaje.NombrePersonaje,
            Clase = personaje.Clase,
            Level = personaje.Level,
            ImagenUrl = personaje.ImagenUrl,
            PuntosActuales = personaje.PuntosActuales,
            MonedasActuales = personaje.MonedasActuales,
            PersonalBest = personalBest,
            PeriodAverage = Math.Round((decimal)periodAverage, 2),
            ParticipationCount = participationCount,
            TotalWeeks = totalWeeks,
            ParticipationRatio = participationRatio,
            TotalCulvert = totalCulvert,
            RankingCulvert = rankingCulvert
        };
    }

    public async Task<List<PersonajeProgresoDto>> ObtenerProgresoPorNombreAsync(string nombrePersonaje)
    {
        var personaje = await ObtenerPorNombreAsync(nombrePersonaje);

        if (personaje is null)
            return new List<PersonajeProgresoDto>();

        return await _context.RegistrosSemanalesCulvert
            .AsNoTracking()
            .Where(x => x.IdPersonaje == personaje.IdPersonaje)
            .OrderBy(x => x.Anio)
            .ThenBy(x => x.Semana)
            .Select(x => new PersonajeProgresoDto
            {
                Anio = x.Anio,
                Semana = x.Semana,
                CulvertScore = x.CulvertScore
            })
            .ToListAsync();
    }
    public async Task ActualizarImagenAsync(int idPersonaje, string imagenUrl)
    {
        var personaje = await _context.Personajes
            .FirstOrDefaultAsync(x => x.IdPersonaje == idPersonaje);

        if (personaje is null)
            return;

        personaje.ImagenUrl = imagenUrl;
        personaje.FechaActualizacion = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

        await _context.SaveChangesAsync();
    }
    public async Task<bool> ExistePorNombreAsync(string nombrePersonaje)
    {
        return await _context.Personajes
            .AnyAsync(x => EF.Functions.ILike(x.NombrePersonaje, nombrePersonaje));
    }

    public async Task<Personaje> CrearAsync(Personaje personaje)
    {
        _context.Personajes.Add(personaje);
        await _context.SaveChangesAsync();
        return personaje;
    }

    public async Task<CulvertRegistroResultadoDto?> RegistrarCulvertSemanalAsync(
    string nombrePersonaje,
    long culvertScore,
    string usuarioRegistro,
    int anio,
    int semana,
    DateTime fechaRegistro)
    {
        fechaRegistro = DateTime.SpecifyKind(fechaRegistro, DateTimeKind.Utc);

        var personaje = await _context.Personajes
            .FirstOrDefaultAsync(x =>
                x.Activo &&
                x.NombrePersonaje.ToLower() == nombrePersonaje.ToLower());

        if (personaje is null)
            return null;

        int puntosGanados = 0;
        int monedasGanadas = 0;

        if (culvertScore > 0)
        {
            var configuracion = await _context.ConfiguracionesPuntajeCulvert
                .AsNoTracking()
                .Where(x => x.Activo &&
                            culvertScore >= x.PuntajeMinimo &&
                            (x.PuntajeMaximo == null || culvertScore <= x.PuntajeMaximo))
                .OrderByDescending(x => x.PuntajeMinimo)
                .FirstOrDefaultAsync();

            if (configuracion is not null)
            {
                puntosGanados = configuracion.PuntosGanados;
                monedasGanadas = configuracion.MonedasGanadas;
            }
        }

        var participa = culvertScore > 0;

        var registroExistente = await _context.RegistrosSemanalesCulvert
            .FirstOrDefaultAsync(x =>
                x.IdPersonaje == personaje.IdPersonaje &&
                x.Anio == anio &&
                x.Semana == semana);

        bool fueActualizado = false;

        if (registroExistente is null)
        {
            var nuevoRegistro = new RegistroSemanalCulvert
            {
                IdPersonaje = personaje.IdPersonaje,
                Anio = anio,
                Semana = semana,
                CulvertScore = culvertScore,
                PuntosGanados = puntosGanados,
                MonedasGanadas = monedasGanadas,
                Participa = participa,
                Observacion = null,
                UsuarioRegistro = usuarioRegistro,
                FechaRegistro = fechaRegistro
            };

            _context.RegistrosSemanalesCulvert.Add(nuevoRegistro);

            personaje.PuntosActuales += puntosGanados;
            personaje.MonedasActuales += monedasGanadas;
        }
        else
        {
            fueActualizado = true;

            personaje.PuntosActuales -= registroExistente.PuntosGanados;
            personaje.MonedasActuales -= registroExistente.MonedasGanadas;

            registroExistente.CulvertScore = culvertScore;
            registroExistente.PuntosGanados = puntosGanados;
            registroExistente.MonedasGanadas = monedasGanadas;
            registroExistente.Participa = participa;
            registroExistente.UsuarioRegistro = usuarioRegistro;
            registroExistente.FechaRegistro = fechaRegistro;

            personaje.PuntosActuales += puntosGanados;
            personaje.MonedasActuales += monedasGanadas;
        }

        if (personaje.PuntosActuales < 0)
            personaje.PuntosActuales = 0;

        if (personaje.MonedasActuales < 0)
            personaje.MonedasActuales = 0;

        personaje.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new CulvertRegistroResultadoDto
        {
            NombrePersonaje = personaje.NombrePersonaje,
            CulvertScore = culvertScore,
            PuntosGanados = puntosGanados,
            MonedasGanadas = monedasGanadas,
            PuntosActuales = personaje.PuntosActuales,
            MonedasActuales = personaje.MonedasActuales,
            Anio = anio,
            Semana = semana,
            FueActualizado = fueActualizado
        };
    }
    public async Task<List<RankingDto>> ObtenerTopPuntosAsync(int top = 10)
    {
       var data = await _context.RegistrosSemanalesCulvert
            .AsNoTracking()
            .Join(_context.Personajes.Where(p => p.Activo),
                r => r.IdPersonaje,
                p => p.IdPersonaje,
                (r, p) => new
                {
                    p.NombrePersonaje,
                    r.PuntosGanados
                })
            .GroupBy(x => x.NombrePersonaje)
            .Select(g => new
            {
                NombrePersonaje = g.Key,
                Valor = (long)g.Sum(x => x.PuntosGanados)
            })
            .OrderByDescending(x => x.Valor)
            .ThenBy(x => x.NombrePersonaje)
            .Take(top)
            .ToListAsync();

        return data
            .Select((x, index) => new RankingDto
            {
                Posicion = index + 1,
                NombrePersonaje = x.NombrePersonaje,
                Valor = x.Valor
            })
            .ToList();
    }

    public async Task<List<RankingDto>> ObtenerTopCulvertSemanalAsync(int anio, int semana, int top = 10)
    {
        var data = await _context.RegistrosSemanalesCulvert
            .AsNoTracking()
            .Where(x => x.Anio == anio && x.Semana == semana)
            .Join(_context.Personajes,
                r => r.IdPersonaje,
                p => p.IdPersonaje,
                (r, p) => new
                {
                    p.NombrePersonaje,
                    Valor = r.CulvertScore
                })
            .OrderByDescending(x => x.Valor)
            .ThenBy(x => x.NombrePersonaje)
            .Take(top)
            .ToListAsync();

        return data
            .Select((x, index) => new RankingDto
            {
                Posicion = index + 1,
                NombrePersonaje = x.NombrePersonaje,
                Valor = x.Valor
            })
            .ToList();
    }

    public async Task<PersonajeResumenDto?> ObtenerResumenPersonajeAsync(string nombrePersonaje)
    {
        var personaje = await ObtenerPorNombreAsync(nombrePersonaje);

        if (personaje is null)
            return null;

        var registros = await _context.RegistrosSemanalesCulvert
            .AsNoTracking()
            .Where(x => x.IdPersonaje == personaje.IdPersonaje)
            .ToListAsync();

        return new PersonajeResumenDto
        {
            NombrePersonaje = personaje.NombrePersonaje,
            PuntosActuales = registros.Sum(x => x.PuntosGanados),
            MonedasActuales = registros.Sum(x => x.MonedasGanadas),
            Participaciones = registros.Count(x => x.Participa),
            PromedioCulvert = registros.Any() ? Math.Round((decimal)registros.Average(x => x.CulvertScore), 2) : 0,
            MejorCulvert = registros.Any() ? registros.Max(x => x.CulvertScore) : 0
        };
    }

    public async Task<RankingDto?> ObtenerRecordSemanalAsync(int anio, int semana)
    {
        var record = await _context.RegistrosSemanalesCulvert
            .AsNoTracking()
            .Where(x => x.Anio == anio && x.Semana == semana)
            .Join(_context.Personajes,
                r => r.IdPersonaje,
                p => p.IdPersonaje,
                (r, p) => new
                {
                    p.NombrePersonaje,
                    Valor = r.CulvertScore
                })
            .OrderByDescending(x => x.Valor)
            .ThenBy(x => x.NombrePersonaje)
            .FirstOrDefaultAsync();

        if (record is null)
            return null;

        return new RankingDto
        {
            Posicion = 1,
            NombrePersonaje = record.NombrePersonaje,
            Valor = record.Valor
        };
    }

    public async Task<List<string>> ObtenerFaltantesSemanaAsync(int anio, int semana)
    {
        var idsConRegistro = await _context.RegistrosSemanalesCulvert
            .AsNoTracking()
            .Where(x => x.Anio == anio && x.Semana == semana)
            .Select(x => x.IdPersonaje)
            .Distinct()
            .ToListAsync();

        return await _context.Personajes
            .AsNoTracking()
            .Where(x => x.Activo && !idsConRegistro.Contains(x.IdPersonaje))
            .OrderBy(x => x.NombrePersonaje)
            .Select(x => x.NombrePersonaje)
            .ToListAsync();
    }

    public async Task<List<GuildTiendaItemDto>> ObtenerItemsTiendaAsync()
    {
        return await _context.GuildTiendaItems
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.Categoria)
            .ThenBy(x => x.CostoPuntos)
            .ThenBy(x => x.NombreItem)
            .Select(x => new GuildTiendaItemDto
            {
                IdItem = x.IdItem,
                Categoria = x.Categoria,
                NombreItem = x.NombreItem,
                Descripcion = x.Descripcion,
                CostoPuntos = x.CostoPuntos,
                CostoMonedas = x.CostoMonedas,
                Stock = x.Stock
            })
            .ToListAsync();
    }

    public async Task<CompraResultadoDto> ComprarItemAsync(string nombrePersonaje, string nombreItem, string usuarioCompra)
    {
        var personaje = await ObtenerPorNombreAsync(nombrePersonaje);

        if (personaje is null)
        {
            return new CompraResultadoDto
            {
                Ok = false,
                Mensaje = $"No encontré al personaje {nombrePersonaje}."
            };
        }

        var item = await _context.GuildTiendaItems
            .FirstOrDefaultAsync(x => x.Activo && x.NombreItem.ToLower() == nombreItem.ToLower());

        if (item is null)
        {
            return new CompraResultadoDto
            {
                Ok = false,
                Mensaje = $"No encontré el item {nombreItem}."
            };
        }

        if (item.Stock.HasValue && item.Stock.Value <= 0)
        {
            return new CompraResultadoDto
            {
                Ok = false,
                Mensaje = $"El item {item.NombreItem} no tiene stock."
            };
        }

        if (personaje.PuntosActuales < item.CostoPuntos)
        {
            return new CompraResultadoDto
            {
                Ok = false,
                Mensaje = $"{personaje.NombrePersonaje} no tiene puntos suficientes."
            };
        }

        if (personaje.MonedasActuales < item.CostoMonedas)
        {
            return new CompraResultadoDto
            {
                Ok = false,
                Mensaje = $"{personaje.NombrePersonaje} no tiene monedas suficientes."
            };
        }

        personaje.PuntosActuales -= item.CostoPuntos;
        personaje.MonedasActuales -= item.CostoMonedas;
        personaje.FechaActualizacion = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

        if (item.Stock.HasValue)
            item.Stock -= 1;

        var compra = new GuildTiendaCompra
        {
            IdItem = item.IdItem,
            IdPersonaje = personaje.IdPersonaje,
            Cantidad = 1,
            PuntosGastados = item.CostoPuntos,
            MonedasGastadas = item.CostoMonedas,
            UsuarioCompra = usuarioCompra,
            FechaCompra = DateTime.UtcNow
        };

        _context.GuildTiendaCompras.Add(compra);

        await _context.SaveChangesAsync();

        return new CompraResultadoDto
        {
            Ok = true,
            Mensaje = "Compra realizada correctamente.",
            NombrePersonaje = personaje.NombrePersonaje,
            NombreItem = item.NombreItem,
            PuntosRestantes = personaje.PuntosActuales,
            MonedasRestantes = personaje.MonedasActuales
        };
    }

    public async Task<GuildStatsDto> ObtenerGuildStatsAsync()
    {
        var totalPersonajes = await _context.Personajes
            .AsNoTracking()
            .CountAsync(x => x.Activo);

        var registros = await _context.RegistrosSemanalesCulvert
            .AsNoTracking()
            .ToListAsync();

        return new GuildStatsDto
        {
            TotalPersonajes = totalPersonajes,
            TotalParticipaciones = registros.Count(x => x.Participa),
            TotalCulvert = registros.Sum(x => x.CulvertScore),
            PromedioCulvert = registros.Any() ? Math.Round((decimal)registros.Average(x => x.CulvertScore), 2) : 0
        };
    }
    public async Task<string?> ObtenerProgresoPersonajeAsync(string nombrePersonaje)
    {
        var personaje = await _context.Personajes
            .FirstOrDefaultAsync(x => x.NombrePersonaje.ToLower() == nombrePersonaje.ToLower());

        if (personaje == null)
            return null;

        var registros = await _context.RegistrosSemanalesCulvert
            .Where(x => x.IdPersonaje == personaje.IdPersonaje)
            .OrderBy(x => x.Anio)
            .ThenBy(x => x.Semana)
            .ToListAsync();

        if (!registros.Any())
            return $"⚠ {personaje.NombrePersonaje} no tiene registros de Culvert.";

        var promedio = registros.Average(x => x.CulvertScore);
        var mejor = registros.MaxBy(x => x.CulvertScore);

        var ultima = registros.Last();
        var anterior = registros.Count > 1 ? registros[^2] : null;

        string tendencia = "Sin comparación";

        if (anterior != null)
        {
            var diff = ultima.CulvertScore - anterior.CulvertScore;

            if (diff > 0)
                tendencia = $"+{diff:N0} 📈";
            else if (diff < 0)
                tendencia = $"{diff:N0} 📉";
            else
                tendencia = "Sin cambios ➡";
        }

        var ultimasSemanas = registros
            .TakeLast(5)
            .Reverse()
            .Select(x =>
                $"Semana {x.Semana} → {x.CulvertScore:N0} | +{x.PuntosGanados} pts");

        var historial = string.Join("\n", ultimasSemanas);

        return $"""
    📈 Progreso de {personaje.NombrePersonaje}

    {historial}

    Promedio: {promedio:N0}
    Mejor semana: {mejor?.CulvertScore:N0}
    Tendencia: {tendencia}
    """;
    }

    public async Task<List<CulvertCeroRachaDto>> ObtenerRachaCulvertCeroAsync(int minimoSemanas)
    {
        var data = await _context.Personajes
            .Where(p => p.Activo)
            .Select(p => new
            {
                p.NombrePersonaje,
                Registros = _context.RegistrosSemanalesCulvert
                    .Where(r => r.IdPersonaje == p.IdPersonaje)
                    .OrderByDescending(r => r.Anio)
                    .ThenByDescending(r => r.Semana)
                    .Select(r => r.CulvertScore)
                    .ToList()
            })
            .ToListAsync();

        var resultado = new List<CulvertCeroRachaDto>();

        foreach (var item in data)
        {
            int contador = 0;

            foreach (var score in item.Registros)
            {
                if (score == 0)
                    contador++;
                else
                    break; // rompe cuando encuentra uno > 0
            }

            if (contador >= minimoSemanas)
            {
                resultado.Add(new CulvertCeroRachaDto
                {
                    Nombre = item.NombrePersonaje,
                    SemanasConsecutivas = contador
                });
            }
        }

        return resultado
            .OrderByDescending(x => x.SemanasConsecutivas)
            .ThenBy(x => x.Nombre)
            .ToList();
    }
}