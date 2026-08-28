namespace Infrastructure.Persistence.Configurations;

public class EmployeeSalaryProfileConfigurations : IEntityTypeConfiguration<EmployeeSalaryProfile>
{
    public void Configure(EntityTypeBuilder<EmployeeSalaryProfile> builder)
    {
        builder.ToTable(EmployeeSalaryProfile.TableName);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.HasIndex(x => new { x.EmployeeId, x.EffectiveFrom }).IsUnique();

        builder.Property(x => x.EmployeeId)
            .IsRequired();

        builder.Property(x => x.EffectiveFrom)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(x => x.BaseMonthlySalary)
            .IsRequired()
            .HasPrecision(18, 0);

        builder.Property(x => x.AttractionAllowance)
            .HasPrecision(18, 0);

        builder.Property(x => x.SupervisionAllowance)
            .HasPrecision(18, 0);

        builder.Property(x => x.SeniorityBaseApplicationMode)
            .IsRequired()
            .IsUnicode(false)
            .HasConversion<string>()
            .HasMaxLength(100);

        builder.Property(x => x.SeniorityBaseCalculationMethod)
            .IsUnicode(false)
            .HasConversion<string>()
            .HasMaxLength(100);

        builder.Property(x => x.YearEndSeniorityMode)
            .IsRequired()
            .IsUnicode(false)
            .HasConversion<string>()
            .HasMaxLength(100);

        builder.Property(x => x.ShiftType)
            .IsRequired()
            .IsUnicode(false)
            .HasConversion<string>()
            .HasMaxLength(100);

        builder.Property(x => x.HousingAllowance)
            .HasPrecision(18, 0);

        builder.Property(x => x.FoodAllowance)
            .HasPrecision(18, 0);

        builder.Property(x => x.ChildAllowancePerChild)
            .HasPrecision(18, 0);

        builder.Property(x => x.TransportationAllowanceNet)
            .HasPrecision(18, 0);

        builder.Property(x => x.KaranehAmountNet)
            .HasPrecision(18, 0);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
