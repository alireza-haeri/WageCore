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

        builder.Property(x => x.FatherName)
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(50);

        builder.Property(x => x.Gender)
            .IsRequired()
            .IsUnicode(false)
            .HasConversion<string>();

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

        builder.Property(x => x.Region)
            .IsRequired()
            .IsUnicode(false)
            .HasConversion<string>();

        builder.Property(x => x.LeaveUsedInCurrentYear);

        builder.Property(x => x.NetWorkedDaysBeforeCurrentMonth);

        builder.Property(x => x.CarriedOverLeaveFromPreviousYear);

        builder.HasOne<Workshop>()
            .WithMany()
            .HasForeignKey(x => x.WorkshopId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Department>()
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade);

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

            bankAccountBuilder.Property(x => x.BankName)
                .IsUnicode()
                .HasMaxLength(100);

            bankAccountBuilder.Property(x => x.BranchCode)
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
