namespace Application.Features.CalculationFormulas;

public class DeleteCalculationFormulaCommandHandler(ICalculationFormulaRepository calculationFormulaRepository)
    : IRequestHandler<DeleteCalculationFormulaCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteCalculationFormulaCommand request, CancellationToken cancellationToken)
    {
        var formula = await calculationFormulaRepository.GetByIdAsync(request.CalculationFormulaId, cancellationToken);
        if (formula is null)
            return Result<bool>.NotfoundFailure("فرمول مورد نظر یافت نشد.");

        var deleteResult = await calculationFormulaRepository.DeleteAsync(request.CalculationFormulaId, cancellationToken);
        if (!deleteResult)
            return Result<bool>.GeneralFailure("خطایی در حذف فرمول رخ داد.");

        return Result<bool>.Success(true);
    }
}
