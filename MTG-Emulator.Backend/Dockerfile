FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MTG-Emulator.Backend/MTG-Emulator.Backend.csproj", "MTG-Emulator.Backend/"]
RUN dotnet restore "MTG-Emulator.Backend/MTG-Emulator.Backend.csproj"
COPY . .
WORKDIR "/src/MTG-Emulator.Backend"
RUN dotnet build "./MTG-Emulator.Backend.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MTG-Emulator.Backend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MTG-Emulator.Backend.dll"]
