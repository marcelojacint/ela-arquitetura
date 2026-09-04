using ElaArquitetura.Domain.ValueObjects;
using Xunit;

namespace ElaArquitetura.Domain.Tests;

public class TelefoneTests
{
    [Theory]
    [InlineData("11987654321", "+5511987654321")]
    [InlineData("+5511987654321", "+5511987654321")]
    [InlineData("(11) 98765-4321", "+5511987654321")]
    public void TryCriar_deve_normalizar_para_E164(string entrada, string esperado)
    {
        var criado = Telefone.TryCriar(entrada, out var telefone, out _);

        Assert.True(criado);
        Assert.Equal(esperado, telefone!.Numero);
    }

    [Fact]
    public void TryCriar_deve_falhar_para_numero_muito_curto()
    {
        var criado = Telefone.TryCriar("123", out var telefone, out var erro);

        Assert.False(criado);
        Assert.Null(telefone);
        Assert.NotNull(erro);
    }

    [Fact]
    public void LinkWhatsApp_deve_montar_url_wa_me()
    {
        Telefone.TryCriar("11987654321", out var telefone, out _);

        Assert.Equal("https://wa.me/5511987654321", telefone!.LinkWhatsApp());
    }
}
