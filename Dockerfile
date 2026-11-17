# Imagem de produção da API. Build: docker build -t ela-arquitetura-api .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restaura primeiro só com os .csproj, para o cache do Docker reaproveitar os pacotes.
COPY src/ElaArquitetura.Domain/ElaArquitetura.Domain.csproj src/ElaArquitetura.Domain/
COPY src/ElaArquitetura.Application/ElaArquitetura.Application.csproj src/ElaArquitetura.Application/
COPY src/ElaArquitetura.Infrastructure/ElaArquitetura.Infrastructure.csproj src/ElaArquitetura.Infrastructure/
COPY src/ElaArquitetura.Api/ElaArquitetura.Api.csproj src/ElaArquitetura.Api/
RUN dotnet restore src/ElaArquitetura.Api/ElaArquitetura.Api.csproj

COPY src/ src/
RUN dotnet publish src/ElaArquitetura.Api/ElaArquitetura.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# O driver do PostgreSQL (Npgsql) carrega essa biblioteca ao conectar; sem ela, loga erro a cada inicialização.
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

# Usuário sem privilégios que já vem na imagem oficial.
USER $APP_UID

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Configuração obrigatória, por variável de ambiente na plataforma de hospedagem:
#   ConnectionStrings__DefaultConnection, Jwt__Key, UsuariaInicial__Senha
# Atrás de proxy reverso com HTTPS (Render, Railway, Fly, Nginx...): ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
ENTRYPOINT ["dotnet", "ElaArquitetura.Api.dll"]
