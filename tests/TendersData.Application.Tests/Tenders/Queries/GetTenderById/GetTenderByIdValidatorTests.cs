using FluentAssertions;
using TendersData.Tests.Common.Builders;

namespace TendersData.Application.Tests.Tenders.Queries.GetTenderById;

public class GetTenderByIdValidatorTests : GetTenderByIdValidatorMockHelper
{
    [Fact]
    public void Validate_WithValidId_ShouldPass()
    {
        // Arrange
        var query = GetTenderByIdQueryBuilder.Default.WithId(1).Build();

        // Act
        var result = Validator.Validate(query);

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
        var query = GetTenderByIdQueryBuilder.Default.WithId(id).Build();

        // Act
        var result = Validator.Validate(query);

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
        var query = GetTenderByIdQueryBuilder.Default.WithId(id).Build();

        // Act
        var result = Validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
