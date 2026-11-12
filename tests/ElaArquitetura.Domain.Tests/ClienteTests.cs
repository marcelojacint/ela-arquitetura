using ElaArquitetura.Domain.Entities;
using Xunit;

namespace ElaArquitetura.Domain.Tests;

public class ClienteTests
{
    [Fact]
    public void Criar_deve_ser_valido_com_dados_corretos()
    {
        var cliente = Cliente.Criar("Maria Silva", "11987654321", "maria@email.com", "Rua A, 123");

        Assert.True(cliente.IsValid);
        Assert.Equal("+5511987654321", cliente.Telefone!.Numero);
    }

    [Fact]
    public void Criar_deve_gerar_notification_quando_telefone_e_invalido()
    {
        var cliente = Cliente.Criar("Maria Silva", "123");

        Assert.False(cliente.IsValid);
        Assert.Contains(cliente.Notifications, n => n.Chave == nameof(Cliente.Telefone));
    }

    [Fact]
    public void Criar_deve_gerar_notification_quando_nome_esta_vazio()
    {
        var cliente = Cliente.Criar("", "11987654321");

        Assert.False(cliente.IsValid);
        Assert.Contains(cliente.Notifications, n => n.Chave == nameof(Cliente.Nome));
    }
}
