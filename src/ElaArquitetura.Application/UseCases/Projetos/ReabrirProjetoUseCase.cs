using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace ElaArquitetura.Application.UseCases.Projetos;

public sealed record ReabrirProjetoInput(Guid ProjetoId);

public sealed class ReabrirProjetoUseCase
{
    private readonly IProjetoRepository _projetoRepository;
    private readonly ILogger<ReabrirProjetoUseCase> _logger;

    public ReabrirProjetoUseCase(IProjetoRepository projetoRepository, ILogger<ReabrirProjetoUseCase> logger)
    {
        _projetoRepository = projetoRepository;
        _logger = logger;
    }

    public async Task<UseCaseResult<ProjetoOutput>> ExecutarAsync(ReabrirProjetoInput input, CancellationToken cancellationToken)
    {
        var projeto = await _projetoRepository.ObterPorIdAsync(input.ProjetoId, cancellationToken);
        if (projeto is null)
            return UseCaseResult<ProjetoOutput>.Falha(new[] { "Projeto não encontrado." });

        projeto.Reabrir();
        await _projetoRepository.AtualizarAsync(projeto, cancellationToken);

        _logger.LogInformation("Projeto {ProjetoId} voltou para o status {NovoStatus}", projeto.Id, projeto.Status);

        return UseCaseResult<ProjetoOutput>.Ok(ProjetoOutput.DeProjeto(projeto));
    }
}
