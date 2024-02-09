#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 5167
EXPOSE 5166

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

RUN apt update && apt install -y curl bash

RUN mkdir -p tmp && cd tmp && \
    curl -L -o sass_package.tar.gz https://github.com/sass/dart-sass/releases/download/1.55.0/dart-sass-1.55.0-linux-x64.tar.gz && \
    tar -xvf ./sass_package.tar.gz && \
    ls -la dart-sass && \
    cp dart-sass/sass /usr/bin/ && ls -la /usr/bin && /usr/bin/sass --version && \
    chmod +x /usr/bin/sass && \
    cd - && rm -Rf tmp

WORKDIR /src
COPY ["TemporalAirlinesConcept.Web/TemporalAirlinesConcept.Web.csproj", "TemporalAirlinesConcept.Web/"]
COPY ["TemporalAirlinesConcept.Common/TemporalAirlinesConcept.Common.csproj", "TemporalAirlinesConcept.Common/"]
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
