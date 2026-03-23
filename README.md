# NightfallBot 🎮

Bot de **Discord** para gestión de guild en **MapleStory**.

NightfallBot permite registrar personajes, controlar el **Culvert semanal**, administrar **puntos del guild**, manejar una **tienda de recompensas** y generar **estadísticas visuales del guild**.

---

# 🚀 Características

- Registro automático de personajes
- Scraping de MapleBot para obtener clase y level
- Registro de **Culvert semanal**
- Sistema automático de **puntos del guild**
- Ranking de jugadores
- **Tienda de recompensas**
- Dashboard visual del guild
- Estadísticas de progreso por personaje
- Registro masivo de datos

---

# 🧰 Tecnologías utilizadas

- **.NET 10**
- **Discord.Net**
- **PostgreSQL**
- **Entity Framework Core**
- **SkiaSharp**
- **MapleBot Scraping**

---

# 📦 Arquitectura

El proyecto sigue una arquitectura por capas.

```
DiscordBotSolution
│
├── src
│   ├── DiscordBot.API
│   ├── DiscordBot.Application
│   ├── DiscordBot.Domain
│   ├── DiscordBot.Infrastructure
│   └── DiscordBot.Utilities
│
├── docs
├── scripts
└── tests
```

### Capas

**Domain**
- Entidades del sistema.

**Application**
- Casos de uso.
- Interfaces de servicios y repositorios.

**Infrastructure**
- Implementaciones de base de datos
- Scraping
- Generación de dashboards

**API**
- Entrada del bot
- Comandos Discord

**Utilities**
- Helpers
- Cálculos
- utilidades

---

# ⚙️ Instalación

## Requisitos

- .NET 10
- PostgreSQL
- Token de Discord Bot

---

## Clonar repositorio

```bash
git clone <repository-url>
```

---

## Restaurar dependencias

```bash
dotnet restore
```

---

## Compilar proyecto

```bash
dotnet build
```

---

## Configurar bot

Crear archivo:

```
appsettings.json
```

Ejemplo:

```json
{
  "Discord": {
    "Token": "TOKEN_DEL_BOT"
  },
  "Database": {
    "Host": "localhost",
    "Port": 5432,
    "Database": "discordbotdb",
    "Username": "postgres",
    "Password": "password"
  }
}
```

---

## Ejecutar bot

```bash
dotnet run --project src/DiscordBot.API
```

---

# 📊 Sistema de semanas

El sistema usa semanas personalizadas:

```
Miércoles 7:00 PM (hora Perú)
```

Todo Culvert registrado después de esa hora pertenece a la **nueva semana**.

---

# 🎮 Comandos del Bot

## Registro de personajes

Registrar personaje automáticamente

```
!registrar Nombre
```

Ejemplo

```
!registrar Fõxy
```

---

Registro manual

```
!registrar Nombre "Clase" Level
```

Ejemplo

```
!registrar Annz "Shadower" 292
```

---

Registro masivo

```
!registrarmasivo
Nombre1
Nombre2
Nombre3
```

Ejemplo

```
!registrarmasivo
Fõxy
Annz
SalamRulez
```

---

# ⚔️ Culvert

Registrar Culvert individual

```
!culvert Nombre Score
```

Ejemplo

```
!culvert Fõxy 270000
```

---

Registrar Culvert masivo

```
!culvertmasivo
Nombre|Score
Nombre|Score
Nombre|Score
```

Ejemplo

```
!culvertmasivo
Fõxy|270000
Annz|170000
SalamRulez|100000
```

---

# 📈 Progreso

Ver progreso histórico

```
!progreso Nombre
```

Ejemplo

```
!progreso Fõxy
```

---

# 🏆 Rankings

Ranking por puntos

```
!toppuntos
```

---

Ranking Culvert semanal

```
!topculvert
```

---

# 💰 Puntos

Ver puntos de un jugador

```
!puntos Nombre
```

Ejemplo

```
!puntos Fõxy
```

---

# 🛒 Tienda

Ver tienda

```
!tienda
```

Comprar item

```
!comprar "Nombre Item"
```

Ejemplo

```
!comprar "Hard Cubes Service Pack x10"
```

---

# ⚠ Participación

Ver jugadores que no registraron Culvert

```
!faltantes
```

---

# 🏆 Récord del Guild

```
!record
```

---

# 📊 Dashboard

Generar dashboard visual del guild

```
!guildstatsimg
```

El bot enviará una imagen con:

- total personajes
- participaciones
- promedio culvert
- top jugadores
- faltantes

---

# 🗄 Base de Datos

Motor utilizado

```
PostgreSQL
```

Tablas principales

- personaje
- registro_semanal_culvert
- configuracion_puntaje_culvert
- tienda_item
- tienda_compra

---

# 📈 Sistema de puntos Culvert

Ejemplo de configuración

```
5k – 19k → 3 pts
20k – 39k → 5 pts
40k – 59k → 8 pts
60k – 79k → 12 pts
80k – 99k → 16 pts
100k – 119k → 20 pts
120k – 149k → 25 pts
150k – 179k → 30 pts
180k – 209k → 36 pts
210k – 239k → 42 pts
240k – 269k → 50 pts
270k+ → 60 pts
```

---

# 📚 Documentación

Más documentación en:

```
docs/
```

Incluye:

- comandos
- arquitectura
- base de datos
- instalación
- despliegue

---

# 🔮 Futuras mejoras

- gráficos de progreso (`!progresoimg`)
- OCR para leer scoreboard de Culvert
- dashboard web
- sistema de actividad del guild
- panel administrativo

---

# 🧑‍💻 Autor

Desarrollado para la guild **Nightfall**.