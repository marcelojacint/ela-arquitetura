using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Etapas;

public sealed class ListarEtapasUseCase
{
    private readonly IEtapaRepository _etapaRepository;
    private readonly ISubEtapaRepository _subEtapaRepository;

    public ListarEtapasUseCase(IEtapaRepository etapaRepository, ISubEtapaRepository subEtapaRepository)
    {
        _etapaRepository = etapaRepository;
        _subEtapaRepository = subEtapaRepository;
    }

    public async Task<IReadOnlyCollection<EtapaOutput>> ExecutarAsync(CancellationToken cancellationToken)
    {
        var etapas = await _etapaRepository.ListarTodasAsync(cancellationToken);
        var subEtapas = await _subEtapaRepository.ListarTodasAsync(cancellationToken);

        return etapas
            .OrderBy(etapa => etapa.Ordem)
            .Select(etapa => new EtapaOutput(
                etapa.Id,
                etapa.Nome,
                etapa.Ordem,
                etapa.Opcional,
                etapa.Final,
                subEtapas
                    .Where(subEtapa => subEtapa.EtapaId == etapa.Id)
                    .OrderBy(subEtapa => subEtapa.Ordem)
                    .Select(subEtapa => new SubEtapaOutput(subEtapa.Id, subEtapa.Nome, subEtapa.Ordem))
                    .ToList()))
            .ToList();
    }
}
