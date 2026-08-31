namespace Infrastructure.Persistence.Configurations;

public class SalaryDecreeConfigurations : IEntityTypeConfiguration<SalaryDecree>
{
    public void Configure(EntityTypeBuilder<SalaryDecree> builder)
    {
        builder.ToTable(SalaryDecree.TableName);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.HasIndex(x => new { x.EmployeeId, x.EffectiveFrom }).IsUnique();

        builder.Property(x => x.EmployeeId)
            .IsRequired();

        builder.Property(x => x.EffectiveFrom)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(x => x.BaseDailySalary)
            .IsRequired()
            .HasPrecision(18, 0);

        builder.Property(x => x.AttractionAllowance)
            .HasPrecision(18, 0);

        builder.Property(x => x.SupervisionAllowance)
            .HasPrecision(18, 0);

        builder.Property(x => x.ShiftType)
            .IsRequired()
            .IsUnicode(false)
            .HasConversion<string>()
            .HasMaxLength(100);

        builder.Property(x => x.ContractType)
            .IsRequired()
            .IsUnicode(false)
            .HasConversion<string>()
            .HasMaxLength(100);

        builder.Property(x => x.HousingAllowance)
            .HasPrecision(18, 0);

        builder.Property(x => x.FoodAllowance)
            .HasPrecision(18, 0);

        builder.Property(x => x.TransportationAllowanceNet)
            .HasPrecision(18, 0);

        builder.Property(x => x.KaranehAmountNet)
            .HasPrecision(18, 0);

        builder.Property(x => x.MaritalStatus)
            .IsRequired()
            .IsUnicode(false)
            .HasConversion<string>()
            .HasMaxLength(100);

        builder.Property(x => x.ChildrenCount)
            .IsRequired();

        builder.Property(x => x.IsTaxSubject)
            .IsRequired();

        builder.OwnsOne(x => x.Insurance, insuranceBuilder =>
        {
            insuranceBuilder.Property(x => x.InsuranceNumber)
                .HasColumnName(nameof(Insurance.InsuranceNumber))
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(20);

            insuranceBuilder.Property(x => x.SocialSecurityContractRow)
                .HasColumnName(nameof(Insurance.SocialSecurityContractRow))
                .IsUnicode(false)
                .HasMaxLength(20);

            insuranceBuilder.Property(x => x.PositionInInsuranceList)
                .HasColumnName(nameof(Insurance.PositionInInsuranceList))
                .IsRequired()
                .IsUnicode()
                .HasMaxLength(100);

            insuranceBuilder.Property(x => x.IsSubjectTo7PercentInsurance)
                .HasColumnName(nameof(Insurance.IsSubjectTo7PercentInsurance))
                .IsRequired();

            insuranceBuilder.Property(x => x.IsSubjectTo20PercentInsurance)
                .HasColumnName(nameof(Insurance.IsSubjectTo20PercentInsurance))
                .IsRequired();

            insuranceBuilder.Property(x => x.IsSubjectTo3PercentInsurance)
                .HasColumnName(nameof(Insurance.IsSubjectTo3PercentInsurance))
                .IsRequired();

            insuranceBuilder.Property(x => x.IsSubjectTo4PercentInsurance)
                .HasColumnName(nameof(Insurance.IsSubjectTo4PercentInsurance))
                .IsRequired();

            insuranceBuilder.Property(x => x.InsuranceCalculationProfile)
                .HasColumnName(nameof(Insurance.InsuranceCalculationProfile))
                .IsRequired()
                .IsUnicode(false)
                .HasConversion<string>();
        });

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
