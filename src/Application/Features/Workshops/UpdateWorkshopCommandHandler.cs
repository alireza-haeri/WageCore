namespace Application.Features.Workshops;

public class UpdateWorkshopCommandHandler(IWorkShopRepository workShopRepository, IWorkshopQuery workshopQuery)
    : IRequestHandler<UpdateWorkshopCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateWorkshopCommand request, CancellationToken cancellationToken)
    {
        var workshop = await workShopRepository.GetByIdAsync(request.UserId, request.WorkshopId, cancellationToken);
        if (workshop is null)
            return Result<bool>.NotfoundFailure("کارگاه مورد نظر یافت نشد.");

        var existWorkshopName =
            await workshopQuery.IsExistWorkshopName(request.UserId, request.Name, workshop.Id, cancellationToken);
        if (existWorkshopName)
            return Result<bool>.ValidationFailure(new Dictionary<string, string[]>()
            {
                { nameof(request.Name), ["نام کارگاه تکراری است"] }
            });

        var existWorkshopNationalId =
            await workshopQuery.IsExistWorkshopNationalId(request.UserId, request.NationalId, workshop.Id,
                cancellationToken);
        if (existWorkshopNationalId)
            return Result<bool>.ValidationFailure(new Dictionary<string, string[]>()
            {
                { nameof(request.NationalId), ["شناسه ملی کارگاه تکراری است"] }
            });

        var domainResult = workshop.Update(
            request.Name,
            request.Address,
            request.RegistrationDate,
            request.NationalId,
            request.PostalCode
        );
        if (!domainResult.IsSuccess)
            return Result<bool>.GeneralFailure(domainResult.ErrorMessage!);

        var updateResult = await workShopRepository.UpdateAsync(workshop, cancellationToken);
        if (!updateResult)
            return Result<bool>.GeneralFailure("خطایی در بروزرسانی کارگاه رخ داد.");

        return Result<bool>.Success(true);
    }
}