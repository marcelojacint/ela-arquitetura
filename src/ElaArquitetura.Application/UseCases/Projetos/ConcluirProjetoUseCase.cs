using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Projetos;

public sealed record ConcluirProjetoInput(Guid ProjetoId);

/// <summary>
/// RF08/RF17 — só conclui o projeto se a etapa atual for a etapa final do fluxo
/// e existir ao menos uma Entrega registrada.
/// </summary>
public sealed class ConcluirProjetoUseCase
{
    private readonly IProjetoRepository _projetoRepository;
    private readonly IEtapaRepository _etapaRepository;
    private readonly IEntregaRepository _entregaRepository;

    public ConcluirProjetoUseCase(
        IProjetoRepository projetoRepository,
        IEtapaRepository etapaRepository,
        IEntregaRepository entregaRepository)
    {
        _projetoRepository = projetoRepository;
        _etapaRepository = etapaRepository;
        _entregaRepository = entregaRepository;
    }

    public async Task<UseCaseResult<ProjetoOutput>> ExecutarAsync(ConcluirProjetoInput input, CancellationToken cancellationToken)
    {
        var projeto = await _projetoRepository.ObterPorIdAsync(input.ProjetoId, cancellationToken);
        if (projeto is null)
            return UseCaseResult<ProjetoOutput>.Falha(new[] { "Projeto não encontrado." });

        var etapaAtual = await _etapaRepository.ObterPorIdAsync(projeto.EtapaAtualId, cancellationToken);
        if (etapaAtual is null)
            return UseCaseResult<ProjetoOutput>.Falha(new[] { "Etapa atual do projeto não encontrada." });

        var entregas = await _entregaRepository.ListarPorProjetoAsync(projeto.Id, cancellationToken);

        projeto.Concluir(etapaAtual, entregas);
        if (!projeto.IsValid)
            return UseCaseResult<ProjetoOutput>.Falha(projeto.Notifications.Select(n => n.Mensagem));

        await _projetoRepository.AtualizarAsync(projeto, cancellationToken);

        return UseCaseResult<ProjetoOutput>.Ok(ProjetoOutput.DeProjeto(projeto));
    }
}
