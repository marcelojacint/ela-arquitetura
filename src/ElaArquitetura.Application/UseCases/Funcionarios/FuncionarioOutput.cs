using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.UseCases.Funcionarios;

public sealed record FuncionarioOutput(Guid Id, string Nome, string Email, string? Telefone, DateOnly? DataNascimento, bool Ativo)
{
    public static FuncionarioOutput DeFuncionario(Funcionario funcionario) =>
        new(funcionario.Id, funcionario.Nome, funcionario.Email, funcionario.Telefone?.Numero, funcionario.DataNascimento, funcionario.Ativo);
}
