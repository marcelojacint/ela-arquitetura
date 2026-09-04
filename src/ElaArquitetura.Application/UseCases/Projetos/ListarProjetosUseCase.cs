using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Domain.Enums;

namespace ElaArquitetura.Application.UseCases.Projetos;

public sealed record ListarProjetosInput(StatusProjeto? Status, Guid? EtapaId);

public sealed class ListarProjetosUseCase
{
    private readonly IProjetoRepository _projetoRepository;
    private readonly IEtapaRepository _etapaRepository;

    public ListarProjetosUseCase(IProjetoRepository projetoRepository, IEtapaRepository etapaRepository)
    {
        _projetoRepository = projetoRepository;
        _etapaRepository = etapaRepository;
    }

    public async Task<IReadOnlyCollection<ProjetoOutput>> ExecutarAsync(ListarProjetosInput input, CancellationToken cancellationToken)
    {
        var projetos = await _projetoRepository.ListarAsync(input.Status, input.EtapaId, cancellationToken);
        var etapas = await _etapaRepository.ListarTodasAsync(cancellationToken);

        return projetos.Select(projeto => ProjetoOutput.DeProjeto(projeto, etapas)).ToList();
    }
}
