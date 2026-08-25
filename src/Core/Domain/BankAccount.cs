namespace Core.Domain;

public class BankAccount
{
    public Guid Id { get; private init; }
    public string? Title { get; private set; }
    public string Iban { get; private set; } = null!;

    public static DomainResult<BankAccount> Create(Guid bankAccountId, EmployeeBankAccountDto? bankAccount)
    {
        if (bankAccountId == Guid.Empty)
            return DomainResult<BankAccount>.Failure("شناسه حساب بانکی نمیتواند خالی باشد.");

        var validationResult = Validate(bankAccount);
        if (!validationResult.IsSuccess)
            return DomainResult<BankAccount>.Failure(validationResult.ErrorMessage!);

        var ibanResult = NormalizeIban(bankAccount!.Iban);
        if (!ibanResult.IsSuccess)
            return DomainResult<BankAccount>.Failure(ibanResult.ErrorMessage!);

        return DomainResult<BankAccount>.Success(new BankAccount
        {
            Id = bankAccountId,
            Title = NormalizeTitle(bankAccount.Title),
            Iban = ibanResult.Response
        });
    }

    public static DomainResult<BankAccount> Create(EmployeeBankAccountDto? bankAccount) =>
        Create(Guid.NewGuid(), bankAccount);

    public DomainResult Update(EmployeeBankAccountDto? bankAccount)
    {
        var validationResult = Validate(bankAccount);
        if (!validationResult.IsSuccess)
            return validationResult;

        var ibanResult = NormalizeIban(bankAccount!.Iban);
        if (!ibanResult.IsSuccess)
            return DomainResult.Failure(ibanResult.ErrorMessage!);

        Title = NormalizeTitle(bankAccount.Title);
        Iban = ibanResult.Response;

        return DomainResult.Success();
    }

    internal static DomainResult<string> NormalizeIban(string iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
            return DomainResult<string>.Failure("شماره شبا نمیتواند خالی باشد.");

        if (!RegexExtensions.ValidIranianIbanRegex().IsMatch(iban))
            return DomainResult<string>.Failure("شماره شبا باید با IR شروع شود و پس از آن 24 رقم انگلیسی داشته باشد.");

        return DomainResult<string>.Success(iban[2..]);
    }

    private static DomainResult Validate(EmployeeBankAccountDto? bankAccount)
    {
        if (bankAccount is null)
            return DomainResult.Failure("اطلاعات حساب بانکی نمیتواند خالی باشد.");

        if (!string.IsNullOrWhiteSpace(bankAccount.Title) && bankAccount.Title.Length > 100)
            return DomainResult.Failure("عنوان حساب بانکی نمیتواند بیشتر از 100 حرف باشد.");

        return DomainResult.Success();
    }

    private static string? NormalizeTitle(string? title) =>
        string.IsNullOrWhiteSpace(title) ? null : title;
}
