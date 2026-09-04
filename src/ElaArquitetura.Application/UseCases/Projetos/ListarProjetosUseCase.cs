using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Domain.Enums;

namespace ElaArquitetura.Application.UseCases.Projetos;

public sealed record ListarProjetosInput(StatusProjeto? Status, Guid? EtapaId);

/// <summary>RF13 — listagem filtrável por status e/ou etapa.</summary>
public sealed class ListarProjetosUseCase
{
    private readonly IProjetoRepository _projetoRepository;

    public ListarProjetosUseCase(IProjetoRepository projetoRepository) => _projetoRepository = projetoRepository;

    public async Task<IReadOnlyCollection<ProjetoOutput>> ExecutarAsync(ListarProjetosInput input, CancellationToken cancellationToken)
    {
        var projetos = await _projetoRepository.ListarAsync(input.Status, input.EtapaId, cancellationToken);
        return projetos.Select(ProjetoOutput.DeProjeto).ToList();
    }
}
