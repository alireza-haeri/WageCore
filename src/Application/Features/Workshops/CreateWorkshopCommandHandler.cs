using Core.Abstractions.Repositories.Workshops;

namespace Application.Features.Workshops;

public class CreateWorkshopCommandHandler(IWorkShopRepository workShopRepository)
    : IRequestHandler<CreateWorkshopCommand, Result<CreateWorkshopCommandResponse>>
{
    public async Task<Result<CreateWorkshopCommandResponse>> Handle(CreateWorkshopCommand request,
        CancellationToken cancellationToken)
    {
        var workshop = Workshop.Create(
            request.UserId,
            request.Name,
            request.Address,
            request.Region,
            request.RegistrationDate,
            request.NationalId,
            request.PostalCode);

        if (!workshop.IsSuccess)
            return Result<CreateWorkshopCommandResponse>.GeneralFailure(workshop.ErrorMessage!);
        
        var createResult = await workShopRepository.CreateAsync(workshop.Response!, cancellationToken);
        if(createResult is null)
            return Result<CreateWorkshopCommandResponse>.GeneralFailure("خطا در ایجاد کارگاه");
        
        return Result<CreateWorkshopCommandResponse>.Success(new CreateWorkshopCommandResponse(createResult.Value));
    }
}