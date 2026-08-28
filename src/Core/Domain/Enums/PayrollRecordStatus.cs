namespace Core.Domain.Enums;

/// <summary>
/// Represents the lifecycle state of a payroll record.
/// A record is editable and deletable while it is Draft, and becomes final once it is Paid.
/// </summary>
public enum PayrollRecordStatus
{
    Draft = 0,
    Paid = 1
}
