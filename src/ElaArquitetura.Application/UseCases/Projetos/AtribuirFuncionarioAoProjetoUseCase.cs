using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.UseCases.Projetos;

public sealed record AtribuirFuncionarioInput(Guid ProjetoId, Guid FuncionarioId, string? PapelNoProjeto);

public sealed class AtribuirFuncionarioAoProjetoUseCase
{
    private readonly IProjetoRepository _projetoRepository;
    private readonly IFuncionarioRepository _funcionarioRepository;
    private readonly IProjetoFuncionarioRepository _projetoFuncionarioRepository;

    public AtribuirFuncionarioAoProjetoUseCase(
        IProjetoRepository projetoRepository,
        IFuncionarioRepository funcionarioRepository,
        IProjetoFuncionarioRepository projetoFuncionarioRepository)
    {
        _projetoRepository = projetoRepository;
        _funcionarioRepository = funcionarioRepository;
        _projetoFuncionarioRepository = projetoFuncionarioRepository;
    }

    public async Task<UseCaseResult<bool>> ExecutarAsync(AtribuirFuncionarioInput input, CancellationToken cancellationToken)
    {
        var projeto = await _projetoRepository.ObterPorIdAsync(input.ProjetoId, cancellationToken);
        if (projeto is null)
            return UseCaseResult<bool>.Falha(new[] { "Projeto não encontrado." });

        var funcionario = await _funcionarioRepository.ObterPorIdAsync(input.FuncionarioId, cancellationToken);
        if (funcionario is null)
            return UseCaseResult<bool>.Falha(new[] { "Funcionário não encontrado." });

        var existente = await _projetoFuncionarioRepository.ObterAsync(input.ProjetoId, input.FuncionarioId, cancellationToken);
        if (existente is not null)
            return UseCaseResult<bool>.Falha(new[] { "Funcionário já está atribuído a este projeto." });

        await _projetoFuncionarioRepository.AdicionarAsync(
            new ProjetoFuncionario(input.ProjetoId, input.FuncionarioId, input.PapelNoProjeto), cancellationToken);

        return UseCaseResult<bool>.Ok(true);
    }
}
