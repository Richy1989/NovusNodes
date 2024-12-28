FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY NovusNodo/NovusNodo.csproj NovusNodo/
COPY NovusNodoCore/NovusNodoCore.csproj NovusNodoCore/
RUN dotnet restore NovusNodo/NovusNodo.csproj

#FROM node:alpine AS nodetailwind
#WORKDIR /src
#COPY . .
#WORKDIR /src/NovusNodo
#RUN npm run tailwind

FROM restore AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY . .
#COPY --from=nodetailwind /src .
WORKDIR /src/NovusNodo
RUN dotnet build NovusNodo.csproj -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish NovusNodo.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NovusNodo.dll"]