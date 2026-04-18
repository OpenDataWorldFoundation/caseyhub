FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY CaseyHub.API/CaseyHub.API.csproj CaseyHub.API/
COPY CaseyHub.Core/CaseyHub.Core.csproj CaseyHub.Core/
COPY CaseyHub.Models/CaseyHub.Models.csproj CaseyHub.Models/
RUN dotnet restore CaseyHub.API/CaseyHub.API.csproj

COPY . .
RUN dotnet publish CaseyHub.API/CaseyHub.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CaseyHub.API.dll"]
