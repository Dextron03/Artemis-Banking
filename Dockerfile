FROM  mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

COPY ["ArtemisBanking/ArtemisBanking.csproj", "ArtemisBanking/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["Application/Application.csproj", "Application/"]

RUN dotnet restore "ArtemisBanking/ArtemisBanking.csproj"

COPY . .
WORKDIR "src/ArtemisBanking"
RUN dotnet build "ArtemisBanking.csproj" -c Release -o /app/publish

# STAGE de PUBLISH
FROM build AS publish
RUN dotnet publish "ArtemisBanking.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app
EXPOSE 8000

COPY --from=publish /app/publish .
ENTRYPOINT [ "dotnet", "ArtemisBanking.csproj" ]