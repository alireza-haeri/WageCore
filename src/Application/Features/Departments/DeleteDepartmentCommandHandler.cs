namespace Application.Features.Departments;

public class DeleteDepartmentCommandHandler(IWorkShopRepository workShopRepository)
    : IRequestHandler<DeleteDepartmentCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        var workshop = await workShopRepository.GetByDepartmentIdAsync(request.UserId, request.DepartmentId,
            cancellationToken);
        if (workshop is null)
            return Result<bool>.GeneralFailure("خطایی در حذف دپارتمان رخ داد.");

        var domainResult = workshop.DeleteDepartment(request.DepartmentId);
        if (!domainResult.IsSuccess)
            return Result<bool>.GeneralFailure(domainResult.ErrorMessage!);

        var updateResult = await workShopRepository.UpdateAsync(workshop, cancellationToken);
        if (!updateResult)
            return Result<bool>.GeneralFailure("خطایی در حذف دپارتمان رخ داد.");

        return Result<bool>.Success(true);
    }
}
