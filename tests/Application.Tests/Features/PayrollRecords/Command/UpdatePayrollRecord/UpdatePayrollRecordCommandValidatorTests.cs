using Application.Features.PayrollRecords;
using Core.Contracts.PayrollRecords;
using FluentValidation.TestHelper;
using Shared.Tests.Builders;

namespace Application.Tests.Features.PayrollRecords.Command.UpdatePayrollRecord;

public class UpdatePayrollRecordCommandValidatorTests
{
    private readonly UpdatePayrollRecordCommandValidator _validator = new();
    private readonly PayrollRecordBuilder _builder = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private static readonly Guid ValidPayrollRecordId = Guid.NewGuid();
    private const int ValidPersianYear = 1404;
    private const int ValidPersianMonth = 6;

    private UpdatePayrollRecordCommand CreateValidCommand(
        UserWorkInputDto? work = null,
        Guid? userId = null,
        Guid? employeeId = null,
        Guid? payrollRecordId = null,
        int? persianYear = null,
        int? persianMonth = null) =>
        new(
            userId ?? ValidUserId,
            employeeId ?? ValidEmployeeId,
            payrollRecordId ?? ValidPayrollRecordId,
            persianYear ?? ValidPersianYear,
            persianMonth ?? ValidPersianMonth,
            work ?? _builder.BuildUserWorkInputDto());

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
    public void Validate_WithEmptyPayrollRecordId_ShouldHaveValidationError()
    {
        var command = CreateValidCommand(payrollRecordId: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PayrollRecordId);
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

    [Fact]
    public void Validate_WithNullWork_ShouldHaveValidationError()
    {
        var command = new UpdatePayrollRecordCommand(
            ValidUserId,
            ValidEmployeeId,
            ValidPayrollRecordId,
            ValidPersianYear,
            ValidPersianMonth,
            null!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Work);
    }

    [Theory]
    [InlineData("WorkedDaysCount")]
    [InlineData("AbsenceDaysCount")]
    [InlineData("MissionDays")]
    public void Validate_ShouldRunTheNestedWorkValidatorOnDayCounts(string fieldName)
    {
        var work = fieldName switch
        {
            "WorkedDaysCount" => _builder.BuildUserWorkInputDto() with { WorkedDaysCount = 40m },
            "AbsenceDaysCount" => _builder.BuildUserWorkInputDto() with { AbsenceDaysCount = 40m },
            _ => _builder.BuildUserWorkInputDto() with { MissionDays = 40 }
        };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor($"Work.{fieldName}");
    }

    [Theory]
    [InlineData("Overtime")]
    [InlineData("NightShift")]
    [InlineData("FridayWork")]
    public void Validate_ShouldRunTheNestedWorkValidatorOnHourCounts(string fieldName)
    {
        var work = fieldName switch
        {
            "Overtime" => _builder.BuildUserWorkInputDto() with { Overtime = new WorkTimeInput(-1, 0) },
            "NightShift" => _builder.BuildUserWorkInputDto() with { NightShift = new WorkTimeInput(-1, 0) },
            _ => _builder.BuildUserWorkInputDto() with { FridayWork = new WorkTimeInput(-1, 0) }
        };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor($"Work.{fieldName}.Hours");
    }

    [Fact]
    public void Validate_WithOutOfRangeLeaveDays_ShouldHaveValidationError()
    {
        var work = _builder.BuildUserWorkInputDto() with { Leave = new DayTimeInput(40, 0, 0) };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Work.Leave.Days);
    }

    [Fact]
    public void Validate_WithNegativeMissionHours_ShouldHaveValidationError()
    {
        var work = _builder.BuildUserWorkInputDto() with { MissionHours = new WorkTimeInput(-1, 0) };
        var command = CreateValidCommand(work);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Work.MissionHours.Hours);
    }
}
