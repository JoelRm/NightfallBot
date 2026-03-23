using System.Globalization;

namespace DiscordBot.Utilities.Helpers;

public static class WeekHelper
{
    public static (int anio, int semana, DateTime fechaUtc, DateTime fechaPeru) ObtenerSemanaActualPeru()
    {
        var peruTz = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsMacOS() || OperatingSystem.IsLinux()
                ? "America/Lima"
                : "SA Pacific Standard Time");

        var fechaUtc = DateTime.UtcNow;
        var fechaPeru = TimeZoneInfo.ConvertTimeFromUtc(fechaUtc, peruTz);

        // Corte semanal: miércoles 7:00 PM Perú
        var fechaAjustada = fechaPeru.AddDays(-2).AddHours(-19);

        var calendar = CultureInfo.InvariantCulture.Calendar;
        var semana = calendar.GetWeekOfYear(
            fechaAjustada,
            CalendarWeekRule.FirstFourDayWeek,
            DayOfWeek.Monday);

        var anio = fechaAjustada.Year;

        return (anio, semana, DateTime.SpecifyKind(fechaUtc, DateTimeKind.Utc), fechaPeru);
    }
}