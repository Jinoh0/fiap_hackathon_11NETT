using ConexaoSolidaria.Domain.Enums;

namespace ConexaoSolidaria.Domain.Validation;

public static class CampanhaRules
{
    public static void EnsureCanCreate(DateTime dataFimUtc, decimal metaFinanceira, DateTime agoraUtc)
    {
        if (dataFimUtc.Date < agoraUtc.Date)
        {
            throw new InvalidOperationException("Uma campanha não pode ser criada com a data de término no passado.");
        }

        if (metaFinanceira <= 0)
        {
            throw new InvalidOperationException("A meta financeira deve ser maior que zero.");
        }
    }

    public static void EnsureCanReceiveDonation(CampanhaStatus status)
    {
        if (status is CampanhaStatus.Concluida or CampanhaStatus.Cancelada)
        {
            throw new InvalidOperationException("A doação não pode ser feita para campanhas encerradas ou canceladas.");
        }
    }
}
