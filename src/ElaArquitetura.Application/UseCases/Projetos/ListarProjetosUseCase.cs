using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Domain.Enums;

namespace ElaArquitetura.Application.UseCases.Projetos;

public sealed record ListarProjetosInput(StatusProjeto? Status, Guid? EtapaId, Guid? ClienteId = null);

public sealed class ListarProjetosUseCase
{
    private readonly IProjetoRepository _projetoRepository;
    private readonly IEtapaRepository _etapaRepository;
    private readonly IProjetoFuncionarioRepository _projetoFuncionarioRepository;
    private readonly IFuncionarioRepository _funcionarioRepository;

    public ListarProjetosUseCase(
        IProjetoRepository projetoRepository,
        IEtapaRepository etapaRepository,
        IProjetoFuncionarioRepository projetoFuncionarioRepository,
        IFuncionarioRepository funcionarioRepository)
    {
        _projetoRepository = projetoRepository;
        _etapaRepository = etapaRepository;
        _projetoFuncionarioRepository = projetoFuncionarioRepository;
        _funcionarioRepository = funcionarioRepository;
    }

    public async Task<IReadOnlyCollection<ProjetoOutput>> ExecutarAsync(ListarProjetosInput input, CancellationToken cancellationToken)
    {
        var projetos = await _projetoRepository.ListarAsync(input.Status, input.EtapaId, input.ClienteId, cancellationToken);
        var etapas = await _etapaRepository.ListarTodasAsync(cancellationToken);

        var vinculos = await _projetoFuncionarioRepository.ListarPorProjetosAsync(projetos.Select(p => p.Id).ToList(), cancellationToken);
        var funcionarios = await _funcionarioRepository.ListarAsync(cancellationToken);
        var funcionariosPorProjeto = FuncionarioDoProjetoOutput.AgruparPorProjeto(vinculos, funcionarios);

        return projetos
            .Select(projeto => ProjetoOutput.DeProjeto(projeto, etapas, funcionariosPorProjeto[projeto.Id]))
            .ToList();
    }
}
