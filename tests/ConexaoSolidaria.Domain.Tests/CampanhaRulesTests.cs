using ConexaoSolidaria.Domain.Enums;
using ConexaoSolidaria.Domain.Validation;

namespace ConexaoSolidaria.Domain.Tests;

public class CampanhaRulesTests
{
    [Fact]
    public void EnsureCanCreate_RejectsPastEndDate()
    {
        var agora = new DateTime(2026, 9, 7, 12, 0, 0, DateTimeKind.Utc);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            CampanhaRules.EnsureCanCreate(agora.AddDays(-1), 100m, agora));
        Assert.Contains("término no passado", ex.Message);
    }

    [Fact]
    public void EnsureCanCreate_RejectsZeroMeta()
    {
        var agora = new DateTime(2026, 9, 7, 12, 0, 0, DateTimeKind.Utc);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            CampanhaRules.EnsureCanCreate(agora.AddDays(10), 0m, agora));
        Assert.Contains("maior que zero", ex.Message);
    }

    [Fact]
    public void EnsureCanCreate_AcceptsValidCampaign()
    {
        var agora = new DateTime(2026, 9, 7, 12, 0, 0, DateTimeKind.Utc);
        CampanhaRules.EnsureCanCreate(agora.AddDays(30), 5000m, agora);
    }

    [Theory]
    [InlineData(CampanhaStatus.Concluida)]
    [InlineData(CampanhaStatus.Cancelada)]
    public void EnsureCanReceiveDonation_RejectsClosedCampaigns(CampanhaStatus status)
    {
        Assert.Throws<InvalidOperationException>(() => CampanhaRules.EnsureCanReceiveDonation(status));
    }

    [Fact]
    public void EnsureCanReceiveDonation_AllowsActive()
    {
        CampanhaRules.EnsureCanReceiveDonation(CampanhaStatus.Ativa);
    }

    [Fact]
    public void EnsureCanUpdate_RejectsZeroMeta()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => CampanhaRules.EnsureCanUpdate(0m));
        Assert.Contains("maior que zero", ex.Message);
    }

    [Fact]
    public void EnsureCanLeaveActive_AllowsActive()
    {
        CampanhaRules.EnsureCanLeaveActive(CampanhaStatus.Ativa);
    }

    [Theory]
    [InlineData(CampanhaStatus.Concluida)]
    [InlineData(CampanhaStatus.Cancelada)]
    public void EnsureCanLeaveActive_RejectsClosedCampaigns(CampanhaStatus status)
    {
        var ex = Assert.Throws<InvalidOperationException>(() => CampanhaRules.EnsureCanLeaveActive(status));
        Assert.Contains("esteja ativa", ex.Message);
    }

    [Fact]
    public void EnsureCanChangeStatus_AllowsSameStatus()
    {
        CampanhaRules.EnsureCanChangeStatus(CampanhaStatus.Ativa, CampanhaStatus.Ativa);
        CampanhaRules.EnsureCanChangeStatus(CampanhaStatus.Concluida, CampanhaStatus.Concluida);
    }

    [Theory]
    [InlineData(CampanhaStatus.Concluida)]
    [InlineData(CampanhaStatus.Cancelada)]
    public void EnsureCanChangeStatus_AllowsLeavingActive(CampanhaStatus destino)
    {
        CampanhaRules.EnsureCanChangeStatus(CampanhaStatus.Ativa, destino);
    }

    [Fact]
    public void EnsureCanChangeStatus_RejectsReopening()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            CampanhaRules.EnsureCanChangeStatus(CampanhaStatus.Cancelada, CampanhaStatus.Ativa));
        Assert.Contains("esteja ativa", ex.Message);
    }
}
