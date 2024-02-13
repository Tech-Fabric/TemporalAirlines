#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 5167
EXPOSE 5166

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

RUN apt update && apt install -y curl && apt clean

RUN curl -L -o sass_package.tar.gz https://github.com/sass/dart-sass/releases/download/1.55.0/dart-sass-1.55.0-linux-x64.tar.gz
RUN tar -xvf sass_package.tar.gz
RUN ls -la dart-sass
RUN cp dart-sass/sass /usr/bin/ && /usr/bin/sass --version
RUN chmod +x /usr/bin/sass
RUN rm -Rf dart-sass sass_package.tar.gz

WORKDIR /src
COPY ["TemporalAirlinesConcept.Configuration/TemporalAirlinesConcept.Configuration.csproj", "TemporalAirlinesConcept.Configuration/"]
COPY ["TemporalAirlinesConcept.DAL/TemporalAirlinesConcept.DAL.csproj", "TemporalAirlinesConcept.DAL/"]
COPY ["TemporalAirlinesConcept.Services/TemporalAirlinesConcept.Services.csproj", "TemporalAirlinesConcept.Services/"]
COPY ["TemporalAirlinesConcept.Common/TemporalAirlinesConcept.Common.csproj", "TemporalAirlinesConcept.Common/"]
COPY ["TemporalAirlinesConcept.Web/TemporalAirlinesConcept.Web.csproj", "TemporalAirlinesConcept.Web/"]
RUN dotnet restore "./TemporalAirlinesConcept.Web/TemporalAirlinesConcept.Web.csproj"
COPY . .
WORKDIR "/src/TemporalAirlinesConcept.Web"
RUN dotnet build "./TemporalAirlinesConcept.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TemporalAirlinesConcept.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

CMD sleep 45 && dotnet TemporalAirlinesConcept.Web.dll
