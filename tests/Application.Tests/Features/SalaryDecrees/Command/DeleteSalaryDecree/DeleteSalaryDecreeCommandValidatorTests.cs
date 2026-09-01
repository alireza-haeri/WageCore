namespace Application.Tests.Features.SalaryDecrees.Command.DeleteSalaryDecree;

public class DeleteSalaryDecreeCommandValidatorTests
{
    private readonly DeleteSalaryDecreeCommandValidator _validator = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private static readonly Guid ValidSalaryProfileId = Guid.NewGuid();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveAnyErrors()
    {
        var command = new DeleteSalaryDecreeCommand(ValidUserId, ValidEmployeeId, ValidSalaryProfileId);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveValidationError()
    {
        var command = new DeleteSalaryDecreeCommand(Guid.Empty, ValidEmployeeId, ValidSalaryProfileId);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WithEmptyEmployeeId_ShouldHaveValidationError()
    {
        var command = new DeleteSalaryDecreeCommand(ValidUserId, Guid.Empty, ValidSalaryProfileId);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EmployeeId);
    }

    [Fact]
    public void Validate_WithEmptySalaryProfileId_ShouldHaveValidationError()
    {
        var command = new DeleteSalaryDecreeCommand(ValidUserId, ValidEmployeeId, Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SalaryDecreeId);
    }

    [Fact]
    public void Validate_WithAllIdsEmpty_ShouldHaveValidationErrors()
    {
        var command = new DeleteSalaryDecreeCommand(Guid.Empty, Guid.Empty, Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
        result.ShouldHaveValidationErrorFor(x => x.EmployeeId);
        result.ShouldHaveValidationErrorFor(x => x.SalaryDecreeId);
    }
}
