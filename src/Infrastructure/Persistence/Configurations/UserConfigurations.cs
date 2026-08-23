using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserConfigurations : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable(User.TableName);
        
        builder.Property(x=>x.FullName)
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(100);
    }
}