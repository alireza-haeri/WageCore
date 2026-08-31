namespace Core.Tests.Domain.Employees;

public class ControlInsuranceTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        var insuranceDto = new EmployeeInsuranceDto(
            "INS-888",
            "کارشناس اداری",
            false,
            true,
            true,
            false,
            InsuranceCalculationProfile.MinimumLaborLaw);

        var result = Insurance.Create(insuranceDto);

        result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            result.Response!.InsuranceNumber.Should().Be("INS-888");
            result.Response.PositionInInsuranceList.Should().Be("کارشناس اداری");
            result.Response.IsSubjectTo7PercentInsurance.Should().BeFalse();
            result.Response.IsSubjectTo20PercentInsurance.Should().BeTrue();
            result.Response.IsSubjectTo3PercentInsurance.Should().BeTrue();
            result.Response.IsSubjectTo4PercentInsurance.Should().BeFalse();
            result.Response.InsuranceCalculationProfile.Should().Be(InsuranceCalculationProfile.MinimumLaborLaw);
        }
    }

    [Fact]
    public void Update_WithValidData_ShouldReturnSuccess()
    {
        var insurance = Insurance.Create(new EmployeeInsuranceDto(
            "INS-001",
            "اپراتور",
            true,
            true,
            false,
            false,
            InsuranceCalculationProfile.FullLegal)).ShouldBeSuccess();

        var result = insurance.Update(new EmployeeInsuranceDto(
            "INS-888",
            "کارشناس اداری",
            false,
            true,
            true,
            true,
            InsuranceCalculationProfile.MinimumLaborLaw));

        result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            insurance.InsuranceNumber.Should().Be("INS-888");
            insurance.PositionInInsuranceList.Should().Be("کارشناس اداری");
            insurance.IsSubjectTo7PercentInsurance.Should().BeFalse();
            insurance.IsSubjectTo20PercentInsurance.Should().BeTrue();
            insurance.IsSubjectTo3PercentInsurance.Should().BeTrue();
            insurance.IsSubjectTo4PercentInsurance.Should().BeTrue();
            insurance.InsuranceCalculationProfile.Should().Be(InsuranceCalculationProfile.MinimumLaborLaw);
        }
    }
}
