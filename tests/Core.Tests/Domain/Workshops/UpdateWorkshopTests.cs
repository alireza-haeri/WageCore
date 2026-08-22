namespace Core.Tests.Domain.Workshops;

public class UpdateWorkshopTests
{
    private readonly WorkshopBuilder _builder = new();

    [Fact]
    public void Update_WithValidData_ShouldReturnSuccess()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();

        var newName = "کارگاه جدید";
        var newAddress = "شیراز، خیابان جدید، پلاک ۱۰۰";
        var newRegion = WorkshopRegion.LessDeveloped;
        var newRegistrationDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-5));
        var newNationalId = "9876543210";
        var newPostalCode = "0987654321";

        var result = workshop.Update(newName, newAddress, newRegion, newRegistrationDate, newNationalId, newPostalCode);

        result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            workshop.Name.Should().Be(newName);
            workshop.Address.Should().Be(newAddress);
            workshop.Region.Should().Be(newRegion);
            workshop.RegistrationDate.Should().Be(newRegistrationDate);
            workshop.NationalId.Should().Be(newNationalId);
            workshop.PostalCode.Should().Be(newPostalCode);
        }
    }

    [Fact]
    public void Update_WithValidDataAndNullPostalCode_ShouldSetPostalCodeToNull()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();

        var result = workshop.Update(
            "کارگاه جدید",
            "شیراز، خیابان جدید، پلاک ۱۰۰",
            WorkshopRegion.LessDeveloped,
            DateOnly.FromDateTime(DateTime.Now.AddDays(-5)),
            "9876543210",
            null);

        result.ShouldBeSuccess();
        workshop.PostalCode.Should().BeNull();
    }

    [Fact]
    public void Update_WithValidDataAndEmptyPostalCode_ShouldSetPostalCodeToNull()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();

        var result = workshop.Update(
            "کارگاه جدید",
            "شیراز، خیابان جدید، پلاک ۱۰۰",
            WorkshopRegion.LessDeveloped,
            DateOnly.FromDateTime(DateTime.Now.AddDays(-5)),
            "9876543210",
            "");

        result.ShouldBeSuccess();
        workshop.PostalCode.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Update_WithNullOrWhiteSpaceName_ShouldFail(string? name)
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();

        var result = workshop.Update(
            name!,
            "تهران، خیابان نمونه، پلاک ۱۲۳",
            WorkshopRegion.Normal,
            DateOnly.FromDateTime(DateTime.Now),
            "1234567890",
            "1234567890");

        result.ShouldBeFailure("نام کارگاه");
    }

    [Theory]
    [InlineData("ا")]
    [InlineData("آ")]
    public void Update_WithNameLessThan2Characters_ShouldFail(string name)
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();

        var result = workshop.Update(
            name,
            "تهران، خیابان نمونه، پلاک ۱۲۳",
            WorkshopRegion.Normal,
            DateOnly.FromDateTime(DateTime.Now),
            "1234567890",
            "1234567890");

        result.ShouldBeFailure("نام کارگاه باید بین 2 تا 200 حرف باشد.");
    }

    [Fact]
    public void Update_WithNameMoreThan200Characters_ShouldFail()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();
        var name = new string('a', 201);

        var result = workshop.Update(
            name,
            "تهران، خیابان نمونه، پلاک ۱۲۳",
            WorkshopRegion.Normal,
            DateOnly.FromDateTime(DateTime.Now),
            "1234567890",
            "1234567890");

        result.ShouldBeFailure("نام کارگاه باید بین 2 تا 200 حرف باشد.");
    }

    [Fact]
    public void Update_WithNameExactly2Characters_ShouldReturnSuccess()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();

        var result = workshop.Update(
            "اب",
            "تهران، خیابان نمونه، پلاک ۱۲۳",
            WorkshopRegion.Normal,
            DateOnly.FromDateTime(DateTime.Now),
            "1234567890",
            "1234567890");

        result.ShouldBeSuccess();
        workshop.Name.Should().Be("اب");
    }

    [Fact]
    public void Update_WithNameExactly200Characters_ShouldReturnSuccess()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();
        var name = new string('a', 200);

        var result = workshop.Update(
            name,
            "تهران، خیابان نمونه، پلاک ۱۲۳",
            WorkshopRegion.Normal,
            DateOnly.FromDateTime(DateTime.Now),
            "1234567890",
            "1234567890");

        result.ShouldBeSuccess();
        workshop.Name.Should().Be(name);
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Update_WithNullOrWhiteSpaceAddress_ShouldFail(string? address)
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();

        var result = workshop.Update(
            "کارگاه نمونه",
            address!,
            WorkshopRegion.Normal,
            DateOnly.FromDateTime(DateTime.Now),
            "1234567890",
            "1234567890");

        result.ShouldBeFailure("آدرس کارگاه");
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("اصفهان")]
    public void Update_WithAddressLessThan10Characters_ShouldFail(string address)
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();

        var result = workshop.Update(
            "کارگاه نمونه",
            address,
            WorkshopRegion.Normal,
            DateOnly.FromDateTime(DateTime.Now),
            "1234567890",
            "1234567890");

        result.ShouldBeFailure("آدرس کارگاه باید بین 10 تا 1000 حرف باشد.");
    }

    [Fact]
    public void Update_WithAddressMoreThan1000Characters_ShouldFail()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();
        var address = new string('a', 1001);

        var result = workshop.Update(
            "کارگاه نمونه",
            address,
            WorkshopRegion.Normal,
            DateOnly.FromDateTime(DateTime.Now),
            "1234567890",
            "1234567890");

        result.ShouldBeFailure("آدرس کارگاه باید بین 10 تا 1000 حرف باشد.");
    }

    [Fact]
    public void Update_WithAddressExactly10Characters_ShouldReturnSuccess()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();

        var result = workshop.Update(
            "کارگاه نمونه",
            "1234567890",
            WorkshopRegion.Normal,
            DateOnly.FromDateTime(DateTime.Now),
            "1234567890",
            "1234567890");

        result.ShouldBeSuccess();
        workshop.Address.Should().Be("1234567890");
    }

    [Fact]
    public void Update_WithAddressExactly1000Characters_ShouldReturnSuccess()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();
        var address = new string('a', 1000);

        var result = workshop.Update(
            "کارگاه نمونه",
            address,
            WorkshopRegion.Normal,
            DateOnly.FromDateTime(DateTime.Now),
            "1234567890",
            "1234567890");

        result.ShouldBeSuccess();
        workshop.Address.Should().Be(address);
    }

    [Fact]
    public void Update_WithNullRegion_ShouldFail()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();

        var result = workshop.Update(
            "کارگاه نمونه",
            "تهران، خیابان نمونه، پلاک ۱۲۳",
            null,
            DateOnly.FromDateTime(DateTime.Now),
            "1234567890",
            "1234567890");

        result.ShouldBeFailure("منطقه کارگاه");
    }

    [Fact]
    public void Update_WithNormalRegion_ShouldReturnSuccess()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();

        var result = workshop.Update(
            "کارگاه نمونه",
            "تهران، خیابان نمونه، پلاک ۱۲۳",
            WorkshopRegion.Normal,
            DateOnly.FromDateTime(DateTime.Now),
            "1234567890",
            "1234567890");

        result.ShouldBeSuccess();
        workshop.Region.Should().Be(WorkshopRegion.Normal);
    }

    [Fact]
    public void Update_WithLessDevelopedRegion_ShouldReturnSuccess()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();

        var result = workshop.Update(
            "کارگاه نمونه",
            "تهران، خیابان نمونه، پلاک ۱۲۳",
            WorkshopRegion.LessDeveloped,
            DateOnly.FromDateTime(DateTime.Now),
            "1234567890",
            "1234567890");

        result.ShouldBeSuccess();
        workshop.Region.Should().Be(WorkshopRegion.LessDeveloped);
    }

    [Fact]
    public void Update_WithNullRegistrationDate_ShouldFail()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();

        var result = workshop.Update(
            "کارگاه نمونه",
            "تهران، خیابان نمونه، پلاک ۱۲۳",
            WorkshopRegion.Normal,
            null,
            "1234567890",
            "1234567890");

        result.ShouldBeFailure("تاریخ ثبت کارگاه");
    }

    [Fact]
    public void Update_WithRegistrationDateInFuture_ShouldFail()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();
        var futureDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1));

        var result = workshop.Update(
            "کارگاه نمونه",
            "تهران، خیابان نمونه، پلاک ۱۲۳",
            WorkshopRegion.Normal,
            futureDate,
            "1234567890",
            "1234567890");

        result.ShouldBeFailure("تاریخ ثبت کارگاه نباید برای آینده باشد.");
    }

    [Fact]
    public void Update_WithRegistrationDateToday_ShouldReturnSuccess()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();
        var today = DateOnly.FromDateTime(DateTime.Now);

        var result = workshop.Update(
            "کارگاه نمونه",
            "تهران، خیابان نمونه، پلاک ۱۲۳",
            WorkshopRegion.Normal,
            today,
            "1234567890",
            "1234567890");

        result.ShouldBeSuccess();
        workshop.RegistrationDate.Should().Be(today);
    }

    [Fact]
    public void Update_WithRegistrationDatePast_ShouldReturnSuccess()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();
        var pastDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));

        var result = workshop.Update(
            "کارگاه نمونه",
            "تهران، خیابان نمونه، پلاک ۱۲۳",
            WorkshopRegion.Normal,
            pastDate,
            "1234567890",
            "1234567890");

        result.ShouldBeSuccess();
        workshop.RegistrationDate.Should().Be(pastDate);
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Update_WithNullOrWhiteSpaceNationalId_ShouldFail(string? nationalId)
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();

        var result = workshop.Update(
            "کارگاه نمونه",
            "تهران، خیابان نمونه، پلاک ۱۲۳",
            WorkshopRegion.Normal,
            DateOnly.FromDateTime(DateTime.Now),
            nationalId!,
            "1234567890");

        result.ShouldBeFailure("شناسه ملی کارگاه");
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("12345678901")]
    [InlineData("abcdefghij")]
    [InlineData("۱۲۳۴۵۶۷۸۹۰")]
    public void Update_WithInvalidNationalId_ShouldFail(string nationalId)
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();

        var result = workshop.Update(
            "کارگاه نمونه",
            "تهران، خیابان نمونه، پلاک ۱۲۳",
            WorkshopRegion.Normal,
            DateOnly.FromDateTime(DateTime.Now),
            nationalId,
            "1234567890");

        result.ShouldBeFailure("شناسه ملی کارگاه باید 10 رقم انگلیسی باشد.");
    }

    [Fact]
    public void Update_WithValidNationalId_ShouldReturnSuccess()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();
        var newNationalId = "9876543210";

        var result = workshop.Update(
            "کارگاه نمونه",
            "تهران، خیابان نمونه، پلاک ۱۲۳",
            WorkshopRegion.Normal,
            DateOnly.FromDateTime(DateTime.Now),
            newNationalId,
            "1234567890");

        result.ShouldBeSuccess();
        workshop.NationalId.Should().Be(newNationalId);
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("12345678901")]
    [InlineData("abcdefghij")]
    [InlineData("۱۲۳۴۵۶۷۸۹۰")]
    public void Update_WithInvalidPostalCode_ShouldFail(string postalCode)
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();

        var result = workshop.Update(
            "کارگاه نمونه",
            "تهران، خیابان نمونه، پلاک ۱۲۳",
            WorkshopRegion.Normal,
            DateOnly.FromDateTime(DateTime.Now),
            "1234567890",
            postalCode);

        result.ShouldBeFailure("کد پستی باید 10 رقم انگلیسی باشد.");
    }

    [Fact]
    public void Update_WithValidPostalCode_ShouldReturnSuccess()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();
        var newPostalCode = "0987654321";

        var result = workshop.Update(
            "کارگاه نمونه",
            "تهران، خیابان نمونه، پلاک ۱۲۳",
            WorkshopRegion.Normal,
            DateOnly.FromDateTime(DateTime.Now),
            "1234567890",
            newPostalCode);

        result.ShouldBeSuccess();
        workshop.PostalCode.Should().Be(newPostalCode);
    }

    [Fact]
    public void Update_WithNullPostalCode_ShouldSetPostalCodeToNull_AndReturnSuccess()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();

        var result = workshop.Update(
            "کارگاه نمونه",
            "تهران، خیابان نمونه، پلاک ۱۲۳",
            WorkshopRegion.Normal,
            DateOnly.FromDateTime(DateTime.Now),
            "1234567890",
            null);

        result.ShouldBeSuccess();
        workshop.PostalCode.Should().BeNull();
    }

    [Fact]
    public void Update_WithEmptyPostalCode_ShouldSetPostalCodeToNull_AndReturnSuccess()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();

        var result = workshop.Update(
            "کارگاه نمونه",
            "تهران، خیابان نمونه، پلاک ۱۲۳",
            WorkshopRegion.Normal,
            DateOnly.FromDateTime(DateTime.Now),
            "1234567890",
            "");

        result.ShouldBeSuccess();
        workshop.PostalCode.Should().BeNull();
    }
}