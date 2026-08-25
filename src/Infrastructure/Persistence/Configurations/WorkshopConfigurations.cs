using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class WorkshopConfigurations : IEntityTypeConfiguration<Workshop>
{
    public void Configure(EntityTypeBuilder<Workshop> builder)
    {
        builder.ToTable(Workshop.TableName);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        
        builder.HasIndex(x => new { x.UserId, x.Id });
        
        builder.Property(x => x.Name)
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(200);
        
        builder.Property(x=>x.Address)
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(1000);

        builder.Property(x => x.Region)
            .IsRequired()
            .IsUnicode(false)
            .HasConversion<string>();

        builder.Property(x => x.RegistrationDate)
            .IsRequired()
            .HasColumnType("date");
        
        builder.Property(x=>x.NationalId)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(20);
        
        builder.Property(x => x.PostalCode)
            .IsUnicode(false)
            .HasMaxLength(20);

        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsMany(x => x.Departments, departmentBuilder =>
        {
            departmentBuilder.ToTable(Department.TableName);

            departmentBuilder.WithOwner()
                .HasForeignKey(x => x.WorkshopId);

            departmentBuilder.HasKey(x => x.Id);
            departmentBuilder.Property(x => x.Id).ValueGeneratedNever();

            departmentBuilder.Property(x => x.WorkshopId)
                .IsRequired();

            departmentBuilder.Property(x => x.Name)
                .IsRequired()
                .IsUnicode()
                .HasMaxLength(100);

            departmentBuilder.HasIndex(x => x.WorkshopId);
        });

        builder.Navigation(x => x.Departments)
            .HasField("_departments")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
