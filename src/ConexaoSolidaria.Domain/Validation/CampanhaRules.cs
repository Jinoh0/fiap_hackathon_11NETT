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

    public static void EnsureCanUpdate(decimal metaFinanceira)
    {
        if (metaFinanceira <= 0)
        {
            throw new InvalidOperationException("A meta financeira deve ser maior que zero.");
        }
    }

    public static void EnsureCanLeaveActive(CampanhaStatus status)
    {
        if (status != CampanhaStatus.Ativa)
        {
            throw new InvalidOperationException("Só é possível cancelar ou concluir uma campanha que esteja ativa.");
        }
    }

    public static void EnsureCanChangeStatus(CampanhaStatus atual, CampanhaStatus novo)
    {
        if (atual == novo)
        {
            return;
        }

        EnsureCanLeaveActive(atual);

        if (novo is not (CampanhaStatus.Concluida or CampanhaStatus.Cancelada))
        {
            throw new InvalidOperationException("A partir de Ativa, o status só pode ir para Concluida ou Cancelada.");
        }
    }
}
