using System.Security.Claims;
using Campanhas.Api.Contracts;
using Campanhas.Api.Data;
using Campanhas.Api.Messaging;
using ConexaoSolidaria.Domain.Entities;
using ConexaoSolidaria.Domain.Enums;
using ConexaoSolidaria.Domain.Events;
using ConexaoSolidaria.Domain.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Campanhas.Api.Controllers;

[ApiController]
[Route("api/doacoes")]
[Authorize(Roles = UserRoles.Doador)]
public class DoacoesController(AppDbContext db, IDoacaoEventPublisher publisher) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<DoacaoResponse>> Create([FromBody] CreateDoacaoRequest request, CancellationToken ct)
    {
        if (request.ValorDoacao <= 0)
        {
            return BadRequest(new { message = "O valor da doação deve ser maior que zero." });
        }

        var doadorIdValue = User.FindFirstValue("uid");
        if (!Guid.TryParse(doadorIdValue, out var doadorId))
        {
            return Unauthorized();
        }

        var campanha = await db.Campanhas.FirstOrDefaultAsync(c => c.Id == request.IdCampanha, ct);
        if (campanha is null)
        {
            return NotFound(new { message = "Campanha não encontrada." });
        }

        try
        {
            CampanhaRules.EnsureCanReceiveDonation(campanha.Status);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        var doacao = new Doacao
        {
            CampanhaId = campanha.Id,
            DoadorId = doadorId,
            ValorDoacao = request.ValorDoacao,
            Status = DoacaoStatus.Pendente
        };

        db.Doacoes.Add(doacao);
        await db.SaveChangesAsync(ct);

        publisher.Publish(new DoacaoRecebidaEvent(
            doacao.Id,
            doacao.CampanhaId,
            doacao.DoadorId,
            doacao.ValorDoacao,
            DateTime.UtcNow));

        return Accepted(new DoacaoResponse(
            doacao.Id,
            doacao.CampanhaId,
            doacao.ValorDoacao,
            doacao.Status.ToString(),
            doacao.CriadoEm));
    }
}
