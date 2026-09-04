using ElaArquitetura.Application.Interfaces.Repositories;

namespace ElaArquitetura.Application.UseCases.Clientes;

public sealed record WhatsAppLinkOutput(string Link);

public sealed class ObterWhatsAppLinkUseCase
{
    private readonly IClienteRepository _clienteRepository;

    public ObterWhatsAppLinkUseCase(IClienteRepository clienteRepository) => _clienteRepository = clienteRepository;

    public async Task<WhatsAppLinkOutput?> ExecutarAsync(Guid clienteId, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(clienteId, cancellationToken);
        if (cliente?.Telefone is null)
            return null;

        return new WhatsAppLinkOutput(cliente.Telefone.LinkWhatsApp());
    }
}
