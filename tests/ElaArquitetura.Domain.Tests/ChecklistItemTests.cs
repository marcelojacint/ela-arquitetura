using ElaArquitetura.Domain.Entities;
using Xunit;

namespace ElaArquitetura.Domain.Tests;

public class ChecklistItemTests
{
    [Fact]
    public void Concluir_deve_registrar_funcionario_e_data()
    {
        var item = new ChecklistItem(Guid.NewGuid(), Guid.NewGuid(), "Briefing");
        var funcionarioId = Guid.NewGuid();

        item.Concluir(funcionarioId);

        Assert.True(item.Concluido);
        Assert.Equal(funcionarioId, item.ConcluidoPor);
        Assert.NotNull(item.DataConclusao);
    }

    [Fact]
    public void Reabrir_deve_limpar_conclusao()
    {
        var item = new ChecklistItem(Guid.NewGuid(), Guid.NewGuid(), "Briefing");
        item.Concluir(Guid.NewGuid());

        item.Reabrir();

        Assert.False(item.Concluido);
        Assert.Null(item.ConcluidoPor);
        Assert.Null(item.DataConclusao);
    }
}
