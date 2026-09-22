using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.UseCases.Funcionarios;

public sealed record FuncionarioOutput(Guid Id, string Nome, string Email, string Cargo, string? Telefone, bool Ativo)
{
    public static FuncionarioOutput DeFuncionario(Funcionario funcionario) =>
        new(funcionario.Id, funcionario.Nome, funcionario.Email, funcionario.Cargo, funcionario.Telefone?.Numero, funcionario.Ativo);
}
