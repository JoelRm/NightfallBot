FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore DiscordBotSolution.sln
RUN dotnet publish src/DiscordBot.API/DiscordBot.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "DiscordBot.API.dll"]