using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Projetos;

public sealed record RemoverFuncionarioInput(Guid ProjetoId, Guid FuncionarioId);

public sealed class RemoverFuncionarioDoProjetoUseCase
{
    private readonly IProjetoFuncionarioRepository _projetoFuncionarioRepository;

    public RemoverFuncionarioDoProjetoUseCase(IProjetoFuncionarioRepository projetoFuncionarioRepository)
        => _projetoFuncionarioRepository = projetoFuncionarioRepository;

    public async Task<UseCaseResult<bool>> ExecutarAsync(RemoverFuncionarioInput input, CancellationToken cancellationToken)
    {
        var existente = await _projetoFuncionarioRepository.ObterAsync(input.ProjetoId, input.FuncionarioId, cancellationToken);
        if (existente is null)
            return UseCaseResult<bool>.Falha(new[] { "Funcionário não está atribuído a este projeto." });

        await _projetoFuncionarioRepository.RemoverAsync(existente, cancellationToken);

        return UseCaseResult<bool>.Ok(true);
    }
}
