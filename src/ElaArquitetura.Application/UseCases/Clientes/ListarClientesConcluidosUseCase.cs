using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Application.UseCases.Projetos;

namespace ElaArquitetura.Application.UseCases.Clientes;

public sealed record ProjetoConcluidoOutput(
    Guid Id,
    string Titulo,
    string? LinkDrive,
    DateTime? DataConclusao,
    string? Responsavel);

/// <summary>Card da aba "Concluídos": o cliente com os projetos dele, para reabrir ou entregar cada um.</summary>
public sealed record ClienteConcluidoOutput(
    Guid Id,
    string Nome,
    string Telefone,
    IReadOnlyCollection<ProjetoConcluidoOutput> Projetos);

/// <summary>Clientes com todos os projetos concluídos, do que concluiu por último para o mais antigo.</summary>
public sealed class ListarClientesConcluidosUseCase
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IProjetoRepository _projetoRepository;
    private readonly IProjetoFuncionarioRepository _projetoFuncionarioRepository;
    private readonly IFuncionarioRepository _funcionarioRepository;

    public ListarClientesConcluidosUseCase(
        IClienteRepository clienteRepository,
        IProjetoRepository projetoRepository,
        IProjetoFuncionarioRepository projetoFuncionarioRepository,
        IFuncionarioRepository funcionarioRepository)
    {
        _clienteRepository = clienteRepository;
        _projetoRepository = projetoRepository;
        _projetoFuncionarioRepository = projetoFuncionarioRepository;
        _funcionarioRepository = funcionarioRepository;
    }

    public async Task<IReadOnlyCollection<ClienteConcluidoOutput>> ExecutarAsync(string? termo, CancellationToken cancellationToken)
    {
        var clientes = await _clienteRepository.BuscarAsync(termo, cancellationToken);
        var projetos = await _projetoRepository.ListarPorClientesAsync(clientes.Select(c => c.Id).ToList(), cancellationToken);
        var projetosPorCliente = projetos.ToLookup(p => p.ClienteId);

        var concluidos = clientes
            .Where(c => BuscarClientesUseCase.TodosConcluidos(projetosPorCliente[c.Id]))
            .ToList();

        var idsProjetos = concluidos.SelectMany(c => projetosPorCliente[c.Id]).Select(p => p.Id).ToList();
        var vinculos = await _projetoFuncionarioRepository.ListarPorProjetosAsync(idsProjetos, cancellationToken);
        var funcionarios = await _funcionarioRepository.ListarAsync(cancellationToken);
        var funcionariosPorProjeto = FuncionarioDoProjetoOutput.AgruparPorProjeto(vinculos, funcionarios);

        return concluidos
            .Select(c => new ClienteConcluidoOutput(
                c.Id,
                c.Nome,
                c.Telefone!.Numero,
                projetosPorCliente[c.Id]
                    .Select(p => new ProjetoConcluidoOutput(
                        p.Id,
                        p.Titulo,
                        p.LinkDrive,
                        p.DataConclusao,
                        funcionariosPorProjeto[p.Id].FirstOrDefault()?.Nome))
                    .ToList()))
            .OrderByDescending(c => c.Projetos.Max(p => p.DataConclusao))
            .ToList();
    }
}
