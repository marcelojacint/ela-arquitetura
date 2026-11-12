using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Projetos;

public sealed record AtualizarProjetoInput(Guid ProjetoId, string Titulo, string? EnderecoObra, Guid? FuncionarioResponsavelId);

/// <summary>
/// Altera as informações próprias do projeto: nome, endereço da obra e funcionária responsável.
/// O nome não pode repetir entre os projetos do mesmo cliente.
/// </summary>
public sealed class AtualizarProjetoUseCase
{
    private readonly IProjetoRepository _projetoRepository;
    private readonly IEtapaRepository _etapaRepository;
    private readonly IProjetoFuncionarioRepository _projetoFuncionarioRepository;
    private readonly IFuncionarioRepository _funcionarioRepository;
    private readonly ResponsavelDoProjeto _responsavelDoProjeto;

    public AtualizarProjetoUseCase(
        IProjetoRepository projetoRepository,
        IEtapaRepository etapaRepository,
        IProjetoFuncionarioRepository projetoFuncionarioRepository,
        IFuncionarioRepository funcionarioRepository,
        ResponsavelDoProjeto responsavelDoProjeto)
    {
        _projetoRepository = projetoRepository;
        _etapaRepository = etapaRepository;
        _projetoFuncionarioRepository = projetoFuncionarioRepository;
        _funcionarioRepository = funcionarioRepository;
        _responsavelDoProjeto = responsavelDoProjeto;
    }

    public async Task<UseCaseResult<ProjetoOutput>> ExecutarAsync(AtualizarProjetoInput input, CancellationToken cancellationToken)
    {
        var projeto = await _projetoRepository.ObterPorIdAsync(input.ProjetoId, cancellationToken);
        if (projeto is null)
            return UseCaseResult<ProjetoOutput>.Falha(new[] { "Projeto não encontrado." });

        projeto.AtualizarDados(input.Titulo, input.EnderecoObra);
        if (!projeto.IsValid)
            return UseCaseResult<ProjetoOutput>.Falha(projeto.Notifications.Select(n => n.Mensagem));

        if (await _projetoRepository.ExisteComTituloAsync(projeto.ClienteId, projeto.Titulo, projeto.Id, cancellationToken))
            return UseCaseResult<ProjetoOutput>.Falha(new[] { MensagensProjeto.TituloRepetido });

        var erroResponsavel = await _responsavelDoProjeto.ValidarAsync(input.FuncionarioResponsavelId, cancellationToken);
        if (erroResponsavel is not null)
            return UseCaseResult<ProjetoOutput>.Falha(new[] { erroResponsavel });

        await _projetoRepository.AtualizarAsync(projeto, cancellationToken);
        await _responsavelDoProjeto.DefinirAsync(projeto.Id, input.FuncionarioResponsavelId, cancellationToken);

        var etapas = await _etapaRepository.ListarTodasAsync(cancellationToken);
        var vinculos = await _projetoFuncionarioRepository.ListarPorProjetoAsync(projeto.Id, cancellationToken);
        var funcionarios = await _funcionarioRepository.ListarAsync(cancellationToken);
        var funcionariosPorProjeto = FuncionarioDoProjetoOutput.AgruparPorProjeto(vinculos, funcionarios);

        return UseCaseResult<ProjetoOutput>.Ok(ProjetoOutput.DeProjeto(projeto, etapas, funcionariosPorProjeto[projeto.Id]));
    }
}
