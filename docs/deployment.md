# Despliegue

El bot puede ejecutarse en:

- VPS
- Docker
- Railway
- Render
- Fly.io

---

# Ejecutar como servicio
dotnet publish -c Release
Luego ejecutar:
./DiscordBot.API
---

# Docker (opcional)

Crear Dockerfile.

Ejecutar:
docker build -t nightfallbot .
docker run nightfallbot

---

# Recomendación

Ejecutar en servidor Linux con:

- systemd
- pm2
- docker
