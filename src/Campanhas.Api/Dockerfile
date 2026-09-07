FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ConexaoSolidaria.sln ./
COPY src/ConexaoSolidaria.Domain/ConexaoSolidaria.Domain.csproj src/ConexaoSolidaria.Domain/
COPY src/Campanhas.Api/Campanhas.Api.csproj src/Campanhas.Api/
COPY src/Doacoes.Worker/Doacoes.Worker.csproj src/Doacoes.Worker/
COPY tests/ConexaoSolidaria.Domain.Tests/ConexaoSolidaria.Domain.Tests.csproj tests/ConexaoSolidaria.Domain.Tests/
RUN dotnet restore src/Campanhas.Api/Campanhas.Api.csproj
COPY . .
RUN dotnet publish src/Campanhas.Api/Campanhas.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Campanhas.Api.dll"]
