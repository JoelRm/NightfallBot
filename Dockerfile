# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore DiscordBotSolution.sln
RUN dotnet publish src/DiscordBot.API/DiscordBot.API.csproj -c Release -o /app/publish \
    --self-contained false \
    /p:PublishTrimmed=false

# Stage 2: Runtime
FROM mcr.microsoft.com/playwright/dotnet:v1.52.0-noble AS runtime
WORKDIR /app

# Instalar .NET 10 en la misma carpeta que .NET 8
RUN wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh && \
    chmod +x dotnet-install.sh && \
    ./dotnet-install.sh --channel 10.0 --runtime aspnetcore --install-dir /usr/share/dotnet && \
    rm dotnet-install.sh

COPY --from=build /app/publish .

# Instalar chromium usando el playwright.ps1 que viene con tu app publicada
RUN pwsh playwright.ps1 install chromium

ENTRYPOINT ["dotnet", "DiscordBot.API.dll"]