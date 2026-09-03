using Shared.Tests.Builders;
using Shared.Tests.Helpers;

namespace Core.Tests.Domain.Employees;

/// <summary>
/// Hire-date-scenario validation for the onboarding-history fields:
/// 1) hired in the current Persian month → all three fields must be null;
/// 2) hired earlier in the current Persian year → the two current-year fields are required, carried-over is forbidden;
/// 3) hired before the current Persian year → all three fields are required.
/// </summary>
public class EmployeeOnboardingHistoryTests
{
    private readonly EmployeeBuilder _builder = new();
    private readonly FakePersianCalendarService _calendar = new();

    private DateOnly NewHireDate() => DateOnly.FromDateTime(DateTime.Now.AddDays(-3));

    [Fact]
    public void Create_WhenHiredInCurrentMonth_WithoutHistoryFields_ShouldSucceed()
    {
        var hireDate = NewHireDate();
        _calendar.HireDate = hireDate;
        _calendar.HireYear = _calendar.CurrentYear;
        _calendar.HireMonth = _calendar.CurrentMonth;

        var result = _builder
            .WithHireDate(hireDate)
            .WithPersianCalendarService(_calendar)
            .CreateResult();

        var employee = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            employee.LeaveUsedInCurrentYear.Should().BeNull();
            employee.NetWorkedDaysBeforeCurrentMonth.Should().BeNull();
            employee.CarriedOverLeaveFromPreviousYear.Should().BeNull();
        }
    }

    [Fact]
    public void Create_WhenHiredInCurrentMonth_WithLeaveUsed_ShouldFail()
    {
        var hireDate = NewHireDate();
        _calendar.HireDate = hireDate;
        _calendar.HireYear = _calendar.CurrentYear;
        _calendar.HireMonth = _calendar.CurrentMonth;

        var result = _builder
            .WithHireDate(hireDate)
            .WithLeaveUsedInCurrentYear(3)
            .WithPersianCalendarService(_calendar)
            .CreateResult();

        result.ShouldBeFailure("همین ماه استخدام شده");
    }

    [Fact]
    public void Create_WhenHiredInCurrentMonth_WithNetWorkedDays_ShouldFail()
    {
        var hireDate = NewHireDate();
        _calendar.HireDate = hireDate;
        _calendar.HireYear = _calendar.CurrentYear;
        _calendar.HireMonth = _calendar.CurrentMonth;

        var result = _builder
            .WithHireDate(hireDate)
            .WithNetWorkedDaysBeforeCurrentMonth(10)
            .WithPersianCalendarService(_calendar)
            .CreateResult();

        result.ShouldBeFailure("همین ماه استخدام شده");
    }

    [Fact]
    public void Create_WhenHiredInCurrentMonth_WithCarriedOverLeave_ShouldFail()
    {
        var hireDate = NewHireDate();
        _calendar.HireDate = hireDate;
        _calendar.HireYear = _calendar.CurrentYear;
        _calendar.HireMonth = _calendar.CurrentMonth;

        var result = _builder
            .WithHireDate(hireDate)
            .WithCarriedOverLeaveFromPreviousYear(5)
            .WithPersianCalendarService(_calendar)
            .CreateResult();

        result.ShouldBeFailure("قبل از سال جاری استخدام نشده");
    }

    [Fact]
    public void Create_WhenHiredEarlierThisYear_WithCurrentYearFields_ShouldSucceed()
    {
        var hireDate = NewHireDate();
        _calendar.HireDate = hireDate;
        _calendar.HireYear = _calendar.CurrentYear;
        _calendar.HireMonth = 3;

        var result = _builder
            .WithHireDate(hireDate)
            .WithLeaveUsedInCurrentYear(3)
            .WithNetWorkedDaysBeforeCurrentMonth(45)
            .WithPersianCalendarService(_calendar)
            .CreateResult();

        var employee = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            employee.LeaveUsedInCurrentYear.Should().Be(3);
            employee.NetWorkedDaysBeforeCurrentMonth.Should().Be(45);
            employee.CarriedOverLeaveFromPreviousYear.Should().BeNull();
        }
    }

    [Fact]
    public void Create_WhenHiredEarlierThisYear_WithoutLeaveUsed_ShouldFail()
    {
        var hireDate = NewHireDate();
        _calendar.HireDate = hireDate;
        _calendar.HireYear = _calendar.CurrentYear;
        _calendar.HireMonth = 3;

        var result = _builder
            .WithHireDate(hireDate)
            .WithNetWorkedDaysBeforeCurrentMonth(45)
            .WithPersianCalendarService(_calendar)
            .CreateResult();

        result.ShouldBeFailure("مرخصی استفاده‌شده در سال جاری اجباری");
    }

    [Fact]
    public void Create_WhenHiredEarlierThisYear_WithoutNetWorkedDays_ShouldFail()
    {
        var hireDate = NewHireDate();
        _calendar.HireDate = hireDate;
        _calendar.HireYear = _calendar.CurrentYear;
        _calendar.HireMonth = 3;

        var result = _builder
            .WithHireDate(hireDate)
            .WithLeaveUsedInCurrentYear(3)
            .WithPersianCalendarService(_calendar)
            .CreateResult();

        result.ShouldBeFailure("روز خالص کارکرد قبل از ماه جاری اجباری");
    }

    [Fact]
    public void Create_WhenHiredEarlierThisYear_WithNegativeLeaveUsed_ShouldFail()
    {
        var hireDate = NewHireDate();
        _calendar.HireDate = hireDate;
        _calendar.HireYear = _calendar.CurrentYear;
        _calendar.HireMonth = 3;

        var result = _builder
            .WithHireDate(hireDate)
            .WithLeaveUsedInCurrentYear(-1)
            .WithNetWorkedDaysBeforeCurrentMonth(45)
            .WithPersianCalendarService(_calendar)
            .CreateResult();

        result.ShouldBeFailure("مرخصی استفاده‌شده در سال جاری نمی‌تواند منفی");
    }

    [Fact]
    public void Create_WhenHiredEarlierThisYear_WithCarriedOverLeave_ShouldFail()
    {
        var hireDate = NewHireDate();
        _calendar.HireDate = hireDate;
        _calendar.HireYear = _calendar.CurrentYear;
        _calendar.HireMonth = 3;

        var result = _builder
            .WithHireDate(hireDate)
            .WithLeaveUsedInCurrentYear(3)
            .WithNetWorkedDaysBeforeCurrentMonth(45)
            .WithCarriedOverLeaveFromPreviousYear(5)
            .WithPersianCalendarService(_calendar)
            .CreateResult();

        result.ShouldBeFailure("قبل از سال جاری استخدام نشده");
    }

    [Fact]
    public void Create_WhenHiredBeforeCurrentYear_WithAllThreeFields_ShouldSucceed()
    {
        var hireDate = NewHireDate();
        _calendar.HireDate = hireDate;
        _calendar.HireYear = _calendar.CurrentYear - 1;
        _calendar.HireMonth = 4;

        var result = _builder
            .WithHireDate(hireDate)
            .WithLeaveUsedInCurrentYear(3)
            .WithNetWorkedDaysBeforeCurrentMonth(45)
            .WithCarriedOverLeaveFromPreviousYear(5)
            .WithPersianCalendarService(_calendar)
            .CreateResult();

        var employee = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            employee.LeaveUsedInCurrentYear.Should().Be(3);
            employee.NetWorkedDaysBeforeCurrentMonth.Should().Be(45);
            employee.CarriedOverLeaveFromPreviousYear.Should().Be(5);
        }
    }

    [Fact]
    public void Create_WhenHiredBeforeCurrentYear_WithoutCarriedOverLeave_ShouldFail()
    {
        var hireDate = NewHireDate();
        _calendar.HireDate = hireDate;
        _calendar.HireYear = _calendar.CurrentYear - 1;
        _calendar.HireMonth = 4;

        var result = _builder
            .WithHireDate(hireDate)
            .WithLeaveUsedInCurrentYear(3)
            .WithNetWorkedDaysBeforeCurrentMonth(45)
            .WithPersianCalendarService(_calendar)
            .CreateResult();

        result.ShouldBeFailure("مرخصی انتقال‌یافته از سال قبل اجباری");
    }

    [Fact]
    public void Create_WhenHiredBeforeCurrentYear_WithNegativeCarriedOverLeave_ShouldFail()
    {
        var hireDate = NewHireDate();
        _calendar.HireDate = hireDate;
        _calendar.HireYear = _calendar.CurrentYear - 1;
        _calendar.HireMonth = 4;

        var result = _builder
            .WithHireDate(hireDate)
            .WithLeaveUsedInCurrentYear(3)
            .WithNetWorkedDaysBeforeCurrentMonth(45)
            .WithCarriedOverLeaveFromPreviousYear(-2)
            .WithPersianCalendarService(_calendar)
            .CreateResult();

        result.ShouldBeFailure("مرخصی انتقال‌یافته از سال قبل نمی‌تواند منفی");
    }
}
