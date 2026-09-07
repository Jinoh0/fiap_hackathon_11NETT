namespace ConexaoSolidaria.Domain.Events;

public sealed record DoacaoRecebidaEvent(
    Guid DoacaoId,
    Guid CampanhaId,
    Guid DoadorId,
    decimal ValorDoacao,
    DateTime OcorridoEmUtc);
