using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Projetos;

/// <summary>LinkDrive nulo ou vazio remove o link do projeto.</summary>
public sealed record DefinirLinkDriveInput(Guid ProjetoId, string? LinkDrive);

/// <summary>
/// Link da pasta do Drive do projeto (item "Link do Drive" do card). Pode ser trocado ou removido a qualquer momento;
/// é diferente da Entrega, que registra o envio ao cliente.
/// </summary>
public sealed class DefinirLinkDriveUseCase
{
    private readonly IProjetoRepository _projetoRepository;
    private readonly IEtapaRepository _etapaRepository;

    public DefinirLinkDriveUseCase(IProjetoRepository projetoRepository, IEtapaRepository etapaRepository)
    {
        _projetoRepository = projetoRepository;
        _etapaRepository = etapaRepository;
    }

    public async Task<UseCaseResult<ProjetoOutput>> ExecutarAsync(DefinirLinkDriveInput input, CancellationToken cancellationToken)
    {
        var projeto = await _projetoRepository.ObterPorIdAsync(input.ProjetoId, cancellationToken);
        if (projeto is null)
            return UseCaseResult<ProjetoOutput>.Falha(new[] { "Projeto não encontrado." });

        projeto.DefinirLinkDrive(input.LinkDrive);
        if (!projeto.IsValid)
            return UseCaseResult<ProjetoOutput>.Falha(projeto.Notifications.Select(n => n.Mensagem));

        await _projetoRepository.AtualizarAsync(projeto, cancellationToken);

        var etapas = await _etapaRepository.ListarTodasAsync(cancellationToken);
        return UseCaseResult<ProjetoOutput>.Ok(ProjetoOutput.DeProjeto(projeto, etapas));
    }
}
