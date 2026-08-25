using Core.Abstractions.Repositories.Workshops;

namespace Application.Features.Workshops;

public class DeleteWorkshopCommandHandler ( IWorkShopRepository workShopRepository)
: IRequestHandler<DeleteWorkshopCommand,Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteWorkshopCommand request, CancellationToken cancellationToken)
    {
        var deleteResult = await workShopRepository.DeleteAsync(request.UserId, request.WorkshopId, cancellationToken);
        if(!deleteResult)
            return Result<bool>.GeneralFailure("خطایی در حذف کارگاه رخ داد.");
        
        return Result<bool>.Success(true);
    }
}