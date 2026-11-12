using ElaArquitetura.Domain.Entities;
using ElaArquitetura.Domain.Enums;

namespace ElaArquitetura.Application.UseCases.Projetos;

public sealed record FuncionarioDoProjetoOutput(Guid FuncionarioId, string Nome, string? PapelNoProjeto)
{
    /// <summary>Agrupa os vínculos por projeto, já com o nome de cada funcionário.</summary>
    public static ILookup<Guid, FuncionarioDoProjetoOutput> AgruparPorProjeto(
        IEnumerable<ProjetoFuncionario> vinculos,
        IEnumerable<Funcionario> funcionarios)
    {
        var nomes = funcionarios.ToDictionary(f => f.Id, f => f.Nome);

        return vinculos
            .Where(v => nomes.ContainsKey(v.FuncionarioId))
            .ToLookup(
                v => v.ProjetoId,
                v => new FuncionarioDoProjetoOutput(v.FuncionarioId, nomes[v.FuncionarioId], v.PapelNoProjeto));
    }
}

/// <summary>
/// Funcionarios só vem preenchido na listagem e na consulta por id; nas demais ações sai vazio.
/// </summary>
public sealed record ProjetoOutput(
    Guid Id,
    Guid ClienteId,
    string Titulo,
    string? EnderecoObra,
    string? LinkDrive,
    StatusProjeto Status,
    Guid EtapaAtualId,
    DateTime DataInicio,
    DateTime? DataConclusao,
    int PercentualConcluido,
    IReadOnlyCollection<FuncionarioDoProjetoOutput> Funcionarios)
{
    public static ProjetoOutput DeProjeto(
        Projeto projeto,
        IReadOnlyCollection<Etapa> etapas,
        IEnumerable<FuncionarioDoProjetoOutput>? funcionarios = null)
    {
        var percentual = CalcularPercentual(projeto, etapas);

        return new(
            projeto.Id,
            projeto.ClienteId,
            projeto.Titulo,
            projeto.EnderecoObra,
            projeto.LinkDrive,
            projeto.Status,
            projeto.EtapaAtualId,
            projeto.DataInicio,
            projeto.DataConclusao,
            percentual,
            funcionarios?.ToList() ?? new List<FuncionarioDoProjetoOutput>());
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
