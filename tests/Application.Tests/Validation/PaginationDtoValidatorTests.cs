namespace Application.Tests.Validation;

public class PaginationDtoValidatorTests
{
    private readonly PaginationDtoValidator _validator = new();

    [Fact]
    public void Validate_WithValidPagination_ShouldNotHaveAnyErrors()
    {
        var dto = new PaginationDto(1, 10);
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithPageNumberGreaterThanZero_ShouldNotHaveErrors()
    {
        var dto = new PaginationDto(5, 10);
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithPageSizeBetweenOneAndHundred_ShouldNotHaveErrors()
    {
        var dto = new PaginationDto(1, 50);
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithPageSizeExactlyOne_ShouldNotHaveErrors()
    {
        var dto = new PaginationDto(1, 1);
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithPageSizeExactlyHundred_ShouldNotHaveErrors()
    {
        var dto = new PaginationDto(1, 100);
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Validate_WithPageNumberLessThanOrEqualToZero_ShouldHaveValidationError(int pageNumber)
    {
        var dto = new PaginationDto(pageNumber, 10);
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.PageNumber);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validate_WithPageSizeLessThanOrEqualToZero_ShouldHaveValidationError(int pageSize)
    {
        var dto = new PaginationDto(1, pageSize);
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Theory]
    [InlineData(101)]
    [InlineData(200)]
    [InlineData(1000)]
    public void Validate_WithPageSizeGreaterThanHundred_ShouldHaveValidationError(int pageSize)
    {
        var dto = new PaginationDto(1, pageSize);
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }
    
    [Fact]
    public void Map_ShouldTransformItemsCorrectly()
    {
        var original = new PagedResult<int>(
            Items: [1, 2, 3],
            TotalCount: 3,
            PageNumber: 1,
            PageSize: 10
        );

        var mapped = original.Map(x => x.ToString());

        mapped.Items.Should().BeEquivalentTo(["1", "2", "3"]);
        mapped.TotalCount.Should().Be(3);
        mapped.PageNumber.Should().Be(1);
        mapped.PageSize.Should().Be(10);
        mapped.TotalPages.Should().Be(1);
    }
}