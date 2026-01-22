using FluentAssertions;
using TendersData.Application.Tenders.Queries.GetTenderById;

namespace TendersData.Application.Tests.Tenders.Queries.GetTenderById;

public class GetTenderByIdValidatorTests
{
    private readonly GetTenderByIdValidator _validator;

    public GetTenderByIdValidatorTests()
    {
        _validator = new GetTenderByIdValidator();
    }

    [Fact]
    public void Validate_WithValidId_ShouldPass()
    {
        // Arrange
        var query = new GetTenderByIdQuery(1);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WithInvalidId_ShouldFail(int id)
    {
        // Arrange
        var query = new GetTenderByIdQuery(id);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be("Id");
        result.Errors[0].ErrorMessage.Should().Be("Id must be greater than 0");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void Validate_WithPositiveIds_ShouldPass(int id)
    {
        // Arrange
        var query = new GetTenderByIdQuery(id);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
