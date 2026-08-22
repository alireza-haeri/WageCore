using Core.Abstractions.Repositories.Workshops;

namespace Application.Features.Workshops;

public class UpdateWorkshopCommandHandler(IWorkShopRepository workShopRepository)
    : IRequestHandler<UpdateWorkshopCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateWorkshopCommand request, CancellationToken cancellationToken)
    {
        var workshop = await workShopRepository.GetByIdAsync(request.UserId, request.WorkshopId, cancellationToken);
        if( workshop is null)
            return Result<bool>.NotfoundFailure("کارگاه مورد نظر یافت نشد.");
        
        var domainResult = workshop.Update(
            request.Name,
            request.Address,
            request.Region,
            request.RegistrationDate,
            request.NationalId,
            request.PostalCode
        );
        if(!domainResult.IsSuccess)
            return Result<bool>.GeneralFailure(domainResult.ErrorMessage!);
        
        var updateResult = await workShopRepository.UpdateAsync(workshop, cancellationToken);
        if(!updateResult)
            return Result<bool>.GeneralFailure("خطایی در بروزرسانی کارگاه رخ داد.");

        return Result<bool>.Success(true);
    }
}