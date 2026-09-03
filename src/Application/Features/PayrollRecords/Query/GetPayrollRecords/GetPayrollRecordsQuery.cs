using Core.Contracts;

namespace Application.Features.PayrollRecords.Query.GetPayrollRecords;

public record GetPayrollRecordsQuery(
    Guid UserId,
    PaginationDto Pagination,
    string? Search = null,
    Guid? WorkshopId = null,
    Guid? DepartmentId = null,
    int? PersianYear = null,
    int? PersianMonth = null)
    : IRequest<Result<PagedResult<PayrollRecordResult>>>;
