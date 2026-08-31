using Core.Abstractions.Repositories.Workshops;

namespace Application.Features.Workshops;

public class CreateWorkshopCommandHandler(IWorkShopRepository workShopRepository, IWorkshopQuery workshopQuery)
    : IRequestHandler<CreateWorkshopCommand, Result<CreateWorkshopCommandResponse>>
{
    public async Task<Result<CreateWorkshopCommandResponse>> Handle(CreateWorkshopCommand request,
        CancellationToken cancellationToken)
    {
        var existWorkshopName =
            await workshopQuery.IsExistWorkshopName(request.UserId, request.Name, null, cancellationToken);
        if (existWorkshopName)
            return Result<CreateWorkshopCommandResponse>.ValidationFailure(new Dictionary<string, string[]>()
            {
                { nameof(request.Name), ["نام کارگاه تکراری است"] }
            });

        var existWorkshopNationalId =
            await workshopQuery.IsExistWorkshopNationalId(request.UserId, request.NationalId, null, cancellationToken);
        if (existWorkshopNationalId)
            return Result<CreateWorkshopCommandResponse>.ValidationFailure(new Dictionary<string, string[]>()
            {
                { nameof(request.NationalId), ["شناسه ملی کارگاه تکراری است"] }
            });

        var workshop = Workshop.Create(
            request.UserId,
            request.Name,
            request.Address,
            request.RegistrationDate,
            request.NationalId,
            request.PostalCode);

        if (!workshop.IsSuccess)
            return Result<CreateWorkshopCommandResponse>.GeneralFailure(workshop.ErrorMessage!);

        var createResult = await workShopRepository.CreateAsync(workshop.Response!, cancellationToken);
        if (createResult is null)
            return Result<CreateWorkshopCommandResponse>.GeneralFailure("خطا در ایجاد کارگاه");

        return Result<CreateWorkshopCommandResponse>.Success(new CreateWorkshopCommandResponse(createResult.Value));
    }
}