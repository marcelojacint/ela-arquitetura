using ElaArquitetura.Application.Common;
using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Application.UseCases.Checklist;
using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.UseCases.Projetos;

public sealed record DadosDoProjeto(Guid ClienteId, string Titulo, string? EnderecoObra, Guid? FuncionarioResponsavelId = null);

/// <summary>
/// Abre um projeto para o cliente já com o checklist de todas as etapas.
/// Usado no cadastro do cliente (todo cliente nasce com um projeto) e na criação de um novo projeto.
/// </summary>
public sealed class AberturaDeProjeto
{
    private readonly IProjetoRepository _projetoRepository;
    private readonly IEtapaRepository _etapaRepository;
    private readonly ProvisionadorChecklist _provisionadorChecklist;
    private readonly ResponsavelDoProjeto _responsavelDoProjeto;

    public AberturaDeProjeto(
        IProjetoRepository projetoRepository,
        IEtapaRepository etapaRepository,
        ProvisionadorChecklist provisionadorChecklist,
        ResponsavelDoProjeto responsavelDoProjeto)
    {
        _projetoRepository = projetoRepository;
        _etapaRepository = etapaRepository;
        _provisionadorChecklist = provisionadorChecklist;
        _responsavelDoProjeto = responsavelDoProjeto;
    }

    public async Task<UseCaseResult<Projeto>> AbrirAsync(DadosDoProjeto dados, CancellationToken cancellationToken)
    {
        var etapaInicial = await _etapaRepository.ObterPrimeiraEtapaAsync(cancellationToken);

        // A primeira etapa ("Cadastro do Cliente") é opcional e já está cumprida, porque o projeto
        // nasce de um cliente cadastrado; então ele começa direto na etapa seguinte.
        if (etapaInicial.Opcional)
            etapaInicial = await _etapaRepository.ObterProximaEtapaAsync(etapaInicial, cancellationToken) ?? etapaInicial;

        var projeto = Projeto.Criar(dados.ClienteId, dados.Titulo, etapaInicial, dados.EnderecoObra);
        if (!projeto.IsValid)
            return UseCaseResult<Projeto>.Falha(projeto.Notifications.Select(n => n.Mensagem));

        // O título é o que diferencia os projetos do mesmo cliente no app.
        if (await _projetoRepository.ExisteComTituloAsync(dados.ClienteId, projeto.Titulo, null, cancellationToken))
            return UseCaseResult<Projeto>.Falha(new[] { MensagensProjeto.TituloRepetido });

        var erroResponsavel = await _responsavelDoProjeto.ValidarAsync(dados.FuncionarioResponsavelId, cancellationToken);
        if (erroResponsavel is not null)
            return UseCaseResult<Projeto>.Falha(new[] { erroResponsavel });

        await _projetoRepository.AdicionarAsync(projeto, cancellationToken);
        await _provisionadorChecklist.ProvisionarAsync(projeto.Id, cancellationToken);
        await _responsavelDoProjeto.DefinirAsync(projeto.Id, dados.FuncionarioResponsavelId, cancellationToken);

        return UseCaseResult<Projeto>.Ok(projeto);
    }
}

internal static class MensagensProjeto
{
    public const string TituloRepetido = "Este cliente já tem um projeto com esse nome.";
}
