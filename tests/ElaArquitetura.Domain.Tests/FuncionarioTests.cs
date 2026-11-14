using ElaArquitetura.Domain.Entities;
using Xunit;

namespace ElaArquitetura.Domain.Tests;

public class FuncionarioTests
{
    [Fact]
    public void Cadastrar_deve_ser_valido_e_sem_acesso()
    {
        var funcionario = Funcionario.Cadastrar("Jamille Borel", "jamille@email.com", "83999999999", new DateOnly(2003, 8, 10));

        Assert.True(funcionario.IsValid);
        Assert.False(funcionario.PossuiAcesso);
        Assert.Null(funcionario.Cargo);
        Assert.Equal("+5583999999999", funcionario.Telefone!.Numero);
        Assert.Equal(new DateOnly(2003, 8, 10), funcionario.DataNascimento);
    }

    [Theory]
    [InlineData("  Jamille@Email.COM ")]
    [InlineData("JAMILLE@EMAIL.COM")]
    public void Email_deve_ser_gravado_sem_espacos_e_em_minusculas(string email)
    {
        Assert.Equal("jamille@email.com", Funcionario.Cadastrar("Jamille", email, null, null).Email);
        Assert.Equal("jamille@email.com", Funcionario.Criar("Jamille", email, "Proprietaria", "hash").Email);

        var funcionario = Funcionario.Cadastrar("Jamille", "outro@email.com", null, null);
        funcionario.Atualizar("Jamille", email, null, null);
        Assert.Equal("jamille@email.com", funcionario.Email);
    }

    [Fact]
    public void Cadastrar_deve_aceitar_telefone_e_nascimento_vazios()
    {
        var funcionario = Funcionario.Cadastrar("Jamille Borel", "jamille@email.com", null, null);

        Assert.True(funcionario.IsValid);
        Assert.Null(funcionario.Telefone);
        Assert.Null(funcionario.DataNascimento);
    }

    [Theory]
    [InlineData("", "jamille@email.com", nameof(Funcionario.Nome))]
    [InlineData("Jamille", " ", nameof(Funcionario.Email))]
    public void Cadastrar_deve_exigir_nome_e_email(string nome, string email, string chave)
    {
        var funcionario = Funcionario.Cadastrar(nome, email, null, null);

        Assert.False(funcionario.IsValid);
        Assert.Contains(funcionario.Notifications, n => n.Chave == chave);
    }

    [Fact]
    public void Cadastrar_deve_rejeitar_nascimento_no_futuro()
    {
        var amanha = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        var funcionario = Funcionario.Cadastrar("Jamille", "jamille@email.com", null, amanha);

        Assert.False(funcionario.IsValid);
        Assert.Contains(funcionario.Notifications, n => n.Chave == nameof(Funcionario.DataNascimento));
    }

    [Fact]
    public void Atualizar_deve_trocar_dados_e_limpar_opcionais()
    {
        var funcionario = Funcionario.Cadastrar("Jamille", "jamille@email.com", "83999999999", new DateOnly(2003, 8, 10));

        funcionario.Atualizar("Jamille Borel Linhares", "jbl@email.com", null, null);

        Assert.True(funcionario.IsValid);
        Assert.Equal("Jamille Borel Linhares", funcionario.Nome);
        Assert.Equal("jbl@email.com", funcionario.Email);
        Assert.Null(funcionario.Telefone);
        Assert.Null(funcionario.DataNascimento);
    }

    [Fact]
    public void Criar_usuaria_do_sistema_deve_ter_acesso()
    {
        var funcionario = Funcionario.Criar("Administradora", "admin@email.com", "Proprietaria", "hash");

        Assert.True(funcionario.IsValid);
        Assert.True(funcionario.PossuiAcesso);
    }
}
