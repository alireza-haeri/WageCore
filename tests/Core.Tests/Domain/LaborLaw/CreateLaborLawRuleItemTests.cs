using Core.Domain.Enums;

namespace Core.Tests.Domain.LaborLaw;

public class CreateLaborLawRuleItemTests
{
    private readonly LaborLawRuleItemBuilder _builder = new();

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        var result = _builder.CreateResult();

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.Id.Should().NotBeEmpty();
            response.Key.Should().Be(LaborLawRuleKey.MinimumDailySalary);
            response.Value.Should().Be(71_661_840m);
            response.EffectiveFrom.Should().NotBe(default);
        }
    }

    [Fact]
    public void Create_WithAllValidFields_ShouldReturnSuccess()
    {
        var id = Guid.NewGuid();
        var effectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-10));

        var result = _builder
            .WithId(id)
            .WithKey(LaborLawRuleKey.MinimumDailySalary)
            .WithValue(103_909_680m)
            .WithEffectiveFrom(effectiveFrom)
            .CreateResult();

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.Id.Should().Be(id);
            response.Key.Should().Be(LaborLawRuleKey.MinimumDailySalary);
            response.Value.Should().Be(103_909_680m);
            response.EffectiveFrom.Should().Be(effectiveFrom);
        }
    }

    [Fact]
    public void Create_WithGeneratedId_ShouldReturnSuccess()
    {
        var effectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));

        var result = LaborLawRuleItem.Create(
            LaborLawRuleKey.MinimumDailySalary,
            71_661_840m,
            effectiveFrom);

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.Id.Should().NotBeEmpty();
            response.Key.Should().Be(LaborLawRuleKey.MinimumDailySalary);
            response.Value.Should().Be(71_661_840m);
            response.EffectiveFrom.Should().Be(effectiveFrom);
        }
    }

    [Fact]
    public void Create_WithEmptyId_ShouldFail()
    {
        var result = _builder.WithId(Guid.Empty).CreateResult();

        result.ShouldBeFailure("شناسه قانون نمیتواند خالی باشد.");
    }

    [Fact]
    public void Create_WithNegativeValue_ShouldFail()
    {
        var result = _builder.WithValue(-1).CreateResult();

        result.ShouldBeFailure("مقدار قانون نمیتواند منفی باشد.");
    }

    [Fact]
    public void Create_WithZeroValue_ShouldReturnSuccess()
    {
        var result = _builder.WithValue(0).CreateResult();

        var response = result.ShouldBeSuccess();
        response.Value.Should().Be(0);
    }

    [Fact]
    public void Create_WithNullEffectiveFrom_ShouldFail()
    {
        var result = _builder.WithEffectiveFrom(null).CreateResult();

        result.ShouldBeFailure("تاریخ اجرا نمیتواند خالی باشد.");
    }
}
