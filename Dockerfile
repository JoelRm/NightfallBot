FROM mcr.microsoft.com/playwright/dotnet:v1.52.0-jammy AS build
WORKDIR /src

COPY . .

RUN dotnet restore DiscordBotSolution.sln
RUN dotnet publish src/DiscordBot.API/DiscordBot.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/playwright/dotnet:v1.52.0-jammy
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "DiscordBot.API.dll"]