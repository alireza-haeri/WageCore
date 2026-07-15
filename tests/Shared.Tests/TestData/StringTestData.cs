namespace SamarPlanner.Shared.Tests.TestData;

public static class StringTestData
{
    public static TheoryData<string?> NullOrWhiteSpace => new()
    {
        null,
        "",
        "   "
    };

}