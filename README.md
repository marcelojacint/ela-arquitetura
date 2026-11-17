# Éla Arquitetura — API de Gestão de Projetos

Backend do sistema de gestão de projetos da **Éla Arquitetura**, projeto acadêmico do curso de Sistemas para Internet (UNIESP).

## Objetivo

Centralizar em um único sistema a gestão dos projetos de arquitetura da empresa, hoje feita de forma manual e dividida entre várias ferramentas. A API dá suporte a um app mobile (React Native) e controla clientes, projetos, etapas, checklists e entregas.

## Stack

- .NET (C#) — ASP.NET Core Web API
- PostgreSQL
- Entity Framework Core
- JWT para autenticação
- Swagger

## Arquitetura

Clean Architecture:

```
Domain          → entidades e regras de negócio
Application     → casos de uso e DTOs
Infrastructure  → EF Core, repositórios, JWT
Api             → controllers, autenticação, Swagger
```

## Como rodar localmente

Pré-requisitos: .NET 10 SDK e Docker.

1. Suba o PostgreSQL:

   ```bash
   docker compose up -d
   ```

2. Configure os segredos (uma vez por máquina; ficam fora do repositório, no User Secrets):

   ```bash
   dotnet user-secrets set "Jwt:Key" "<chave-aleatoria-com-pelo-menos-32-caracteres>" --project src/ElaArquitetura.Api
   dotnet user-secrets set "UsuariaInicial:Senha" "<senha-da-usuaria>" --project src/ElaArquitetura.Api
   ```

   Sem a chave do JWT (ou com menos de 32 caracteres) a API não sobe e mostra o erro de configuração. Sem a senha da usuária a API sobe, mas ninguém consegue entrar.

3. Rode a API (as migrations são aplicadas e a usuária `contato.elaarquitetura@gmail.com` é criada ou atualizada na inicialização):

   ```bash
   dotnet run --project src/ElaArquitetura.Api --launch-profile http
   ```

   Para testar o app no celular (Expo Go), exponha a API na rede local com `--urls http://0.0.0.0:5164` e aponte o `EXPO_PUBLIC_API_URL` do app para o IP da máquina.

4. Testes (os de integração usam Testcontainers, então o Docker precisa estar rodando):

   ```bash
   dotnet test ElaArquitetura.slnx
   ```

## Deploy

A API é publicada como imagem Docker ([Dockerfile](Dockerfile)), escutando na porta `8080`. Na inicialização ela aplica as migrations pendentes e garante a usuária do sistema; o endpoint `GET /health` (sem autenticação) informa se ela está no ar e conectada ao banco.

Variáveis de ambiente na plataforma de hospedagem:

| Variável | Obrigatória | Descrição |
| --- | --- | --- |
| `ConnectionStrings__DefaultConnection` | sim | Conexão do PostgreSQL de produção |
| `Jwt__Key` | sim | Chave aleatória com pelo menos 32 caracteres (diferente da de desenvolvimento) |
| `UsuariaInicial__Senha` | sim | Senha de login da usuária (mínimo 8 caracteres) |
| `UsuariaInicial__Email` | não | Já vem como `contato.elaarquitetura@gmail.com` no `appsettings.json` |
| `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | atrás de proxy | `true` quando há proxy reverso/HTTPS na frente (Render, Railway, Fly, Nginx), para o rate limit do login ver o IP real |
| `Database__AplicarMigrationsNaInicializacao` | não | `false` para aplicar as migrations por fora (ex.: `dotnet ef migrations bundle`) |

A API precisa ficar atrás de **HTTPS**: o app Android em release não aceita HTTP e o token não pode trafegar aberto.

**Trocar e-mail ou senha da usuária:** altere `UsuariaInicial__Email`/`UsuariaInicial__Senha` e reinicie a API. A configuração é a fonte da verdade; quando a senha muda, todas as sessões abertas são encerradas.

Para testar a imagem de produção localmente: `JWT_KEY=... USUARIA_SENHA=... docker compose --profile api up --build` (API em `http://localhost:8080`).

## Usuária e funcionários

O sistema tem uma única usuária (a dona do escritório), que é quem faz login. Os funcionários cadastrados pelo app são só um registro para consulta: não têm senha nem acesso ao sistema. A conta da usuária não aparece na listagem de funcionários e não pode ser editada nem excluída por `/api/funcionarios`. Excluir um funcionário apenas o desativa, preservando o histórico dos projetos.
