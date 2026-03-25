using DiscordBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Infrastructure.Data;

public class BotDbContext : DbContext
{
    public BotDbContext(DbContextOptions<BotDbContext> options) : base(options)
    {
    }

    public DbSet<Personaje> Personajes => Set<Personaje>();
    public DbSet<ConfiguracionPuntajeCulvert> ConfiguracionesPuntajeCulvert => Set<ConfiguracionPuntajeCulvert>();
    public DbSet<RegistroSemanalCulvert> RegistrosSemanalesCulvert => Set<RegistroSemanalCulvert>();
    public DbSet<MovimientoPuntos> MovimientosPuntos => Set<MovimientoPuntos>();
    public DbSet<MovimientoMonedas> MovimientosMonedas => Set<MovimientoMonedas>();
    public DbSet<GuildTiendaItem> GuildTiendaItems { get; set; }
    public DbSet<GuildTiendaCompra> GuildTiendaCompras { get; set; }
    public DbSet<RecordatorioConfig> RecordatoriosConfig => Set<RecordatorioConfig>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Personaje>(entity =>
        {
            entity.ToTable("personaje");

            entity.HasKey(x => x.IdPersonaje);

            entity.Property(x => x.IdPersonaje)
                .HasColumnName("id_personaje");

            entity.Property(x => x.NombrePersonaje)
                .HasColumnName("nombre_personaje")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Clase)
                .HasColumnName("clase")
                .HasMaxLength(100);

            entity.Property(x => x.Level)
                .HasColumnName("level")
                .IsRequired();

            entity.Property(x => x.ImagenUrl)
                .HasColumnName("imagen_url")
                .HasMaxLength(500);

            entity.Property(x => x.PuntosActuales)
                .HasColumnName("puntos_actuales")
                .IsRequired();

            entity.Property(x => x.MonedasActuales)
                .HasColumnName("monedas_actuales")
                .IsRequired();

            entity.Property(x => x.Activo)
                .HasColumnName("activo")
                .IsRequired();

            entity.Property(x => x.FechaCreacion)
                .HasColumnName("fecha_creacion")
                .IsRequired();

            entity.Property(x => x.FechaActualizacion)
                .HasColumnName("fecha_actualizacion");

            entity.HasIndex(x => x.NombrePersonaje)
                .IsUnique()
                .HasDatabaseName("ux_personaje_nombre_personaje");
        });

        modelBuilder.Entity<ConfiguracionPuntajeCulvert>(entity =>
        {
            entity.ToTable("configuracion_puntaje_culvert");

            entity.HasKey(x => x.IdConfiguracion);

            entity.Property(x => x.IdConfiguracion)
                .HasColumnName("id_configuracion");

            entity.Property(x => x.PuntajeMinimo)
                .HasColumnName("puntaje_minimo")
                .IsRequired();

            entity.Property(x => x.PuntajeMaximo)
                .HasColumnName("puntaje_maximo");

            entity.Property(x => x.PuntosGanados)
                .HasColumnName("puntos_ganados")
                .IsRequired();

            entity.Property(x => x.MonedasGanadas)
                .HasColumnName("monedas_ganadas")
                .IsRequired();

            entity.Property(x => x.Activo)
                .HasColumnName("activo")
                .IsRequired();

            entity.Property(x => x.FechaCreacion)
                .HasColumnName("fecha_creacion")
                .IsRequired();
        });

        modelBuilder.Entity<RegistroSemanalCulvert>(entity =>
        {
            entity.ToTable("registro_semanal_culvert");

            entity.HasKey(x => x.IdRegistroSemanal);

            entity.Property(x => x.IdRegistroSemanal)
                .HasColumnName("id_registro_semanal");

            entity.Property(x => x.IdPersonaje)
                .HasColumnName("id_personaje")
                .IsRequired();

            entity.Property(x => x.Anio)
                .HasColumnName("anio")
                .IsRequired();

            entity.Property(x => x.Semana)
                .HasColumnName("semana")
                .IsRequired();

            entity.Property(x => x.CulvertScore)
                .HasColumnName("culvert_score")
                .IsRequired();

            entity.Property(x => x.PuntosGanados)
                .HasColumnName("puntos_ganados")
                .IsRequired();

            entity.Property(x => x.MonedasGanadas)
                .HasColumnName("monedas_ganadas")
                .IsRequired();

            entity.Property(x => x.Participa)
                .HasColumnName("participa")
                .IsRequired();

            entity.Property(x => x.UsuarioRegistro)
                .HasColumnName("usuario_registro")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.FechaRegistro)
                .HasColumnName("fecha_registro")
                .IsRequired();

            entity.Property(x => x.Observacion)
                .HasColumnName("observacion")
                .HasMaxLength(300);

            entity.HasIndex(x => new { x.IdPersonaje, x.Anio, x.Semana })
                .IsUnique()
                .HasDatabaseName("ux_registro_personaje_anio_semana");

            entity.HasOne(x => x.Personaje)
                .WithMany(x => x.RegistrosSemanales)
                .HasForeignKey(x => x.IdPersonaje)
                .HasConstraintName("fk_registro_personaje");
        });

        modelBuilder.Entity<MovimientoPuntos>(entity =>
        {
            entity.ToTable("movimiento_puntos");

            entity.HasKey(x => x.IdMovimientoPuntos);

            entity.Property(x => x.IdMovimientoPuntos)
                .HasColumnName("id_movimiento_puntos");

            entity.Property(x => x.IdPersonaje)
                .HasColumnName("id_personaje")
                .IsRequired();

            entity.Property(x => x.TipoMovimiento)
                .HasColumnName("tipo_movimiento")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.Cantidad)
                .HasColumnName("cantidad")
                .IsRequired();

            entity.Property(x => x.Motivo)
                .HasColumnName("motivo")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.UsuarioRegistro)
                .HasColumnName("usuario_registro")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.FechaRegistro)
                .HasColumnName("fecha_registro")
                .IsRequired();

            entity.HasOne(x => x.Personaje)
                .WithMany(x => x.MovimientosPuntos)
                .HasForeignKey(x => x.IdPersonaje)
                .HasConstraintName("fk_movimiento_puntos_personaje");
        });

        modelBuilder.Entity<MovimientoMonedas>(entity =>
        {
            entity.ToTable("movimiento_monedas");

            entity.HasKey(x => x.IdMovimientoMonedas);

            entity.Property(x => x.IdMovimientoMonedas)
                .HasColumnName("id_movimiento_monedas");

            entity.Property(x => x.IdPersonaje)
                .HasColumnName("id_personaje")
                .IsRequired();

            entity.Property(x => x.TipoMovimiento)
                .HasColumnName("tipo_movimiento")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.Cantidad)
                .HasColumnName("cantidad")
                .IsRequired();

            entity.Property(x => x.Motivo)
                .HasColumnName("motivo")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.UsuarioRegistro)
                .HasColumnName("usuario_registro")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.FechaRegistro)
                .HasColumnName("fecha_registro")
                .IsRequired();

            entity.HasOne(x => x.Personaje)
                .WithMany(x => x.MovimientosMonedas)
                .HasForeignKey(x => x.IdPersonaje)
                .HasConstraintName("fk_movimiento_monedas_personaje");
        });

        modelBuilder.Entity<GuildTiendaItem>(entity =>
        {
            entity.ToTable("guild_tienda_item");
            entity.HasKey(e => e.IdItem);

            entity.Property(e => e.IdItem).HasColumnName("id_item");
            entity.Property(e => e.NombreItem).HasColumnName("nombre_item");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.CostoPuntos).HasColumnName("costo_puntos");
            entity.Property(e => e.CostoMonedas).HasColumnName("costo_monedas");
            entity.Property(e => e.Stock).HasColumnName("stock");
            entity.Property(e => e.Activo).HasColumnName("activo");
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion");
            entity.Property(e => e.Categoria).HasColumnName("categoria");
        });

        modelBuilder.Entity<GuildTiendaCompra>(entity =>
        {
            entity.ToTable("guild_tienda_compra");
            entity.HasKey(e => e.IdCompra);

            entity.Property(e => e.IdCompra).HasColumnName("id_compra");
            entity.Property(e => e.IdItem).HasColumnName("id_item");
            entity.Property(e => e.IdPersonaje).HasColumnName("id_personaje");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.PuntosGastados).HasColumnName("puntos_gastados");
            entity.Property(e => e.MonedasGastadas).HasColumnName("monedas_gastadas");
            entity.Property(e => e.UsuarioCompra).HasColumnName("usuario_compra");
            entity.Property(e => e.FechaCompra).HasColumnName("fecha_compra");

            entity.HasOne(e => e.Item)
                .WithMany()
                .HasForeignKey(e => e.IdItem);

            entity.HasOne(e => e.Personaje)
                .WithMany()
                .HasForeignKey(e => e.IdPersonaje);
        });

        modelBuilder.Entity<RecordatorioConfig>(entity =>
        {
            entity.ToTable("recordatorio_config");
            entity.HasKey(e => e.IdRecordatorio);

            entity.Property(e => e.IdRecordatorio).HasColumnName("id_recordatorio");
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
            entity.Property(e => e.CanalId).HasColumnName("canal_id").IsRequired();
            entity.Property(e => e.RolMencion).HasColumnName("rol_mencion").HasMaxLength(100);
            entity.Property(e => e.Mensaje).HasColumnName("mensaje").HasMaxLength(1000).IsRequired();
            entity.Property(e => e.DiaSemana).HasColumnName("dia_semana").IsRequired();
            entity.Property(e => e.Hora).HasColumnName("hora").IsRequired();
            entity.Property(e => e.Minuto).HasColumnName("minuto").IsRequired();
            entity.Property(e => e.ZonaHoraria).HasColumnName("zona_horaria").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Activo).HasColumnName("activo").IsRequired();
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").IsRequired();

            entity.HasIndex(e => e.Nombre)
                .IsUnique()
                .HasDatabaseName("ux_recordatorio_nombre");
        });
    }
}