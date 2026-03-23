using DiscordBot.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace DiscordBot.Infrastructure.Persistence;

public class ConnectionFactory
{
    private readonly DatabaseSettings _settings;

    public ConnectionFactory(IOptions<DatabaseSettings> settings)
    {
        _settings = settings.Value;
    }

    public string GetConnectionString()
    {
        return $"Host={_settings.Host};" +
               $"Port={_settings.Port};" +
               $"Database={_settings.Database};" +
               $"Username={_settings.Username};" +
               $"Password={_settings.Password};" +
               $"SSL Mode=Require;" +
               $"Trust Server Certificate=true";
    }
}