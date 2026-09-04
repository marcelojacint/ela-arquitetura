using ElaArquitetura.Domain.Entities;
using ElaArquitetura.Domain.Enums;

namespace ElaArquitetura.Application.UseCases.Projetos;

public sealed record ProjetoOutput(
    Guid Id,
    Guid ClienteId,
    string Titulo,
    StatusProjeto Status,
    Guid EtapaAtualId,
    DateTime DataInicio,
    DateTime? DataConclusao)
{
    public static ProjetoOutput DeProjeto(Projeto projeto) =>
        new(projeto.Id, projeto.ClienteId, projeto.Titulo, projeto.Status, projeto.EtapaAtualId, projeto.DataInicio, projeto.DataConclusao);
}
