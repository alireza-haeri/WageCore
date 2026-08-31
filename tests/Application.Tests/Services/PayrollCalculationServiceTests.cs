namespace Application.Tests.Services;

public class PayrollCalculationServiceTests
{
    [Fact]
    public void Calculate_ShouldNotBeImplementedYet()
    {
        var service = new PayrollCalculationService();

        Action action = () => service.Calculate(
            null!,
            null!,
            null!,
            new DateOnly(2025, 2, 1),
            new DateOnly(2025, 2, 24),
            null!);

        action.Should().Throw<NotImplementedException>();
    }
}
