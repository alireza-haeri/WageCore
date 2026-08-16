namespace Core.Tests.Domain.Users;

public class CreateUserTests
{
    private readonly UserBuilder _builder = new();


    [Fact]
    public void Create_WithValidPhoneNumber_ShouldReturnSuccess()
    {
        var phoneNumber = "09123456789";

        var result = _builder.WithPhoneNumber(phoneNumber).WithEmail(null).CreateResult();

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.Id.Should().NotBeEmpty();
            response.PhoneNumber.Should().Be(phoneNumber);
            response.Email.Should().BeNull(); 
        }
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co")]
    [InlineData("user+tag@domain.com")]
    public void Create_WithValidEmail_ShouldReturnSuccess(string email)
    {
        var result = _builder.WithEmail(email).WithPhoneNumber(null).CreateResult();

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.Id.Should().NotBeEmpty();
            response.Email.Should().Be(email);
            response.PhoneNumber.Should().BeNull();
        }
    }

    [Fact]
    public void Create_WithValidFullName_ShouldReturnSuccess()
    {
        var fullName = "علی رضایی";

        var result = _builder.WithFullName(fullName).CreateResult();

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.Id.Should().NotBeEmpty();
            response.FullName.Should().Be(fullName);
        }
    }

    [Fact]
    public void Create_WithBothPhoneAndEmailProvided_ShouldReturnSuccess()
    {
        var phone = "09123456789";
        var email = "test@example.com";

        var result = _builder
            .WithPhoneNumber(phone)
            .WithEmail(email)
            .CreateResult();

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.PhoneNumber.Should().Be(phone);
            response.Email.Should().Be(email);
        }
    }

    [Fact]
    public void Create_WithAllValidFields_ShouldReturnSuccess()
    {
        var id = Guid.NewGuid();
        var phone = "09123456789";
        var email = "test@example.com";
        var fullName = "علی رضایی";

        var result = _builder
            .WithId(id)
            .WithPhoneNumber(phone)
            .WithEmail(email)
            .WithFullName(fullName)
            .CreateResult();

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.Id.Should().Be(id);
            response.PhoneNumber.Should().Be(phone);
            response.Email.Should().Be(email);
            response.FullName.Should().Be(fullName);
        }
    }

    [Fact]
    public void Create_WithBothPhoneAndEmailNull_ShouldFail()
    {
        var result = _builder
            .WithPhoneNumber(null)
            .WithEmail(null)
            .CreateResult();

        result.ShouldBeFailure("حداقل یکی");
    }

    [Fact]
    public void Create_WithBothPhoneAndEmailEmpty_ShouldFail()
    {
        var result = _builder
            .WithPhoneNumber("")
            .WithEmail("")
            .CreateResult();

        result.ShouldBeFailure("حداقل یکی");
    }

    [Fact]
    public void Create_WithPhoneNumberProvidedAndEmailEmpty_ShouldReturnSuccess()
    {
        var result = _builder
            .WithPhoneNumber("09123456789")
            .WithEmail("")
            .CreateResult();

        var response = result.ShouldBeSuccess();
        response.PhoneNumber.Should().Be("09123456789");
        response.Email.Should().BeNull(); // چون خالی بوده، دامنه باید null ذخیره کنه
    }

    [Fact]
    public void Create_WithEmailProvidedAndPhoneNumberEmpty_ShouldReturnSuccess()
    {
        var result = _builder
            .WithPhoneNumber("")
            .WithEmail("test@example.com")
            .CreateResult();

        var response = result.ShouldBeSuccess();
        response.PhoneNumber.Should().BeNull();
        response.Email.Should().Be("test@example.com");
    }

    [Fact]
    public void Create_WithEmptyGuid_ShouldFail()
    {
        var result = _builder.WithId(Guid.Empty).CreateResult();
        result.ShouldBeFailure("شناسه");
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Create_WithNullOrWhiteSpaceFullName_ShouldFail(string? fullName)
    {
        var result = _builder.WithFullName(fullName).CreateResult();
        result.ShouldBeFailure("خالی");
    }

    [Theory]
    [InlineData("اب")]
    [InlineData("ال")]
    public void Create_WithFullNameLessThan3Characters_ShouldFail(string fullName)
    {
        var result = _builder.WithFullName(fullName).CreateResult();
        result.ShouldBeFailure("3 حرف");
    }

    [Fact]
    public void Create_WithFullNameMoreThan100Characters_ShouldFail()
    {
        var fullName = new string('a', 101);
        var result = _builder.WithFullName(fullName).CreateResult();
        result.ShouldBeFailure("100 حرف");
    }

    [Fact]
    public void Create_WithFullNameExactly100Characters_ShouldReturnSuccess()
    {
        var fullName = new string('a', 100);
        var result = _builder.WithFullName(fullName).CreateResult();
        result.ShouldBeSuccess();
    }

    [Fact]
    public void Create_WithFullNameExactly3Characters_ShouldReturnSuccess()
    {
        var fullName = "abc";
        var result = _builder.WithFullName(fullName).CreateResult();
        result.ShouldBeSuccess();
    }

    [Theory]
    [MemberData(nameof(PhoneNumberTestData.InvalidLengthPhoneNumbers), MemberType = typeof(PhoneNumberTestData))]
    public void Create_WithInvalidLengthPhoneNumber_ShouldFail(string phoneNumber)
    {
        var result = _builder.WithPhoneNumber(phoneNumber).CreateResult();
        result.ShouldBeFailure("رقم");
    }

    [Theory]
    [MemberData(nameof(PhoneNumberTestData.NoneEnglishDigitsOrInvalidCharacters), MemberType = typeof(PhoneNumberTestData))]
    public void Create_WithNonEnglishDigitsOrInvalidCharactersPhoneNumber_ShouldFail(string phoneNumber)
    {
        var result = _builder.WithPhoneNumber(phoneNumber).CreateResult();
        result.ShouldBeFailure("انگلیسی");
    }

    [Theory]
    [InlineData("0912345678")]      // 10 رقم
    [InlineData("091234567890")]    // 12 رقم
    [InlineData("00989123456789")]  // با کد کشور
    [InlineData("+989123456789")]   // با +98
    [InlineData("0912345678a")]     // شامل حرف
    [InlineData("09123456789 ")]    // با فاصله آخر
    [InlineData(" 09123456789")]    // با فاصله اول
    public void Create_WithInvalidPhoneNumberPattern_ShouldFail(string phoneNumber)
    {
        var result = _builder.WithPhoneNumber(phoneNumber).CreateResult();
        result.ShouldBeFailure("شماره تلفن");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("user@")]
    [InlineData("@domain.com")]
    [InlineData("user@domain.")]
    [InlineData("user@domain..com")]
    [InlineData("user name@domain.com")]
    public void Create_WithInvalidEmailFormat_ShouldFail(string email)
    {
        var result = _builder.WithEmail(email).CreateResult();
        result.ShouldBeFailure("فرمت ایمیل");
    }

    [Theory]
    [InlineData("user@domain.com")]
    [InlineData("user.name@domain.co")]
    [InlineData("user+tag@domain.org")]
    [InlineData("user-name@domain.net")]
    public void Create_WithValidEmailFormat_ShouldReturnSuccess(string email)
    {
        var result = _builder.WithEmail(email).CreateResult();
        var response = result.ShouldBeSuccess();
        response.Email.Should().Be(email);
    }
}