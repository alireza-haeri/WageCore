using Core.Abstractions.Repositories.Departments;

namespace Application.Features.Departments;

public class CreateDepartmentCommandHandler(IWorkShopRepository workShopRepository, IDepartmentQuery departmentQuery)
    : IRequestHandler<CreateDepartmentCommand, Result<CreateDepartmentCommandResponse>>
{
    public async Task<Result<CreateDepartmentCommandResponse>> Handle(CreateDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var workshop = await workShopRepository.GetByIdAsync(request.UserId, request.WorkshopId, cancellationToken);
        if (workshop is null)
            return Result<CreateDepartmentCommandResponse>.NotfoundFailure("کارگاه مورد نظر یافت نشد.");

        var existDepartmentName =
            await departmentQuery.IsExistDepartmentName(workshop.Id, request.Name, null, cancellationToken);
        if (existDepartmentName)
            return Result<CreateDepartmentCommandResponse>.ValidationFailure(new Dictionary<string, string[]>()
            {
                { nameof(request.Name), ["نام بخش تکراری است"] }
            });

        var departmentResult = workshop.CreateDepartment(request.Name);
        if (!departmentResult.IsSuccess)
            return Result<CreateDepartmentCommandResponse>.GeneralFailure(departmentResult.ErrorMessage!);

        var updateResult = await workShopRepository.UpdateAsync(workshop, cancellationToken);
        if (!updateResult)
            return Result<CreateDepartmentCommandResponse>.GeneralFailure("خطا در ایجاد بخش");

        return Result<CreateDepartmentCommandResponse>.Success(
            new CreateDepartmentCommandResponse(departmentResult.Response!.Id));
    }
}
