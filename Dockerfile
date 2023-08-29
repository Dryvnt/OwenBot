LABEL org.opencontainers.image.source=https://github.com/dryvnt/owenbot

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app

COPY OwenBot.sln Owenbot.sln
COPY OwenBot/OwenBot.csproj OwenBot/OwenBot.csproj
RUN dotnet restore

COPY . .
RUN dotnet publish OwenBot -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:7.0
COPY --from=build-env /app/out /app
ENTRYPOINT ["/app/OwenBot"]
