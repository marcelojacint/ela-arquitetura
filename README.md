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

2. Configure a chave do JWT (uma vez por máquina; fica fora do repositório, no User Secrets):

   ```bash
   dotnet user-secrets set "Jwt:Key" "<chave-aleatoria-com-pelo-menos-32-caracteres>" --project src/ElaArquitetura.Api
   ```

   Sem essa chave a API sobe, mas o login falha.

3. Rode a API (em Development as migrations são aplicadas automaticamente e é criada a usuária padrão `admin@elaarquitetura.com.br` / `Trocar123!`):

   ```bash
   dotnet run --project src/ElaArquitetura.Api --launch-profile http
   ```

   Para testar o app no celular (Expo Go), exponha a API na rede local com `--urls http://0.0.0.0:5164` e aponte o `EXPO_PUBLIC_API_URL` do app para o IP da máquina.

4. Testes (os de integração usam Testcontainers, então o Docker precisa estar rodando):

   ```bash
   dotnet test ElaArquitetura.slnx
   ```

## Usuária e funcionários

O sistema tem uma única usuária (a dona do escritório), que é quem faz login. Os funcionários cadastrados pelo app são só um registro para consulta: não têm senha nem acesso ao sistema. A conta da usuária não aparece na listagem de funcionários e não pode ser editada nem excluída por `/api/funcionarios`. Excluir um funcionário apenas o desativa, preservando o histórico dos projetos.
