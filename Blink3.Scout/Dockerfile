FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Blink3.Scout/Blink3.Scout.csproj", "Blink3.Scout/"]
RUN dotnet restore "Blink3.Scout/Blink3.Scout.csproj"
COPY . .
WORKDIR "/src/Blink3.Scout"
RUN dotnet build "Blink3.Scout.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Blink3.Scout.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Blink3.Scout.dll"]
