using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.UseCases.Projetos;

/// <summary>
/// No app cada projeto tem uma funcionária responsável (ou nenhuma). Definir a responsável deixa
/// o projeto vinculado só a ela; sem funcionário, o projeto fica sem responsável.
/// </summary>
public sealed class ResponsavelDoProjeto
{
    public const string Papel = "Responsavel";

    private readonly IProjetoFuncionarioRepository _projetoFuncionarioRepository;
    private readonly IFuncionarioRepository _funcionarioRepository;

    public ResponsavelDoProjeto(IProjetoFuncionarioRepository projetoFuncionarioRepository, IFuncionarioRepository funcionarioRepository)
    {
        _projetoFuncionarioRepository = projetoFuncionarioRepository;
        _funcionarioRepository = funcionarioRepository;
    }

    /// <summary>Mensagem de erro, ou null se a funcionária pode ser responsável.</summary>
    public async Task<string?> ValidarAsync(Guid? funcionarioId, CancellationToken cancellationToken)
    {
        if (funcionarioId is null)
            return null;

        var funcionario = await _funcionarioRepository.ObterPorIdAsync(funcionarioId.Value, cancellationToken);
        return funcionario is null || !funcionario.Ativo ? "Funcionária responsável não encontrada." : null;
    }

    public async Task DefinirAsync(Guid projetoId, Guid? funcionarioId, CancellationToken cancellationToken)
    {
        var vinculos = await _projetoFuncionarioRepository.ListarPorProjetoAsync(projetoId, cancellationToken);

        foreach (var vinculo in vinculos.Where(v => v.FuncionarioId != funcionarioId))
            await _projetoFuncionarioRepository.RemoverAsync(vinculo, cancellationToken);

        if (funcionarioId is not null && vinculos.All(v => v.FuncionarioId != funcionarioId))
            await _projetoFuncionarioRepository.AdicionarAsync(
                new ProjetoFuncionario(projetoId, funcionarioId.Value, Papel), cancellationToken);
    }
}
