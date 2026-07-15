namespace Core.Contracts;

public record IdentityResult(bool Succeeded, IDictionary<string, string[]> Errors);