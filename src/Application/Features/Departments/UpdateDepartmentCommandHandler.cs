using Core.Abstractions.Repositories.Departments;

namespace Application.Features.Departments;

public class UpdateDepartmentCommandHandler(IWorkShopRepository workShopRepository, IDepartmentQuery departmentQuery)
    : IRequestHandler<UpdateDepartmentCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var workshop = await workShopRepository.GetByDepartmentIdAsync(request.UserId, request.DepartmentId,
            cancellationToken);
        if (workshop is null)
            return Result<bool>.NotfoundFailure("دپارتمان مورد نظر یافت نشد.");

        var existDepartmentName =
            await departmentQuery.IsExistDepartmentName(request.UserId, request.Name, request.DepartmentId,
                cancellationToken);
        if (existDepartmentName)
            return Result<bool>.ValidationFailure(new Dictionary<string, string[]>()
            {
                { nameof(request.Name), ["نام دپارتمان تکراری است"] }
            });

        var domainResult = workshop.UpdateDepartment(request.DepartmentId, request.Name);
        if (!domainResult.IsSuccess)
            return Result<bool>.GeneralFailure(domainResult.ErrorMessage!);

        var updateResult = await workShopRepository.UpdateAsync(workshop, cancellationToken);
        if (!updateResult)
            return Result<bool>.GeneralFailure("خطایی در بروزرسانی دپارتمان رخ داد.");

        return Result<bool>.Success(true);
    }
}
