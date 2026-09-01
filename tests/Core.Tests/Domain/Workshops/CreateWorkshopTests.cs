namespace Core.Tests.Domain.Workshops;

public class CreateWorkshopTests
{
    private readonly WorkshopBuilder _builder = new();

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        var result = _builder.CreateResult();

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.Id.Should().NotBeEmpty();
            response.UserId.Should().NotBeEmpty();
            response.Name.Should().Be("کارگاه نمونه");
            response.Address.Should().Be("تهران، خیابان نمونه، پلاک ۱۲۳");
            response.RegistrationDate.Should().Be(DateOnly.FromDateTime(DateTime.Now));
            response.NationalId.Should().Be("1234567890");
            response.PostalCode.Should().Be("1234567890");
            response.SocialSecurityNumber.Should().Be("1234567890");
            response.EconomicCode.Should().BeNull();
        }
    }

    [Fact]
    public void Create_WithAllValidFields_ShouldReturnSuccess()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var name = "کارگاه نساجی";
        var address = "اصفهان، خیابان صنعت، پلاک ۵";
        var registrationDate = new DateOnly(1404, 1, 1);
        var nationalId = "9876543210";
        var postalCode = "0987654321";
        var socialSecurityNumber = "12345678901";
        var economicCode = "987654321";

        var result = _builder
            .WithId(id)
            .WithUserId(userId)
            .WithName(name)
            .WithAddress(address)
            .WithRegistrationDate(registrationDate)
            .WithNationalId(nationalId)
            .WithPostalCode(postalCode)
            .WithSocialSecurityNumber(socialSecurityNumber)
            .WithEconomicCode(economicCode)
            .CreateResult();

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.Id.Should().Be(id);
            response.UserId.Should().Be(userId);
            response.Name.Should().Be(name);
            response.Address.Should().Be(address);
            response.RegistrationDate.Should().Be(registrationDate);
            response.NationalId.Should().Be(nationalId);
            response.PostalCode.Should().Be(postalCode);
            response.SocialSecurityNumber.Should().Be(socialSecurityNumber);
            response.EconomicCode.Should().Be(economicCode);
        }
    }

    [Fact]
    public void Create_WithEmptyGuid_ShouldFail()
    {
        var result = _builder.WithId(Guid.Empty).CreateResult();
        result.ShouldBeFailure("شناسه کارگاه");
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldFail()
    {
        var result = _builder.WithUserId(Guid.Empty).CreateResult();
        result.ShouldBeFailure("شناسه کاربر");
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Create_WithNullOrWhiteSpaceName_ShouldFail(string? name)
    {
        var result = _builder.WithName(name!).CreateResult();
        result.ShouldBeFailure("نام کارگاه");
    }

    [Theory]
    [InlineData("ا")]
    [InlineData("آ")]
    public void Create_WithNameLessThan2Characters_ShouldFail(string name)
    {
        var result = _builder.WithName(name).CreateResult();
        result.ShouldBeFailure("نام کارگاه باید بین 2 تا 200 حرف باشد.");
    }

    [Fact]
    public void Create_WithNameMoreThan200Characters_ShouldFail()
    {
        var name = new string('a', 201);
        var result = _builder.WithName(name).CreateResult();
        result.ShouldBeFailure("نام کارگاه باید بین 2 تا 200 حرف باشد.");
    }

    [Fact]
    public void Create_WithNameExactly2Characters_ShouldReturnSuccess()
    {
        var result = _builder.WithName("اب").CreateResult();
        result.ShouldBeSuccess();
    }

    [Fact]
    public void Create_WithNameExactly200Characters_ShouldReturnSuccess()
    {
        var name = new string('a', 200);
        var result = _builder.WithName(name).CreateResult();
        result.ShouldBeSuccess();
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Create_WithNullOrWhiteSpaceAddress_ShouldFail(string? address)
    {
        var result = _builder.WithAddress(address!).CreateResult();
        result.ShouldBeFailure("آدرس کارگاه");
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("اصفهان")]
    public void Create_WithAddressLessThan10Characters_ShouldFail(string address)
    {
        var result = _builder.WithAddress(address).CreateResult();
        result.ShouldBeFailure("آدرس کارگاه باید بین 10 تا 1000 حرف باشد.");
    }

    [Fact]
    public void Create_WithAddressMoreThan1000Characters_ShouldFail()
    {
        var address = new string('a', 1001);
        var result = _builder.WithAddress(address).CreateResult();
        result.ShouldBeFailure("آدرس کارگاه باید بین 10 تا 1000 حرف باشد.");
    }

    [Fact]
    public void Create_WithAddressExactly10Characters_ShouldReturnSuccess()
    {
        var result = _builder.WithAddress("1234567890").CreateResult();
        result.ShouldBeSuccess();
    }

    [Fact]
    public void Create_WithAddressExactly1000Characters_ShouldReturnSuccess()
    {
        var address = new string('a', 1000);
        var result = _builder.WithAddress(address).CreateResult();
        result.ShouldBeSuccess();
    }

    [Fact]
    public void Create_WithNullRegistrationDate_ShouldFail()
    {
        var result = _builder.WithRegistrationDate(null).CreateResult();
        result.ShouldBeFailure("تاریخ ثبت کارگاه");
    }

    [Fact]
    public void Create_WithRegistrationDateInFuture_ShouldFail()
    {
        var futureDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
        var result = _builder.WithRegistrationDate(futureDate).CreateResult();
        result.ShouldBeFailure("تاریخ ثبت کارگاه نباید برای آینده باشد.");
    }

    [Fact]
    public void Create_WithRegistrationDateToday_ShouldReturnSuccess()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var result = _builder.WithRegistrationDate(today).CreateResult();
        var response = result.ShouldBeSuccess();
        response.RegistrationDate.Should().Be(today);
    }

    [Fact]
    public void Create_WithRegistrationDatePast_ShouldReturnSuccess()
    {
        var pastDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
        var result = _builder.WithRegistrationDate(pastDate).CreateResult();
        var response = result.ShouldBeSuccess();
        response.RegistrationDate.Should().Be(pastDate);
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Create_WithNullOrWhiteSpaceNationalId_ShouldFail(string? nationalId)
    {
        var result = _builder.WithNationalId(nationalId!).CreateResult();
        result.ShouldBeFailure("شناسه ملی کارگاه");
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("12345678901")]
    [InlineData("abcdefghij")]
    [InlineData("۱۲۳۴۵۶۷۸۹۰")]
    public void Create_WithInvalidNationalId_ShouldFail(string nationalId)
    {
        var result = _builder.WithNationalId(nationalId).CreateResult();
        result.ShouldBeFailure("شناسه ملی کارگاه باید 10 رقم انگلیسی باشد.");
    }

    [Fact]
    public void Create_WithValidNationalId_ShouldReturnSuccess()
    {
        var result = _builder.WithNationalId("1234567890").CreateResult();
        var response = result.ShouldBeSuccess();
        response.NationalId.Should().Be("1234567890");
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("12345678901")]
    [InlineData("abcdefghij")]
    [InlineData("۱۲۳۴۵۶۷۸۹۰")]
    public void Create_WithInvalidPostalCode_ShouldFail(string postalCode)
    {
        var result = _builder.WithPostalCode(postalCode).CreateResult();
        result.ShouldBeFailure("کد پستی باید 10 رقم انگلیسی باشد.");
    }

    [Fact]
    public void Create_WithValidPostalCode_ShouldReturnSuccess()
    {
        var result = _builder.WithPostalCode("1234567890").CreateResult();
        var response = result.ShouldBeSuccess();
        response.PostalCode.Should().Be("1234567890");
    }

    [Fact]
    public void Create_WithNullPostalCode_ShouldReturnSuccess()
    {
        var result = _builder.WithPostalCode(null).CreateResult();
        var response = result.ShouldBeSuccess();
        response.PostalCode.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyPostalCode_ShouldReturnSuccess()
    {
        var result = _builder.WithPostalCode("").CreateResult();
        var response = result.ShouldBeSuccess();
        response.PostalCode.Should().BeNull();
    }
}