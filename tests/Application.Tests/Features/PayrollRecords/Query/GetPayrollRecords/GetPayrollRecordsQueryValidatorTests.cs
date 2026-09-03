using Application.Features.PayrollRecords.Query.GetPayrollRecords;
using Core.Contracts;
using FluentValidation.TestHelper;

namespace Application.Tests.Features.PayrollRecords.Query.GetPayrollRecords;

public class GetPayrollRecordsQueryValidatorTests
{
    private readonly GetPayrollRecordsQueryValidator _validator = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();

    private GetPayrollRecordsQuery CreateValidQuery(
        Guid? userId = null,
        Guid? workshopId = null,
        Guid? departmentId = null,
        string? search = null,
        int? persianYear = null,
        int? persianMonth = null) =>
        new(
            userId ?? ValidUserId,
            new PaginationDto(1, 10),
            search,
            workshopId,
            departmentId,
            persianYear,
            persianMonth);

    [Fact]
    public void Validate_WithValidQuery_ShouldNotHaveAnyErrors()
    {
        var query = CreateValidQuery(
            workshopId: Guid.NewGuid(),
            departmentId: Guid.NewGuid(),
            search: "رضا محمدی",
            persianYear: 1404,
            persianMonth: 6);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveValidationError()
    {
        var query = CreateValidQuery(userId: Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WithEmptyWorkshopId_ShouldHaveValidationError()
    {
        var query = CreateValidQuery(workshopId: Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.WorkshopId);
    }

    [Fact]
    public void Validate_WithEmptyDepartmentId_ShouldHaveValidationError()
    {
        var query = CreateValidQuery(departmentId: Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.DepartmentId);
    }

    [Fact]
    public void Validate_WithSearchTooLong_ShouldHaveValidationError()
    {
        var query = CreateValidQuery(search: new string('a', 101));

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Search);
    }

    [Fact]
    public void Validate_WithNonPositivePersianYear_ShouldHaveValidationError()
    {
        var query = CreateValidQuery(persianYear: 0);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.PersianYear);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void Validate_WithPersianMonthOutOfRange_ShouldHaveValidationError(int persianMonth)
    {
        var query = CreateValidQuery(persianYear: 1404, persianMonth: persianMonth);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.PersianMonth);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(12)]
    public void Validate_WithValidPersianMonth_ShouldNotHaveValidationError(int persianMonth)
    {
        var query = CreateValidQuery(persianYear: 1404, persianMonth: persianMonth);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.PersianMonth);
    }

    [Fact]
    public void Validate_WithPersianMonthWithoutPersianYear_ShouldHaveValidationError()
    {
        var query = CreateValidQuery(persianMonth: 6);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.PersianMonth);
    }
}
