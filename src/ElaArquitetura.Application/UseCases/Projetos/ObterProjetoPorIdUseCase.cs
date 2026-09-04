using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Projetos;

public sealed class ObterProjetoPorIdUseCase
{
    private readonly IProjetoRepository _projetoRepository;
    private readonly IEtapaRepository _etapaRepository;

    public ObterProjetoPorIdUseCase(IProjetoRepository projetoRepository, IEtapaRepository etapaRepository)
    {
        _projetoRepository = projetoRepository;
        _etapaRepository = etapaRepository;
    }

    public async Task<ProjetoOutput?> ExecutarAsync(Guid id, CancellationToken cancellationToken)
    {
        var projeto = await _projetoRepository.ObterPorIdAsync(id, cancellationToken);
        if (projeto is null)
            return null;

        var etapas = await _etapaRepository.ListarTodasAsync(cancellationToken);

        return ProjetoOutput.DeProjeto(projeto, etapas);
    }
}
