namespace Infrastructure.Persistence;

public class WageCoreDbContext(DbContextOptions<WageCoreDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public override DbSet<ApplicationUser> Users { get; set; }
    public DbSet<Workshop> Workshops { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<LaborLawRuleItem> LaborLawRuleItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}