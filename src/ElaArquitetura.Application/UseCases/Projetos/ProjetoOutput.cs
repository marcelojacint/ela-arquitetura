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
    DateTime? DataConclusao,
    int PercentualConcluido)
{
    public static ProjetoOutput DeProjeto(Projeto projeto, IReadOnlyCollection<Etapa> etapas)
    {
        var percentual = CalcularPercentual(projeto, etapas);

        return new(
            projeto.Id,
            projeto.ClienteId,
            projeto.Titulo,
            projeto.Status,
            projeto.EtapaAtualId,
            projeto.DataInicio,
            projeto.DataConclusao,
            percentual);
    }

    private static int CalcularPercentual(Projeto projeto, IReadOnlyCollection<Etapa> etapas)
    {
        if (projeto.Status == StatusProjeto.Concluido)
            return 100;

        if (etapas.Count <= 1)
            return 0;

        var etapaAtual = etapas.FirstOrDefault(e => e.Id == projeto.EtapaAtualId);
        if (etapaAtual is null)
            return 0;

        var percentual = (etapaAtual.Ordem - 1) * 100 / (etapas.Count - 1);
        return Math.Clamp(percentual, 0, 100);
    }
}
