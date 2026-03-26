# Stage 1: Build con SDK de .NET 10
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore DiscordBotSolution.sln
RUN dotnet publish src/DiscordBot.API/DiscordBot.API.csproj -c Release -o /app/publish

# Stage 2: Runtime con Playwright + .NET 10
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS runtime
WORKDIR /app

# Instalar dependencias de Playwright
RUN apt-get update && apt-get install -y \
    libnss3 libatk1.0-0 libatk-bridge2.0-0 libcups2 \
    libxcomposite1 libxdamage1 libxfixes3 libxrandr2 \
    libgbm1 libxkbcommon0 libpango-1.0-0 libcairo2 \
    libasound2 libwayland-client0 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Instalar los browsers de Playwright
RUN dotnet DiscordBot.API.dll --install-browsers || \
    pwsh -Command "playwright install --with-deps chromium" || true

ENTRYPOINT ["dotnet", "DiscordBot.API.dll"]