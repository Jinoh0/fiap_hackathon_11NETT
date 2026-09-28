using Campanhas.Api.Contracts;
using Campanhas.Api.Data;
using Campanhas.Api.Messaging;
using ConexaoSolidaria.Domain.Entities;
using ConexaoSolidaria.Domain.Enums;
using ConexaoSolidaria.Domain.Events;
using ConexaoSolidaria.Domain.Validation;
using Microsoft.EntityFrameworkCore;

namespace Campanhas.Api.Services;

public class DoacaoService(AppDbContext db, IDoacaoEventPublisher publisher)
{
    public async Task<Result<DoacaoResponse>> CreateAsync(CreateDoacaoRequest request, Guid doadorId, CancellationToken ct)
    {
        if (request.ValorDoacao <= 0)
        {
            return Result<DoacaoResponse>.Fail(400, "O valor da doação deve ser maior que zero.");
        }

        var campanha = await db.Campanhas.FirstOrDefaultAsync(c => c.Id == request.IdCampanha, ct);
        if (campanha is null)
        {
            return Result<DoacaoResponse>.Fail(404, "Campanha não encontrada.");
        }

        try
        {
            CampanhaRules.EnsureCanReceiveDonation(campanha.Status);
        }
        catch (InvalidOperationException ex)
        {
            return Result<DoacaoResponse>.Fail(400, ex.Message);
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

        return Result<DoacaoResponse>.Accepted(new DoacaoResponse(
            doacao.Id,
            doacao.CampanhaId,
            doacao.ValorDoacao,
            doacao.Status.ToString(),
            doacao.CriadoEm));
    }
}
