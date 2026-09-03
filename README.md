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
