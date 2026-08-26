using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class EmployeeConfigurations : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable(Employee.TableName);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Ignore(x => x.IsTerminated);

        builder.HasIndex(x => new { x.WorkshopId, x.Id });
        builder.HasIndex(x => x.DepartmentId);

        builder.Property(x => x.WorkshopId)
            .IsRequired();

        builder.Property(x => x.DepartmentId)
            .IsRequired();

        builder.Property(x => x.PersonalCode)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(20);

        builder.Property(x => x.FullName)
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(100);

        builder.Property(x => x.NationalCode)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(10);

        builder.Property(x => x.BirthCertificateNumber)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(20);

        builder.Property(x => x.FatherName)
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(50);

        builder.Property(x => x.Gender)
            .IsRequired()
            .IsUnicode(false)
            .HasConversion<string>();

        builder.Property(x => x.MaritalStatus)
            .IsRequired()
            .IsUnicode(false)
            .HasConversion<string>();

        builder.Property(x => x.ChildrenCount)
            .IsRequired();

        builder.Property(x => x.HireDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(x => x.TerminationDate)
            .HasColumnType("date");

        builder.Property(x => x.PhoneNumber)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(20);

        builder.Property(x => x.JobTitle)
            .IsUnicode()
            .HasMaxLength(100);

        builder.Property(x => x.IsTaxSubject)
            .IsRequired();

        builder.HasOne<Workshop>()
            .WithMany()
            .HasForeignKey(x => x.WorkshopId)
            .OnDelete(DeleteBehavior.Cascade);

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

            insuranceBuilder.Property(x => x.InsuranceCalculationProfile)
                .HasColumnName(nameof(Insurance.InsuranceCalculationProfile))
                .IsRequired()
                .IsUnicode(false)
                .HasConversion<string>();
        });

        builder.OwnsMany(x => x.BankAccounts, bankAccountBuilder =>
        {
            var employeeIdColumnName = $"{nameof(Employee)}Id";

            bankAccountBuilder.ToTable(BankAccount.TableName);

            bankAccountBuilder.WithOwner()
                .HasForeignKey(employeeIdColumnName);

            bankAccountBuilder.HasKey(x => x.Id);
            bankAccountBuilder.Property(x => x.Id).ValueGeneratedNever();

            bankAccountBuilder.Property<Guid>(employeeIdColumnName)
                .IsRequired();

            bankAccountBuilder.Property(x => x.Title)
                .IsUnicode()
                .HasMaxLength(100);

            bankAccountBuilder.Property(x => x.Iban)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(24);

            bankAccountBuilder.HasIndex(employeeIdColumnName);
        });

        builder.Navigation(x => x.BankAccounts)
            .HasField("_bankAccounts")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
