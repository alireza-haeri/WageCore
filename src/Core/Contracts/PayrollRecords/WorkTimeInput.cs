namespace Core.Contracts.PayrollRecords;

/// <summary>
/// A user-provided amount of time expressed in hours and minutes.
/// Minutes are converted to hours with minutes / 60.
/// </summary>
public record WorkTimeInput(
    int Hours,
    int Minutes);
