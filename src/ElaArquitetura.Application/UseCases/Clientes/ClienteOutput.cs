using ElaArquitetura.Domain.Entities;

namespace ElaArquitetura.Application.UseCases.Clientes;

public sealed record ClienteOutput(Guid Id, string Nome, string Telefone, string? Email, string? Endereco, DateTime DataCadastro, bool Ativo)
{
    public static ClienteOutput DeCliente(Cliente cliente) =>
        new(cliente.Id, cliente.Nome, cliente.Telefone!.Numero, cliente.Email, cliente.Endereco, cliente.DataCadastro, cliente.Ativo);
}
