namespace Core.Domain.Enums;

/// <summary>
/// Represents the lifecycle state of an employee's salary decree.
/// A decree is Active while it is the latest one for its employee and
/// becomes Expired once a newer decree (with a later EffectiveFrom) exists.
/// </summary>
public enum SalaryDecreeStatus
{
    Active = 0,
    Expired = 1
}
