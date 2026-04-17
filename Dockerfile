# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore DiscordBotSolution.sln
RUN dotnet publish src/DiscordBot.API/DiscordBot.API.csproj -c Release -o /app/publish \
    --self-contained false \
    /p:PublishTrimmed=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS runtime
WORKDIR /app

# Forzar IPv4 y HTTPS para evitar timeouts de APT
RUN printf 'Acquire::ForceIPv4 "true";\nAcquire::Retries "5";\n' > /etc/apt/apt.conf.d/99force-ipv4 && \
    sed -i 's|http://archive.ubuntu.com/ubuntu|https://archive.ubuntu.com/ubuntu|g' /etc/apt/sources.list.d/ubuntu.sources && \
    sed -i 's|http://security.ubuntu.com/ubuntu|https://security.ubuntu.com/ubuntu|g' /etc/apt/sources.list.d/ubuntu.sources

# Dependencias Chromium
RUN apt-get update && apt-get install -y --no-install-recommends \
    libnss3 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libcups2t64 \
    libxcomposite1 \
    libxdamage1 \
    libxfixes3 \
    libxrandr2 \
    libgbm1 \
    libxkbcommon0 \
    libpango-1.0-0 \
    libcairo2 \
    libasound2t64 \
    libwayland-client0 \
    libxshmfence1 \
    libx11-xcb1 \
    libxcb-dri3-0 \
    fonts-liberation \
    wget \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Instalar PowerShell
RUN wget -q https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    rm packages-microsoft-prod.deb && \
    apt-get update && apt-get install -y --no-install-recommends powershell && \
    rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV PLAYWRIGHT_BROWSERS_PATH=/ms-playwright
RUN pwsh ./playwright.ps1 install chromium

ENTRYPOINT ["dotnet", "DiscordBot.API.dll"]