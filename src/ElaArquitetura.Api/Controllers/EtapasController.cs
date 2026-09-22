using ElaArquitetura.Application.UseCases.Etapas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElaArquitetura.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/etapas")]
public class EtapasController : ControllerBase
{
    private readonly ListarEtapasUseCase _listarEtapasUseCase;

    public EtapasController(ListarEtapasUseCase listarEtapasUseCase) => _listarEtapasUseCase = listarEtapasUseCase;

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var etapas = await _listarEtapasUseCase.ExecutarAsync(cancellationToken);
        return Ok(etapas);
    }
}
