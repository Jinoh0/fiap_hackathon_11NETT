using System.Security.Claims;
using Campanhas.Api.Contracts;
using Campanhas.Api.Services;
using ConexaoSolidaria.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Campanhas.Api.Controllers;

[ApiController]
[Route("api/doacoes")]
[Authorize(Roles = UserRoles.Doador)]
public class DoacoesController(DoacaoService doacoes) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDoacaoRequest request, CancellationToken ct)
    {
        var doadorIdValue = User.FindFirstValue("uid");
        if (!Guid.TryParse(doadorIdValue, out var doadorId))
        {
            return Unauthorized();
        }

        var result = await doacoes.CreateAsync(request, doadorId, ct);
        return result.IsSuccess
            ? StatusCode(result.StatusCode, result.Value)
            : StatusCode(result.StatusCode, new { message = result.Error });
    }
}
