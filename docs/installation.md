# Instalación

## Requisitos

- .NET 10
- PostgreSQL
- Node.js (opcional)
- Discord Developer Application

---

# Clonar repositorio
git clone 
---

# Restaurar dependencias
dotnet restore

---

# Compilar proyecto
dotnet build

---

# Configurar bot

Crear archivo:
appsettings.json

Ejemplo:
{
“Discord”: {
“Token”: “TOKEN_DEL_BOT”
},
“Database”: {
“Host”: “localhost”,
“Port”: 5432,
“Database”: “discordbotdb”,
“Username”: “postgres”,
“Password”: “password”
}
}

---

# Ejecutar bot
dotnet run –project src/DiscordBot.API

