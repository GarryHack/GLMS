namespace GLMS.Tests;

public class CurrencyCalculationTests
{
    private static decimal Convert(decimal amountUsd, decimal rate) =>
        Math.Round(amountUsd * rate, 2);

    [Fact]
    public void OneHundredUsd_AtFallbackRate_Returns1850()
    {
        Assert.Equal(1850.00m, Convert(100m, 18.50m));
    }

    [Fact]
    public void ZeroUsd_ReturnsZeroZar()
    {
        Assert.Equal(0.00m, Convert(0m, 18.50m));
    }

    [Theory]
    [InlineData(100,    18.50,  1850.00)]
    [InlineData(0,      18.50,     0.00)]
    [InlineData(50,     20.00,  1000.00)]
    [InlineData(1,      18.50,    18.50)]
    [InlineData(250,    18.50,  4625.00)]
    [InlineData(99.99,  18.50,  1849.82)]
    public void ConvertUsdToZar_ReturnsExpectedAmount(decimal usd, decimal rate, decimal expected)
    {
        Assert.Equal(expected, Convert(usd, rate));
    }
}
