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

# 1. Primero instalar .NET 10 runtime
RUN apt-get update && apt-get install -y wget ca-certificates && \
    wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh && \
    chmod +x dotnet-install.sh && \
    ./dotnet-install.sh --channel 10.0 --runtime aspnetcore --install-dir /usr/local/dotnet && \
    rm dotnet-install.sh && \
    apt-get remove -y wget && \
    apt-get autoremove -y && \
    rm -rf /var/lib/apt/lists/*

ENV PATH="/usr/local/dotnet:$PATH"
ENV DOTNET_ROOT="/usr/local/dotnet"

# 2. Luego instalar chromium (ahora pwsh puede encontrar .NET 8 que ya viene en la imagen base)
RUN pwsh -Command "playwright install chromium"

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "DiscordBot.API.dll"]