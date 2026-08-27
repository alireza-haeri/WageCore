namespace Infrastructure.Persistence.Configurations;

public class CalculationFormulaConfigurations : IEntityTypeConfiguration<CalculationFormula>
{
    public void Configure(EntityTypeBuilder<CalculationFormula> builder)
    {
        builder.ToTable(CalculationFormula.TableName);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Key)
            .IsRequired()
            .IsUnicode(false)
            .HasConversion<string>()
            .HasMaxLength(100);

        builder.Property(x => x.Expression)
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(2000);

        builder.Property(x => x.EffectiveFrom)
            .IsRequired()
            .HasColumnType("date");

        builder.HasIndex(x => x.EffectiveFrom).IsUnique();
    }
}
