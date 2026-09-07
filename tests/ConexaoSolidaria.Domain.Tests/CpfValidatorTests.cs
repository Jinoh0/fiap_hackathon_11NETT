using ConexaoSolidaria.Domain.Validation;

namespace ConexaoSolidaria.Domain.Tests;

public class CpfValidatorTests
{
    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    public void IsValid_AcceptsKnownValidCpfs(string cpf)
    {
        Assert.True(CpfValidator.IsValid(cpf));
    }

    [Theory]
    [InlineData("111.111.111-11")]
    [InlineData("123")]
    [InlineData("")]
    [InlineData(null)]
    public void IsValid_RejectsInvalidCpfs(string? cpf)
    {
        Assert.False(CpfValidator.IsValid(cpf));
    }
}
