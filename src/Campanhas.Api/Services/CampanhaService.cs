using Campanhas.Api.Contracts;
using Campanhas.Api.Data;
using ConexaoSolidaria.Domain.Entities;
using ConexaoSolidaria.Domain.Enums;
using ConexaoSolidaria.Domain.Validation;
using Microsoft.EntityFrameworkCore;

namespace Campanhas.Api.Services;

public class CampanhaService(AppDbContext db)
{
    public async Task<Result<CampanhaResponse>> CreateAsync(CreateCampanhaRequest request, CancellationToken ct)
    {
        try
        {
            CampanhaRules.EnsureCanCreate(request.DataFim.ToUniversalTime(), request.MetaFinanceira, DateTime.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            return Result<CampanhaResponse>.Fail(400, ex.Message);
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
        return Result<CampanhaResponse>.Created(ToResponse(campanha));
    }

    public async Task<Result<CampanhaResponse>> UpdateAsync(Guid id, UpdateCampanhaRequest request, CancellationToken ct)
    {
        var campanha = await db.Campanhas.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (campanha is null)
        {
            return Result<CampanhaResponse>.Fail(404, "Campanha não encontrada.");
        }

        try
        {
            CampanhaRules.EnsureCanUpdate(request.MetaFinanceira);
            CampanhaRules.EnsureCanChangeStatus(campanha.Status, request.Status);
        }
        catch (InvalidOperationException ex)
        {
            return Result<CampanhaResponse>.Fail(400, ex.Message);
        }

        campanha.Titulo = request.Titulo.Trim();
        campanha.Descricao = request.Descricao.Trim();
        campanha.DataInicio = request.DataInicio.ToUniversalTime();
        campanha.DataFim = request.DataFim.ToUniversalTime();
        campanha.MetaFinanceira = request.MetaFinanceira;
        campanha.Status = request.Status;
        campanha.AtualizadoEm = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return Result<CampanhaResponse>.Ok(ToResponse(campanha));
    }

    public async Task<Result<CampanhaResponse>> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var campanha = await db.Campanhas.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
        return campanha is null
            ? Result<CampanhaResponse>.Fail(404, "Campanha não encontrada.")
            : Result<CampanhaResponse>.Ok(ToResponse(campanha));
    }

    public async Task<IReadOnlyList<CampanhaResponse>> ListAllAsync(CancellationToken ct)
    {
        var items = await db.Campanhas.AsNoTracking()
            .OrderByDescending(c => c.CriadoEm)
            .ToListAsync(ct);
        return items.Select(ToResponse).ToList();
    }

    public async Task<IReadOnlyList<CampanhaPublicaResponse>> ListPublicAsync(CancellationToken ct) =>
        await db.Campanhas.AsNoTracking()
            .Where(c => c.Status == CampanhaStatus.Ativa)
            .OrderByDescending(c => c.CriadoEm)
            .Select(c => new CampanhaPublicaResponse(c.Id, c.Titulo, c.MetaFinanceira, c.ValorArrecadado))
            .ToListAsync(ct);

    public Task<Result<CampanhaResponse>> CancelarAsync(Guid id, CancellationToken ct) =>
        ChangeStatusAsync(id, CampanhaStatus.Cancelada, ct);

    public Task<Result<CampanhaResponse>> ConcluirAsync(Guid id, CancellationToken ct) =>
        ChangeStatusAsync(id, CampanhaStatus.Concluida, ct);

    public async Task<int> FecharExpiradasAsync(CancellationToken ct)
    {
        var agora = DateTime.UtcNow;
        var expiradas = await db.Campanhas
            .Where(c => c.Status == CampanhaStatus.Ativa && c.DataFim < agora)
            .ToListAsync(ct);

        foreach (var campanha in expiradas)
        {
            campanha.Status = CampanhaStatus.Concluida;
            campanha.AtualizadoEm = agora;
        }

        if (expiradas.Count > 0)
        {
            await db.SaveChangesAsync(ct);
        }

        return expiradas.Count;
    }

    private async Task<Result<CampanhaResponse>> ChangeStatusAsync(Guid id, CampanhaStatus destino, CancellationToken ct)
    {
        var campanha = await db.Campanhas.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (campanha is null)
        {
            return Result<CampanhaResponse>.Fail(404, "Campanha não encontrada.");
        }

        try
        {
            CampanhaRules.EnsureCanLeaveActive(campanha.Status);
        }
        catch (InvalidOperationException ex)
        {
            return Result<CampanhaResponse>.Fail(400, ex.Message);
        }

        campanha.Status = destino;
        campanha.AtualizadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Result<CampanhaResponse>.Ok(ToResponse(campanha));
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
