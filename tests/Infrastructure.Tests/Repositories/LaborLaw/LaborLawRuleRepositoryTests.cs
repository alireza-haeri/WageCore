namespace Infrastructure.Tests.Repositories.LaborLaw;

public class LaborLawRuleRepositoryTests(WageCoreDbContextFixture fixture)
    : IClassFixture<WageCoreDbContextFixture>, IAsyncLifetime
{
    private readonly LaborLawRuleItemBuilder _builder = new();

    public async Task InitializeAsync() => await fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<LaborLawRuleItem> CreateRuleAsync(
        AsyncServiceScope scope,
        decimal value = 71_661_840m,
        DateOnly? effectiveFrom = null)
    {
        var repository = scope.ServiceProvider.GetRequiredService<LaborLawRuleRepository>();
        var rule = _builder
            .WithId(Guid.NewGuid())
            .WithKey(LaborLawRuleKey.MinimumDailySalary)
            .WithValue(value)
            .WithEffectiveFrom(effectiveFrom ?? DateOnly.FromDateTime(DateTime.Now.AddDays(-30)))
            .CreateResult()
            .ShouldBeSuccess();

        var result = await repository.CreateAsync(rule);
        result.Should().Be(rule.Id);

        return rule;
    }

    [Fact]
    public async Task CreateAsync_WithValidRule_ShouldPersistRule()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<LaborLawRuleRepository>();
        var effectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-10));

        var rule = _builder
            .WithId(Guid.NewGuid())
            .WithValue(103_909_680m)
            .WithEffectiveFrom(effectiveFrom)
            .CreateResult()
            .ShouldBeSuccess();

        var result = await repository.CreateAsync(rule);

        result.Should().Be(rule.Id);

        var stored = await repository.GetByIdAsync(rule.Id);
        stored.Should().NotBeNull();
        stored!.Key.Should().Be(LaborLawRuleKey.MinimumDailySalary);
        stored.Value.Should().Be(103_909_680m);
        stored.EffectiveFrom.Should().Be(effectiveFrom);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRuleDoesNotExist_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<LaborLawRuleRepository>();

        var result = await repository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WhenRuleExists_ShouldPersistChanges()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<LaborLawRuleRepository>();
        var rule = await CreateRuleAsync(scope);
        var newEffectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));

        rule.Update(LaborLawRuleKey.MinimumDailySalary, 103_909_680m, newEffectiveFrom).ShouldBeSuccess();

        var updateResult = await repository.UpdateAsync(rule);

        updateResult.Should().BeTrue();

        var stored = await repository.GetByIdAsync(rule.Id);
        stored.Should().NotBeNull();
        stored!.Value.Should().Be(103_909_680m);
        stored.EffectiveFrom.Should().Be(newEffectiveFrom);
    }

    [Fact]
    public async Task DeleteAsync_WhenRuleExists_ShouldDeleteRule()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<LaborLawRuleRepository>();
        var rule = await CreateRuleAsync(scope);

        var deleteResult = await repository.DeleteAsync(rule.Id);

        deleteResult.Should().BeTrue();

        var stored = await repository.GetByIdAsync(rule.Id);
        stored.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenRuleDoesNotExist_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<LaborLawRuleRepository>();

        var deleteResult = await repository.DeleteAsync(Guid.NewGuid());

        deleteResult.Should().BeFalse();
    }
}
