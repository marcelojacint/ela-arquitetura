namespace ElaArquitetura.Application.UseCases.Etapas;

public sealed record SubEtapaOutput(Guid Id, string Nome, int Ordem);

public sealed record EtapaOutput(
    Guid Id,
    string Nome,
    int Ordem,
    bool Opcional,
    bool Final,
    IReadOnlyCollection<SubEtapaOutput> SubEtapas);
