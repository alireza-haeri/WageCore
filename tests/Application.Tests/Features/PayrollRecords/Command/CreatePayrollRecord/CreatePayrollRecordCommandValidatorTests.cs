namespace Application.Tests.Features.PayrollRecords.Command.CreatePayrollRecord;

public class CreatePayrollRecordCommandValidatorTests
{
    private readonly CreatePayrollRecordCommandValidator _validator = new();
    private readonly PayrollRecordBuilder _builder = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();

    private CreatePayrollRecordCommand CreateValidCommand(
        PayrollRecordDto? payrollRecord = null,
        Guid? userId = null,
        Guid? employeeId = null)
    {
        var dto = payrollRecord ?? _builder.BuildDto();

        return new CreatePayrollRecordCommand(
            userId ?? ValidUserId,
            employeeId ?? ValidEmployeeId,
            dto);
    }

    private PayrollRecordDto CreateHoursDto(string fieldName, decimal? value) =>
        fieldName switch
        {
            "OvertimeHours" => _builder.BuildDto() with { OvertimeHours = value },
            "NightShiftHours" => _builder.BuildDto() with { NightShiftHours = value },
            _ => _builder.BuildDto() with { FridayWorkHours = value }
        };

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveAnyErrors()
    {
        var command = CreateValidCommand();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveValidationError()
    {
        var command = CreateValidCommand(userId: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WithEmptyEmployeeId_ShouldHaveValidationError()
    {
        var command = CreateValidCommand(employeeId: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EmployeeId);
    }

    [Fact]
    public void Validate_WithNullPayrollRecord_ShouldHaveValidationError()
    {
        var command = new CreatePayrollRecordCommand(ValidUserId, ValidEmployeeId, null!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PayrollRecord);
    }

    [Fact]
    public void Validate_WithNullPeriodStart_ShouldHaveValidationError()
    {
        var payrollRecord = _builder.BuildDto() with { PeriodStart = null };
        var command = CreateValidCommand(payrollRecord);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PayrollRecord.PeriodStart);
    }

    [Fact]
    public void Validate_WithNullPeriodEnd_ShouldHaveValidationError()
    {
        var payrollRecord = _builder.BuildDto() with { PeriodEnd = null };
        var command = CreateValidCommand(payrollRecord);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PayrollRecord.PeriodEnd);
    }

    [Fact]
    public void Validate_WithPeriodEndBeforePeriodStart_ShouldHaveValidationError()
    {
        var periodStart = DateOnly.FromDateTime(DateTime.Now.AddDays(-10));
        var payrollRecord = _builder.BuildDto() with
        {
            PeriodStart = periodStart,
            PeriodEnd = periodStart.AddDays(-1)
        };
        var command = CreateValidCommand(payrollRecord);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PayrollRecord.PeriodEnd);
    }

    [Fact]
    public void Validate_WithPeriodEndEqualToPeriodStart_ShouldNotHaveValidationError()
    {
        var periodStart = DateOnly.FromDateTime(DateTime.Now.AddDays(-10));
        var payrollRecord = _builder.BuildDto() with
        {
            PeriodStart = periodStart,
            PeriodEnd = periodStart
        };
        var command = CreateValidCommand(payrollRecord);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.PayrollRecord.PeriodEnd);
    }

    [Fact]
    public void Validate_WithNullWorkedDaysCount_ShouldHaveValidationError()
    {
        var payrollRecord = _builder.BuildDto() with { WorkedDaysCount = null };
        var command = CreateValidCommand(payrollRecord);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PayrollRecord.WorkedDaysCount);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(32)]
    [InlineData(100)]
    public void Validate_WithWorkedDaysCountOutOfRange_ShouldHaveValidationError(decimal daysCount)
    {
        var payrollRecord = _builder.BuildDto() with { WorkedDaysCount = daysCount };
        var command = CreateValidCommand(payrollRecord);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PayrollRecord.WorkedDaysCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(31)]
    public void Validate_WithWorkedDaysCountOnRangeBoundary_ShouldNotHaveValidationError(decimal daysCount)
    {
        var payrollRecord = _builder.BuildDto() with { WorkedDaysCount = daysCount };
        var command = CreateValidCommand(payrollRecord);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.PayrollRecord.WorkedDaysCount);
    }

    [Theory]
    [InlineData("LeaveDaysCount")]
    [InlineData("AbsenceDaysCount")]
    [InlineData("MissionDaysCount")]
    public void Validate_WithNullOptionalDayCounts_ShouldHaveValidationError(string fieldName)
    {
        var payrollRecord = fieldName switch
        {
            "LeaveDaysCount" => _builder.BuildDto() with { LeaveDaysCount = null },
            "AbsenceDaysCount" => _builder.BuildDto() with { AbsenceDaysCount = null },
            _ => _builder.BuildDto() with { MissionDaysCount = null }
        };
        var command = CreateValidCommand(payrollRecord);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor($"PayrollRecord.{fieldName}");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(31.5)]
    public void Validate_WithLeaveDaysCountOutOfRange_ShouldHaveValidationError(double daysCount)
    {
        var payrollRecord = _builder.BuildDto() with { LeaveDaysCount = (decimal)daysCount };
        var command = CreateValidCommand(payrollRecord);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PayrollRecord.LeaveDaysCount);
    }

    [Fact]
    public void Validate_WithAbsenceDaysCountOutOfRange_ShouldHaveValidationError()
    {
        var payrollRecord = _builder.BuildDto() with { AbsenceDaysCount = 35m };
        var command = CreateValidCommand(payrollRecord);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PayrollRecord.AbsenceDaysCount);
    }

    [Fact]
    public void Validate_WithMissionDaysCountOutOfRange_ShouldHaveValidationError()
    {
        var payrollRecord = _builder.BuildDto() with { MissionDaysCount = -2m };
        var command = CreateValidCommand(payrollRecord);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PayrollRecord.MissionDaysCount);
    }

    [Fact]
    public void Validate_WithFractionalDaysCount_ShouldNotHaveValidationError()
    {
        var payrollRecord = _builder.BuildDto() with
        {
            WorkedDaysCount = 12.5m,
            LeaveDaysCount = 1.5m,
            MissionDaysCount = 0.25m
        };
        var command = CreateValidCommand(payrollRecord);

        var result = _validator.TestValidate(command);

        using (new AssertionScope())
        {
            result.ShouldNotHaveValidationErrorFor(x => x.PayrollRecord.WorkedDaysCount);
            result.ShouldNotHaveValidationErrorFor(x => x.PayrollRecord.LeaveDaysCount);
            result.ShouldNotHaveValidationErrorFor(x => x.PayrollRecord.MissionDaysCount);
        }
    }

    [Theory]
    [InlineData("OvertimeHours")]
    [InlineData("NightShiftHours")]
    [InlineData("FridayWorkHours")]
    public void Validate_WithNullHours_ShouldHaveValidationError(string fieldName)
    {
        var command = CreateValidCommand(CreateHoursDto(fieldName, null));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor($"PayrollRecord.{fieldName}");
    }

    [Theory]
    [InlineData("OvertimeHours")]
    [InlineData("NightShiftHours")]
    [InlineData("FridayWorkHours")]
    public void Validate_WithNegativeHours_ShouldHaveValidationError(string fieldName)
    {
        var command = CreateValidCommand(CreateHoursDto(fieldName, -1m));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor($"PayrollRecord.{fieldName}");
    }

    [Theory]
    [InlineData("OvertimeHours")]
    [InlineData("NightShiftHours")]
    [InlineData("FridayWorkHours")]
    public void Validate_WithZeroHours_ShouldNotHaveAnyErrors(string fieldName)
    {
        var payrollRecord = CreateHoursDto(fieldName, 0m);
        var command = CreateValidCommand(payrollRecord);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithPeriodLongerThanOneMonth_ShouldNotHaveValidationError()
    {
        var periodStart = DateOnly.FromDateTime(DateTime.Now.AddDays(-60));
        var payrollRecord = _builder.BuildDto() with
        {
            PeriodStart = periodStart,
            PeriodEnd = periodStart.AddDays(45)
        };
        var command = CreateValidCommand(payrollRecord);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithOvertimeHoursAboveAnyCap_ShouldNotHaveValidationError()
    {
        var payrollRecord = _builder.BuildDto() with { OvertimeHours = 500m };
        var command = CreateValidCommand(payrollRecord);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.PayrollRecord.OvertimeHours);
    }

}
