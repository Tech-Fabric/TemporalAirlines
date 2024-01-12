#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 5167
EXPOSE 5166

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TemporalAirlinesConcept.Api/TemporalAirlinesConcept.Api.csproj", "TemporalAirlinesConcept.Api/"]
COPY ["TemporalAirlinesConcept.Common/TemporalAirlinesConcept.Common.csproj", "TemporalAirlinesConcept.Common/"]
RUN dotnet restore "./TemporalAirlinesConcept.Api/./TemporalAirlinesConcept.Api.csproj"
COPY . .
WORKDIR "/src/TemporalAirlinesConcept.Api"
RUN dotnet build "./TemporalAirlinesConcept.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TemporalAirlinesConcept.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set the ASPNETCORE_URLS environment variable to specify the desired port
#ENV HOST_PORT=8080

# Set the ASPNETCORE_URLS environment variable to include the host port
#ENV ASPNETCORE_URLS=http://+:5167
#ENV ASPNETCORE_URLS=http://0.0.0.0:${HOST_PORT};http://+:5167

ENTRYPOINT ["dotnet", "TemporalAirlinesConcept.Api.dll"]