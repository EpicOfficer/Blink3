FROM nginx:alpine AS base
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Blink3.Web/Blink3.Web.csproj", "Blink3.Web/"]
RUN dotnet restore "Blink3.Web/Blink3.Web.csproj"
COPY . .
WORKDIR "/src/Blink3.Web"
RUN dotnet build "Blink3.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Blink3.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY Blink3.Web/nginx.conf /etc/nginx/nginx.conf
