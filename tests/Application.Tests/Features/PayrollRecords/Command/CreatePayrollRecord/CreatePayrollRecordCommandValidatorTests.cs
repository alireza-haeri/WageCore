namespace Application.Tests.Features.PayrollRecords.Command.CreatePayrollRecord;

public class CreatePayrollRecordCommandValidatorTests
{
    private readonly CreatePayrollRecordCommandValidator _validator = new();
    private readonly PayrollRecordBuilder _builder = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private const int ValidPersianYear = 1404;
    private const int ValidPersianMonth = 6;

    private CreatePayrollRecordCommand CreateValidCommand(
        PayrollWorkInputDto? work = null,
        Guid? userId = null,
        Guid? employeeId = null,
        int? persianYear = null,
        int? persianMonth = null) =>
        new(
            userId ?? ValidUserId,
            employeeId ?? ValidEmployeeId,
            persianYear ?? ValidPersianYear,
            persianMonth ?? ValidPersianMonth,
            work ?? _builder.BuildDto());

    private PayrollWorkInputDto CreateHoursDto(string fieldName, decimal? value) =>
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
    public void Validate_WithNullWork_ShouldHaveValidationError()
    {
        var command = new CreatePayrollRecordCommand(
            ValidUserId,
            ValidEmployeeId,
            ValidPersianYear,
            ValidPersianMonth,
            null!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Work);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithNonPositivePersianYear_ShouldHaveValidationError(int persianYear)
    {
        var command = CreateValidCommand(persianYear: persianYear);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PersianYear);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(13)]
    public void Validate_WithPersianMonthOutOfRange_ShouldHaveValidationError(int persianMonth)
    {
        var command = CreateValidCommand(persianMonth: persianMonth);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PersianMonth);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(12)]
    public void Validate_WithValidPersianMonth_ShouldNotHaveValidationError(int persianMonth)
    {
        var command = CreateValidCommand(persianMonth: persianMonth);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.PersianMonth);
    }

    [Fact]
    public void Validate_WithFuturePersianYear_ShouldNotHaveValidationError()
    {
        var command = CreateValidCommand(persianYear: 1500, persianMonth: 12);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNullWorkedDaysCount_ShouldHaveValidationError()
    {
        var work = _builder.BuildDto() with { WorkedDaysCount = null };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Work.WorkedDaysCount);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(32)]
    [InlineData(100)]
    public void Validate_WithWorkedDaysCountOutOfRange_ShouldHaveValidationError(decimal daysCount)
    {
        var work = _builder.BuildDto() with { WorkedDaysCount = daysCount };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Work.WorkedDaysCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(31)]
    public void Validate_WithWorkedDaysCountOnRangeBoundary_ShouldNotHaveValidationError(decimal daysCount)
    {
        var work = _builder.BuildDto() with { WorkedDaysCount = daysCount };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Work.WorkedDaysCount);
    }

    [Theory]
    [InlineData("LeaveHours")]
    [InlineData("AbsenceDaysCount")]
    [InlineData("MissionDaysCount")]
    public void Validate_WithNullOptionalCounts_ShouldHaveValidationError(string fieldName)
    {
        var work = fieldName switch
        {
            "LeaveHours" => _builder.BuildDto() with { LeaveHours = null },
            "AbsenceDaysCount" => _builder.BuildDto() with { AbsenceDaysCount = null },
            _ => _builder.BuildDto() with { MissionDaysCount = null }
        };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor($"Work.{fieldName}");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-31.5)]
    public void Validate_WithNegativeLeaveHours_ShouldHaveValidationError(double leaveHours)
    {
        var work = _builder.BuildDto() with { LeaveHours = (decimal)leaveHours };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Work.LeaveHours);
    }

    [Fact]
    public void Validate_WithLargeLeaveHours_ShouldNotHaveValidationError()
    {
        var work = _builder.BuildDto() with { LeaveHours = 40m };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Work.LeaveHours);
    }

    [Fact]
    public void Validate_WithAbsenceDaysCountOutOfRange_ShouldHaveValidationError()
    {
        var work = _builder.BuildDto() with { AbsenceDaysCount = 35m };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Work.AbsenceDaysCount);
    }

    [Fact]
    public void Validate_WithMissionDaysCountOutOfRange_ShouldHaveValidationError()
    {
        var work = _builder.BuildDto() with { MissionDaysCount = -2m };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Work.MissionDaysCount);
    }

    [Fact]
    public void Validate_WithFractionalDaysCount_ShouldNotHaveValidationError()
    {
        var work = _builder.BuildDto() with
        {
            WorkedDaysCount = 12.5m,
            LeaveHours = 1.5m,
            MissionDaysCount = 0.25m
        };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        using (new AssertionScope())
        {
            result.ShouldNotHaveValidationErrorFor(x => x.Work.WorkedDaysCount);
            result.ShouldNotHaveValidationErrorFor(x => x.Work.LeaveHours);
            result.ShouldNotHaveValidationErrorFor(x => x.Work.MissionDaysCount);
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

        result.ShouldHaveValidationErrorFor($"Work.{fieldName}");
    }

    [Theory]
    [InlineData("OvertimeHours")]
    [InlineData("NightShiftHours")]
    [InlineData("FridayWorkHours")]
    public void Validate_WithNegativeHours_ShouldHaveValidationError(string fieldName)
    {
        var command = CreateValidCommand(CreateHoursDto(fieldName, -1m));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor($"Work.{fieldName}");
    }

    [Theory]
    [InlineData("OvertimeHours")]
    [InlineData("NightShiftHours")]
    [InlineData("FridayWorkHours")]
    public void Validate_WithZeroHours_ShouldNotHaveAnyErrors(string fieldName)
    {
        var work = CreateHoursDto(fieldName, 0m);
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithOvertimeHoursAboveAnyCap_ShouldNotHaveValidationError()
    {
        var work = _builder.BuildDto() with { OvertimeHours = 500m };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Work.OvertimeHours);
    }

    [Fact]
    public void Validate_WithNullMissionHours_ShouldHaveValidationError()
    {
        var work = _builder.BuildDto() with { MissionHours = null };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Work.MissionHours);
    }

    [Fact]
    public void Validate_WithNegativeMissionHours_ShouldHaveValidationError()
    {
        var work = _builder.BuildDto() with { MissionHours = -1m };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Work.MissionHours);
    }

    [Fact]
    public void Validate_WithNullHolidayWorkHours_ShouldHaveValidationError()
    {
        var work = _builder.BuildDto() with { HolidayWorkHours = null };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Work.HolidayWorkHours);
    }

    [Fact]
    public void Validate_WithNegativeHolidayWorkHours_ShouldHaveValidationError()
    {
        var work = _builder.BuildDto() with { HolidayWorkHours = -2m };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Work.HolidayWorkHours);
    }

    [Theory]
    [InlineData("MissionAmountOverride")]
    [InlineData("PerformanceBonusAmount")]
    [InlineData("CashBenefitsAmount")]
    public void Validate_WithNegativeOptionalAmount_ShouldHaveValidationError(string fieldName)
    {
        var work = fieldName switch
        {
            "MissionAmountOverride" => _builder.BuildDto() with { MissionAmountOverride = -1m },
            "PerformanceBonusAmount" => _builder.BuildDto() with { PerformanceBonusAmount = -1m },
            _ => _builder.BuildDto() with { CashBenefitsAmount = -1m }
        };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor($"Work.{fieldName}");
    }

    [Theory]
    [InlineData("MissionAmountOverride")]
    [InlineData("PerformanceBonusAmount")]
    [InlineData("CashBenefitsAmount")]
    public void Validate_WithNullOptionalAmount_ShouldNotHaveAnyErrors(string fieldName)
    {
        var work = fieldName switch
        {
            "MissionAmountOverride" => _builder.BuildDto() with { MissionAmountOverride = null },
            "PerformanceBonusAmount" => _builder.BuildDto() with { PerformanceBonusAmount = null },
            _ => _builder.BuildDto() with { CashBenefitsAmount = null }
        };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(27)]
    [InlineData(32)]
    public void Validate_WithStandardWorkingDaysCountOutOfRange_ShouldHaveValidationError(int standardWorkingDaysCount)
    {
        var work = _builder.BuildDto() with { StandardWorkingDaysCount = standardWorkingDaysCount };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Work.StandardWorkingDaysCount);
    }

    [Theory]
    [InlineData(28)]
    [InlineData(31)]
    public void Validate_WithStandardWorkingDaysCountOnRangeBoundary_ShouldNotHaveValidationError(int standardWorkingDaysCount)
    {
        var work = _builder.BuildDto() with { StandardWorkingDaysCount = standardWorkingDaysCount };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Work.StandardWorkingDaysCount);
    }
}
