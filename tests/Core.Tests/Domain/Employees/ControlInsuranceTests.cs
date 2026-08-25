namespace Core.Tests.Domain.Employees;

public class ControlInsuranceTests
{
    private readonly EmployeeBuilder _builder = new();

    [Fact]
    public void UpdateInsurance_WithValidData_ShouldReturnSuccess()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();
        var insuranceDto = new EmployeeInsuranceDto(
            "INS-888",
            null,
            "کارشناس اداری",
            false,
            true,
            true,
            InsuranceCalculationProfile.MinimumLaborLaw);

        var result = employee.UpdateInsurance(insuranceDto);

        result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            employee.Insurance.InsuranceNumber.Should().Be("INS-888");
            employee.Insurance.SocialSecurityContractRow.Should().BeNull();
            employee.Insurance.PositionInInsuranceList.Should().Be("کارشناس اداری");
            employee.Insurance.IsSubjectTo7PercentInsurance.Should().BeFalse();
            employee.Insurance.IsSubjectTo20PercentInsurance.Should().BeTrue();
            employee.Insurance.IsSubjectTo3PercentInsurance.Should().BeTrue();
            employee.Insurance.InsuranceCalculationProfile.Should().Be(InsuranceCalculationProfile.MinimumLaborLaw);
        }
    }

    [Fact]
    public void UpdateInsurance_WhenEmployeeIsTerminated_ShouldFail()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();
        employee.Terminate(DateOnly.FromDateTime(DateTime.Now)).ShouldBeSuccess();
        var insuranceDto = new EmployeeInsuranceDto(
            "INS-888",
            null,
            "کارشناس اداری",
            false,
            true,
            true,
            InsuranceCalculationProfile.MinimumLaborLaw);

        var result = employee.UpdateInsurance(insuranceDto);

        result.ShouldBeFailure("کارمند ترک کار شده است");
    }
}
