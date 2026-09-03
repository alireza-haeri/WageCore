namespace Core.Contracts.PayrollRecords;

/// <summary>
/// A user-provided amount of time expressed in days, hours and minutes.
/// Days are converted to hours in the application layer using the labor-law
/// daily working hours rule that is effective for the period.
/// </summary>
public record DayTimeInput(
    int Days,
    int Hours,
    int Minutes);
