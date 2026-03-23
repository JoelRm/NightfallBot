# Arquitectura del Proyecto

El proyecto sigue una arquitectura por capas.
src
│
├── DiscordBot.API
├── DiscordBot.Application
├── DiscordBot.Domain
├── DiscordBot.Infrastructure
└── DiscordBot.Utilities

---

# Domain

Entidades del sistema.

Ejemplo:

- Personaje
- RegistroSemanalCulvert
- TiendaItem

---

# Application

Casos de uso.

Interfaces:

- repositories
- services

DTOs.

---

# Infrastructure

Implementaciones:

- Entity Framework
- scraping MapleBot
- generación de dashboard
- acceso a base de datos

---

# API

Entrada del sistema.

Aquí vive:

- DiscordBotService
- comandos del bot
- configuración del host

---

# Utilities

Helpers y utilidades:

- WeekHelper
- formateos
- cálculos