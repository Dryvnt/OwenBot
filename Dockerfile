FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
ARG TARGETARCH

WORKDIR /app

COPY OwenBot.sln Owenbot.sln
COPY OwenBot/OwenBot.csproj OwenBot/OwenBot.csproj
RUN dotnet restore -a $TARGETARCH

COPY . .
RUN dotnet publish OwenBot -a $TARGETARCH --no-restore -c Release -o out

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/runtime:8.0
LABEL org.opencontainers.image.source=https://github.com/dryvnt/owenbot

COPY --from=build-env /app/out /app
ENTRYPOINT ["/app/OwenBot"]
