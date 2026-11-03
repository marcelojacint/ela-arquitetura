using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Projetos;

public sealed class ObterProjetoPorIdUseCase
{
    private readonly IProjetoRepository _projetoRepository;
    private readonly IEtapaRepository _etapaRepository;
    private readonly IProjetoFuncionarioRepository _projetoFuncionarioRepository;
    private readonly IFuncionarioRepository _funcionarioRepository;

    public ObterProjetoPorIdUseCase(
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

    public async Task<ProjetoOutput?> ExecutarAsync(Guid id, CancellationToken cancellationToken)
    {
        var projeto = await _projetoRepository.ObterPorIdAsync(id, cancellationToken);
        if (projeto is null)
            return null;

        var etapas = await _etapaRepository.ListarTodasAsync(cancellationToken);

        var vinculos = await _projetoFuncionarioRepository.ListarPorProjetoAsync(projeto.Id, cancellationToken);
        var funcionarios = await _funcionarioRepository.ListarAsync(cancellationToken);
        var funcionariosPorProjeto = FuncionarioDoProjetoOutput.AgruparPorProjeto(vinculos, funcionarios);

        return ProjetoOutput.DeProjeto(projeto, etapas, funcionariosPorProjeto[projeto.Id]);
    }
}
