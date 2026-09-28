using Campanhas.Api.Contracts;
using Campanhas.Api.Services;
using ConexaoSolidaria.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Campanhas.Api.Controllers;

[ApiController]
[Route("api/campanhas")]
public class CampanhasController(CampanhaService campanhas) : ControllerBase
{
    [Authorize(Roles = UserRoles.GestorOng)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCampanhaRequest request, CancellationToken ct)
    {
        var result = await campanhas.CreateAsync(request, ct);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new { message = result.Error });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [Authorize(Roles = UserRoles.GestorOng)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCampanhaRequest request, CancellationToken ct) =>
        ToActionResult(await campanhas.UpdateAsync(id, request, ct));

    [Authorize(Roles = UserRoles.GestorOng)]
    [HttpPost("{id:guid}/cancelar")]
    public async Task<IActionResult> Cancelar(Guid id, CancellationToken ct) =>
        ToActionResult(await campanhas.CancelarAsync(id, ct));

    [Authorize(Roles = UserRoles.GestorOng)]
    [HttpPost("{id:guid}/concluir")]
    public async Task<IActionResult> Concluir(Guid id, CancellationToken ct) =>
        ToActionResult(await campanhas.ConcluirAsync(id, ct));

    [Authorize(Roles = UserRoles.GestorOng)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct) =>
        ToActionResult(await campanhas.GetByIdAsync(id, ct));

    [Authorize(Roles = UserRoles.GestorOng)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CampanhaResponse>>> ListAll(CancellationToken ct) =>
        Ok(await campanhas.ListAllAsync(ct));

    [AllowAnonymous]
    [HttpGet("publicas")]
    public async Task<ActionResult<IEnumerable<CampanhaPublicaResponse>>> ListPublic(CancellationToken ct) =>
        Ok(await campanhas.ListPublicAsync(ct));

    private IActionResult ToActionResult(Result<CampanhaResponse> result) =>
        result.IsSuccess
            ? StatusCode(result.StatusCode, result.Value)
            : StatusCode(result.StatusCode, new { message = result.Error });
}
