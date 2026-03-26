# Stage 1: Build con SDK de .NET 10
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore DiscordBotSolution.sln
RUN dotnet publish src/DiscordBot.API/DiscordBot.API.csproj -c Release -o /app/publish

# Stage 2: Usar imagen oficial de Playwright (ya tiene browsers + deps)
# e instalarle el runtime de .NET 10 encima
FROM mcr.microsoft.com/playwright/dotnet:v1.52.0-noble AS runtime
WORKDIR /app

# Instalar .NET 10 runtime sobre la imagen de Playwright
RUN apt-get update && apt-get install -y wget && \
    wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh && \
    chmod +x dotnet-install.sh && \
    ./dotnet-install.sh --channel 10.0 --runtime aspnetcore --install-dir /usr/local/dotnet && \
    rm dotnet-install.sh && \
    rm -rf /var/lib/apt/lists/*

ENV PATH="$PATH:/usr/local/dotnet"
ENV DOTNET_ROOT="/usr/local/dotnet"

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "DiscordBot.API.dll"]