using ElaArquitetura.Domain.Common;
using ElaArquitetura.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ElaArquitetura.Infrastructure.Persistence;

public class ElaArquiteturaDbContext : DbContext
{
    public ElaArquiteturaDbContext(DbContextOptions<ElaArquiteturaDbContext> options) : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Funcionario> Funcionarios => Set<Funcionario>();
    public DbSet<Projeto> Projetos => Set<Projeto>();
    public DbSet<Etapa> Etapas => Set<Etapa>();
    public DbSet<SubEtapa> SubEtapas => Set<SubEtapa>();
    public DbSet<ChecklistItem> ChecklistItens => Set<ChecklistItem>();
    public DbSet<ProjetoFuncionario> ProjetoFuncionarios => Set<ProjetoFuncionario>();
    public DbSet<Entrega> Entregas => Set<Entrega>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Notification Pattern é um detalhe de validação em memória (Notifiable.Notifications),
        // não faz parte do modelo persistido.
        modelBuilder.Ignore<Notification>();

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ElaArquiteturaDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
