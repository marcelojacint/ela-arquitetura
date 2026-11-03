using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Entregas;

/// <summary>Entregas já registradas do projeto (a mais recente primeiro), para o app mostrar o link do Drive salvo.</summary>
public sealed class ListarEntregasDoProjetoUseCase
{
    private readonly IProjetoRepository _projetoRepository;
    private readonly IEntregaRepository _entregaRepository;

    public ListarEntregasDoProjetoUseCase(IProjetoRepository projetoRepository, IEntregaRepository entregaRepository)
    {
        _projetoRepository = projetoRepository;
        _entregaRepository = entregaRepository;
    }

    public async Task<UseCaseResult<IReadOnlyCollection<EntregaOutput>>> ExecutarAsync(Guid projetoId, CancellationToken cancellationToken)
    {
        var projeto = await _projetoRepository.ObterPorIdAsync(projetoId, cancellationToken);
        if (projeto is null)
            return UseCaseResult<IReadOnlyCollection<EntregaOutput>>.Falha(new[] { "Projeto não encontrado." });

        var entregas = await _entregaRepository.ListarPorProjetoAsync(projetoId, cancellationToken);

        var saida = entregas
            .OrderByDescending(e => e.DataEnvio)
            .Select(e => new EntregaOutput(e.Id, e.ProjetoId, e.LinkDrive, e.DataEnvio, e.EnviadoParaWhatsapp, e.DataEnvioWhatsapp))
            .ToList();

        return UseCaseResult<IReadOnlyCollection<EntregaOutput>>.Ok(saida);
    }
}
