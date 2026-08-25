namespace Application.Tests.Features.Workshops.Command.DeleteWorkshop;

public class DeleteWorkshopCommandValidatorTests
{
    private readonly DeleteWorkshopCommandValidator _validator = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidWorkshopId = Guid.NewGuid();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveAnyErrors()
    {
        var command = new DeleteWorkshopCommand(ValidUserId, ValidWorkshopId);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveValidationError()
    {
        var command = new DeleteWorkshopCommand(Guid.Empty, ValidWorkshopId);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WithEmptyWorkshopId_ShouldHaveValidationError()
    {
        var command = new DeleteWorkshopCommand(ValidUserId, Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.WorkshopId);
    }

    [Fact]
    public void Validate_WithBothIdsEmpty_ShouldHaveValidationErrors()
    {
        var command = new DeleteWorkshopCommand(Guid.Empty, Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
        result.ShouldHaveValidationErrorFor(x => x.WorkshopId);
    }
}