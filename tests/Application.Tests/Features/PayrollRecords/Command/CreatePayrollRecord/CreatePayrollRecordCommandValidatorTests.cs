using Application.Features.PayrollRecords;
using Core.Contracts.PayrollRecords;
using FluentAssertions.Execution;
using FluentValidation.TestHelper;
using Shared.Tests.Builders;

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
        UserWorkInputDto? work = null,
        Guid? userId = null,
        Guid? employeeId = null,
        int? persianYear = null,
        int? persianMonth = null) =>
        new(
            userId ?? ValidUserId,
            employeeId ?? ValidEmployeeId,
            persianYear ?? ValidPersianYear,
            persianMonth ?? ValidPersianMonth,
            work ?? _builder.BuildUserWorkInputDto());

    private UserWorkInputDto CreateHoursDto(string fieldName, WorkTimeInput value) =>
        fieldName switch
        {
            "Overtime" => _builder.BuildUserWorkInputDto() with { Overtime = value },
            "NightShift" => _builder.BuildUserWorkInputDto() with { NightShift = value },
            _ => _builder.BuildUserWorkInputDto() with { FridayWork = value }
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

    [Theory]
    [InlineData(-1)]
    [InlineData(32)]
    [InlineData(100)]
    public void Validate_WithWorkedDaysCountOutOfRange_ShouldHaveValidationError(decimal daysCount)
    {
        var work = _builder.BuildUserWorkInputDto() with { WorkedDaysCount = daysCount };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Work.WorkedDaysCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(31)]
    public void Validate_WithWorkedDaysCountOnRangeBoundary_ShouldNotHaveValidationError(decimal daysCount)
    {
        var work = _builder.BuildUserWorkInputDto() with { WorkedDaysCount = daysCount };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Work.WorkedDaysCount);
    }

    [Fact]
    public void Validate_WithNegativeLeaveHours_ShouldHaveValidationError()
    {
        var work = _builder.BuildUserWorkInputDto() with { Leave = new DayTimeInput(0, -1, 0) };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Work.Leave.Hours);
    }

    [Fact]
    public void Validate_WithNegativeLeaveMinutes_ShouldHaveValidationError()
    {
        var work = _builder.BuildUserWorkInputDto() with { Leave = new DayTimeInput(0, 1, -5) };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Work.Leave.Minutes);
    }

    [Fact]
    public void Validate_WithNegativeLeaveDays_ShouldHaveValidationError()
    {
        var work = _builder.BuildUserWorkInputDto() with { Leave = new DayTimeInput(-1, 0, 0) };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Work.Leave.Days);
    }

    [Fact]
    public void Validate_WithLeaveDaysAboveMax_ShouldHaveValidationError()
    {
        var work = _builder.BuildUserWorkInputDto() with { Leave = new DayTimeInput(32, 0, 0) };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Work.Leave.Days);
    }

    [Fact]
    public void Validate_WithLargeLeaveTime_ShouldNotHaveValidationError()
    {
        var work = _builder.BuildUserWorkInputDto() with { Leave = new DayTimeInput(0, 40, 0) };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Work.Leave.Hours);
    }

    [Fact]
    public void Validate_WithAbsenceDaysCountOutOfRange_ShouldHaveValidationError()
    {
        var work = _builder.BuildUserWorkInputDto() with { AbsenceDaysCount = 35m };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Work.AbsenceDaysCount);
    }

    [Fact]
    public void Validate_WithMissionDaysNegative_ShouldHaveValidationError()
    {
        var work = _builder.BuildUserWorkInputDto() with { MissionDays = -2 };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Work.MissionDays);
    }

    [Fact]
    public void Validate_WithMissionDaysAboveMax_ShouldHaveValidationError()
    {
        var work = _builder.BuildUserWorkInputDto() with { MissionDays = 32 };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Work.MissionDays);
    }

    [Fact]
    public void Validate_WithFractionalWorkedDaysAndTimeInMinutes_ShouldNotHaveValidationError()
    {
        var work = _builder.BuildUserWorkInputDto() with
        {
            WorkedDaysCount = 12.5m,
            Leave = new DayTimeInput(0, 1, 30),
            MissionDays = 1
        };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        using (new AssertionScope())
        {
            result.ShouldNotHaveValidationErrorFor(x => x.Work.WorkedDaysCount);
            result.ShouldNotHaveValidationErrorFor(x => x.Work.Leave.Hours);
            result.ShouldNotHaveValidationErrorFor(x => x.Work.Leave.Minutes);
            result.ShouldNotHaveValidationErrorFor(x => x.Work.MissionDays);
        }
    }

    [Theory]
    [InlineData("Overtime")]
    [InlineData("NightShift")]
    [InlineData("FridayWork")]
    public void Validate_WithNegativeHours_ShouldHaveValidationError(string fieldName)
    {
        var command = CreateValidCommand(CreateHoursDto(fieldName, new WorkTimeInput(-1, 0)));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor($"Work.{fieldName}.Hours");
    }

    [Theory]
    [InlineData("Overtime")]
    [InlineData("NightShift")]
    [InlineData("FridayWork")]
    public void Validate_WithMinutesAboveMax_ShouldHaveValidationError(string fieldName)
    {
        var command = CreateValidCommand(CreateHoursDto(fieldName, new WorkTimeInput(1, 60)));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor($"Work.{fieldName}.Minutes");
    }

    [Theory]
    [InlineData("Overtime")]
    [InlineData("NightShift")]
    [InlineData("FridayWork")]
    public void Validate_WithZeroHours_ShouldNotHaveAnyErrors(string fieldName)
    {
        var work = CreateHoursDto(fieldName, new WorkTimeInput(0, 0));
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithOvertimeHoursAboveAnyCap_ShouldNotHaveValidationError()
    {
        var work = _builder.BuildUserWorkInputDto() with { Overtime = new WorkTimeInput(500, 0) };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Work.Overtime.Hours);
    }

    [Fact]
    public void Validate_WithNegativeMissionHours_ShouldHaveValidationError()
    {
        var work = _builder.BuildUserWorkInputDto() with { MissionHours = new WorkTimeInput(-1, 0) };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Work.MissionHours.Hours);
    }

    [Fact]
    public void Validate_WithNegativeHolidayWorkHours_ShouldHaveValidationError()
    {
        var work = _builder.BuildUserWorkInputDto() with { HolidayWork = new WorkTimeInput(-2, 0) };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Work.HolidayWork.Hours);
    }

    [Theory]
    [InlineData("MissionAmountOverride")]
    [InlineData("PerformanceBonusAmount")]
    [InlineData("CashBenefitsAmount")]
    public void Validate_WithNegativeOptionalAmount_ShouldHaveValidationError(string fieldName)
    {
        var work = fieldName switch
        {
            "MissionAmountOverride" => _builder.BuildUserWorkInputDto() with { MissionAmountOverride = -1m },
            "PerformanceBonusAmount" => _builder.BuildUserWorkInputDto() with { PerformanceBonusAmount = -1m },
            _ => _builder.BuildUserWorkInputDto() with { CashBenefitsAmount = -1m }
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
            "MissionAmountOverride" => _builder.BuildUserWorkInputDto() with { MissionAmountOverride = null },
            "PerformanceBonusAmount" => _builder.BuildUserWorkInputDto() with { PerformanceBonusAmount = null },
            _ => _builder.BuildUserWorkInputDto() with { CashBenefitsAmount = null }
        };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
