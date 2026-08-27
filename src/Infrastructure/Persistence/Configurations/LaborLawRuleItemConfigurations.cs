namespace Infrastructure.Persistence.Configurations;

public class LaborLawRuleItemConfigurations : IEntityTypeConfiguration<LaborLawRuleItem>
{
    public void Configure(EntityTypeBuilder<LaborLawRuleItem> builder)
    {
        builder.ToTable(LaborLawRuleItem.TableName);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Key)
            .IsRequired()
            .IsUnicode(false)
            .HasConversion<string>()
            .HasMaxLength(100);

        builder.Property(x => x.Value)
            .IsRequired()
            .HasPrecision(18, 0);

        builder.Property(x => x.EffectiveFrom)
            .IsRequired()
            .HasColumnType("date");

        builder.HasIndex(x => x.EffectiveFrom).IsUnique();
    }
}
