using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Projetos;

public sealed class ObterProjetoPorIdUseCase
{
    private readonly IProjetoRepository _projetoRepository;

    public ObterProjetoPorIdUseCase(IProjetoRepository projetoRepository) => _projetoRepository = projetoRepository;

    public async Task<ProjetoOutput?> ExecutarAsync(Guid id, CancellationToken cancellationToken)
    {
        var projeto = await _projetoRepository.ObterPorIdAsync(id, cancellationToken);
        return projeto is null ? null : ProjetoOutput.DeProjeto(projeto);
    }
}
