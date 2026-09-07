using Campanhas.Api.Contracts;
using Campanhas.Api.Data;
using ConexaoSolidaria.Domain.Entities;
using ConexaoSolidaria.Domain.Enums;
using ConexaoSolidaria.Domain.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Campanhas.Api.Controllers;

[ApiController]
[Route("api/campanhas")]
public class CampanhasController(AppDbContext db) : ControllerBase
{
    [Authorize(Roles = UserRoles.GestorOng)]
    [HttpPost]
    public async Task<ActionResult<CampanhaResponse>> Create([FromBody] CreateCampanhaRequest request, CancellationToken ct)
    {
        try
        {
            CampanhaRules.EnsureCanCreate(request.DataFim.ToUniversalTime(), request.MetaFinanceira, DateTime.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        var campanha = new Campanha
        {
            Titulo = request.Titulo.Trim(),
            Descricao = request.Descricao.Trim(),
            DataInicio = request.DataInicio.ToUniversalTime(),
            DataFim = request.DataFim.ToUniversalTime(),
            MetaFinanceira = request.MetaFinanceira,
            Status = request.Status ?? CampanhaStatus.Ativa
        };

        db.Campanhas.Add(campanha);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = campanha.Id }, ToResponse(campanha));
    }

    [Authorize(Roles = UserRoles.GestorOng)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CampanhaResponse>> Update(Guid id, [FromBody] UpdateCampanhaRequest request, CancellationToken ct)
    {
        var campanha = await db.Campanhas.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (campanha is null)
        {
            return NotFound();
        }

        if (request.MetaFinanceira <= 0)
        {
            return BadRequest(new { message = "A meta financeira deve ser maior que zero." });
        }

        campanha.Titulo = request.Titulo.Trim();
        campanha.Descricao = request.Descricao.Trim();
        campanha.DataInicio = request.DataInicio.ToUniversalTime();
        campanha.DataFim = request.DataFim.ToUniversalTime();
        campanha.MetaFinanceira = request.MetaFinanceira;
        campanha.Status = request.Status;
        campanha.AtualizadoEm = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return Ok(ToResponse(campanha));
    }

    [Authorize(Roles = UserRoles.GestorOng)]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CampanhaResponse>> GetById(Guid id, CancellationToken ct)
    {
        var campanha = await db.Campanhas.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
        return campanha is null ? NotFound() : Ok(ToResponse(campanha));
    }

    [Authorize(Roles = UserRoles.GestorOng)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CampanhaResponse>>> ListAll(CancellationToken ct)
    {
        var items = await db.Campanhas.AsNoTracking()
            .OrderByDescending(c => c.CriadoEm)
            .ToListAsync(ct);
        return Ok(items.Select(ToResponse));
    }

    [AllowAnonymous]
    [HttpGet("publicas")]
    public async Task<ActionResult<IEnumerable<CampanhaPublicaResponse>>> ListPublic(CancellationToken ct)
    {
        var items = await db.Campanhas.AsNoTracking()
            .Where(c => c.Status == CampanhaStatus.Ativa)
            .OrderByDescending(c => c.CriadoEm)
            .Select(c => new CampanhaPublicaResponse(c.Id, c.Titulo, c.MetaFinanceira, c.ValorArrecadado))
            .ToListAsync(ct);
        return Ok(items);
    }

    private static CampanhaResponse ToResponse(Campanha c) => new(
        c.Id,
        c.Titulo,
        c.Descricao,
        c.DataInicio,
        c.DataFim,
        c.MetaFinanceira,
        c.ValorArrecadado,
        c.Status.ToString());
}
