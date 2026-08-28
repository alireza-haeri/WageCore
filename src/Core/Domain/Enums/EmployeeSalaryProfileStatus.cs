namespace Core.Domain;

/// <summary>
/// Represents the lifecycle state of an employee's salary profile.
/// A profile is Active while it is the latest one for its employee and
/// becomes Expired once a newer profile (with a later EffectiveFrom) exists.
/// </summary>
public enum EmployeeSalaryProfileStatus
{
    Active = 0,
    Expired = 1
}
