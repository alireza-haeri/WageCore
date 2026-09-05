using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PayrollRecordConfigurations : IEntityTypeConfiguration<PayrollRecord>
{
    public void Configure(EntityTypeBuilder<PayrollRecord> builder)
    {
        builder.ToTable(PayrollRecord.TableName);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Ignore(x => x.IsPaid);

        builder.HasIndex(x => new { x.EmployeeId, x.PeriodStart, x.PeriodEnd })
            .IsUnique();

        builder.Property(x => x.EmployeeId)
            .IsRequired();

        builder.Property(x => x.PeriodStart)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(x => x.PeriodEnd)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(x => x.WorkedDaysCount)
            .IsRequired();

        builder.Property(x => x.OvertimeHours)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.NightShiftHours)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.FridayWorkHours)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.LeaveHours)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.HolidaysCount)
            .IsRequired();

        builder.Property(x => x.MissionDaysCount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.MissionHours)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.HolidayWorkHours)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.MissionAmountOverride)
            .HasPrecision(18, 2);

        builder.Property(x => x.StandardWorkingDaysCount)
            .IsRequired();

        builder.Property(x => x.IsEsfandPeriod)
            .IsRequired();

        builder.Property(x => x.AnnualBonusType)
            .IsUnicode(false)
            .HasConversion<string>()
            .HasMaxLength(100);

        builder.Property(x => x.PerformanceBonusAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.CashBenefitsAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.OvertimeAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.NightShiftExtraAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.FridayWorkAllowance)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.BaseSalaryAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.AttractionAllowanceAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.SupervisionAllowanceAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.HolidayWorkAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.ChildAllowanceAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.HousingAllowanceAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.FoodAllowanceAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.MarriageAllowanceAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.ShiftWorkAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.DailyMissionAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.EndOfServiceAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.AnnualBonusAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.CommutingAllowanceAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.MaxMonthlyOvertimeHours)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.MaxFridayHours)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.MaxNightShiftHours)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.DailyWorkingHours)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.CalculatedTaxAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.GrossAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.InsuranceAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.TotalDeductionsAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.NetPayableAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.Status)
            .IsRequired()
            .IsUnicode(false)
            .HasConversion<string>()
            .HasMaxLength(100);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
