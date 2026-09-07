using System.Text.RegularExpressions;

namespace ConexaoSolidaria.Domain.Validation;

public static partial class CpfValidator
{
    public static bool IsValid(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
        {
            return false;
        }

        var digits = DigitsOnly().Replace(cpf, string.Empty);
        if (digits.Length != 11)
        {
            return false;
        }

        if (digits.Distinct().Count() == 1)
        {
            return false;
        }

        var numbers = digits.Select(c => c - '0').ToArray();
        var sum1 = 0;
        for (var i = 0; i < 9; i++)
        {
            sum1 += numbers[i] * (10 - i);
        }

        var rem1 = sum1 % 11;
        var d1 = rem1 < 2 ? 0 : 11 - rem1;
        if (numbers[9] != d1)
        {
            return false;
        }

        var sum2 = 0;
        for (var i = 0; i < 10; i++)
        {
            sum2 += numbers[i] * (11 - i);
        }

        var rem2 = sum2 % 11;
        var d2 = rem2 < 2 ? 0 : 11 - rem2;
        return numbers[10] == d2;
    }

    public static string Normalize(string cpf) => DigitsOnly().Replace(cpf, string.Empty);

    [GeneratedRegex(@"\D")]
    private static partial Regex DigitsOnly();
}
