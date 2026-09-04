using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.UseCases.Entregas;

public sealed record RegistrarEntregaInput(Guid ProjetoId, string LinkDrive, bool NotificarWhatsApp);

public sealed record EntregaOutput(
    Guid Id,
    Guid ProjetoId,
    string LinkDrive,
    DateTime DataEnvio,
    bool EnviadoParaWhatsapp,
    DateTime? DataEnvioWhatsapp);

public sealed class RegistrarEntregaUseCase
{
    private readonly IProjetoRepository _projetoRepository;
    private readonly IEntregaRepository _entregaRepository;

    public RegistrarEntregaUseCase(IProjetoRepository projetoRepository, IEntregaRepository entregaRepository)
    {
        _projetoRepository = projetoRepository;
        _entregaRepository = entregaRepository;
    }

    public async Task<UseCaseResult<EntregaOutput>> ExecutarAsync(RegistrarEntregaInput input, CancellationToken cancellationToken)
    {
        var projeto = await _projetoRepository.ObterPorIdAsync(input.ProjetoId, cancellationToken);
        if (projeto is null)
            return UseCaseResult<EntregaOutput>.Falha(new[] { "Projeto não encontrado." });

        if (string.IsNullOrWhiteSpace(input.LinkDrive))
            return UseCaseResult<EntregaOutput>.Falha(new[] { "Link do Drive é obrigatório." });

        var entrega = new Entrega(projeto.Id, input.LinkDrive);
        if (input.NotificarWhatsApp)
            entrega.RegistrarEnvioWhatsapp();

        await _entregaRepository.AdicionarAsync(entrega, cancellationToken);

        return UseCaseResult<EntregaOutput>.Ok(new EntregaOutput(
            entrega.Id, entrega.ProjetoId, entrega.LinkDrive, entrega.DataEnvio, entrega.EnviadoParaWhatsapp, entrega.DataEnvioWhatsapp));
    }
}
