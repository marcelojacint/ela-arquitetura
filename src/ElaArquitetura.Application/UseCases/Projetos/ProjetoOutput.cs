using ElaArquitetura.Domain.Enums;

namespace ElaArquitetura.Application.UseCases.Projetos;

public sealed record ProjetoOutput(Guid Id, Guid ClienteId, string Titulo, StatusProjeto Status, Guid EtapaAtualId);
