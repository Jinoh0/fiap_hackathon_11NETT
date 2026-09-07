FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ConexaoSolidaria.sln ./
COPY src/ConexaoSolidaria.Domain/ConexaoSolidaria.Domain.csproj src/ConexaoSolidaria.Domain/
COPY src/Campanhas.Api/Campanhas.Api.csproj src/Campanhas.Api/
COPY src/Doacoes.Worker/Doacoes.Worker.csproj src/Doacoes.Worker/
COPY tests/ConexaoSolidaria.Domain.Tests/ConexaoSolidaria.Domain.Tests.csproj tests/ConexaoSolidaria.Domain.Tests/
RUN dotnet restore src/Doacoes.Worker/Doacoes.Worker.csproj
COPY . .
RUN dotnet publish src/Doacoes.Worker/Doacoes.Worker.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
EXPOSE 8081
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Doacoes.Worker.dll"]
