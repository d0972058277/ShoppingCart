using Domain.Models;
using Domain.Errors;
using AwesomeAssertions;

namespace Domain.Tests;

/// <summary>
/// 購物車項目單元測試。
/// </summary>
public class CartItemTests
{
    #region Create Tests

    [Fact]
    public void DecideCreate_WithValidParameters_ShouldSucceed()
    {
        // Arrange & Act
        var result = CartItem.DecideCreate(productId: 101, quantity: 2, unitPrice: 99.99m);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void DecideCreate_WithInvalidProductId_ShouldFail()
    {
        // Arrange & Act
        var result = CartItem.DecideCreate(productId: 0, quantity: 2, unitPrice: 99.99m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.InvalidProductId);
    }

    [Fact]
    public void DecideCreate_WithNegativeProductId_ShouldFail()
    {
        // Arrange & Act
        var result = CartItem.DecideCreate(productId: -1, quantity: 2, unitPrice: 99.99m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.InvalidProductId);
    }

    [Fact]
    public void DecideCreate_WithZeroQuantity_ShouldFail()
    {
        // Arrange & Act
        var result = CartItem.DecideCreate(productId: 101, quantity: 0, unitPrice: 99.99m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.InvalidQuantity);
    }

    [Fact]
    public void DecideCreate_WithNegativeQuantity_ShouldFail()
    {
        // Arrange & Act
        var result = CartItem.DecideCreate(productId: 101, quantity: -1, unitPrice: 99.99m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.InvalidQuantity);
    }

    [Fact]
    public void DecideCreate_WithQuantityExceedingMax_ShouldFail()
    {
        // Arrange & Act
        var result = CartItem.DecideCreate(productId: 101, quantity: 101, unitPrice: 99.99m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.MaxItemQuantityExceeded);
    }

    [Fact]
    public void DecideCreate_WithMaxQuantity_ShouldSucceed()
    {
        // Arrange & Act
        var result = CartItem.DecideCreate(productId: 101, quantity: 100, unitPrice: 99.99m);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void DecideCreate_WithUnitPriceTooLow_ShouldFail()
    {
        // Arrange & Act
        var result = CartItem.DecideCreate(productId: 101, quantity: 1, unitPrice: 0.001m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.InvalidUnitPrice);
    }

    [Fact]
    public void DecideCreate_WithMinUnitPrice_ShouldSucceed()
    {
        // Arrange & Act
        var result = CartItem.DecideCreate(productId: 101, quantity: 1, unitPrice: 0.01m);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void DecideCreate_WithUnitPriceTooHigh_ShouldFail()
    {
        // Arrange & Act
        var result = CartItem.DecideCreate(productId: 101, quantity: 1, unitPrice: 1_000_000.00m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.MaxUnitPriceExceeded);
    }

    [Fact]
    public void DecideCreate_WithInvalidDecimalPlaces_ShouldFail()
    {
        // Arrange & Act
        var result = CartItem.DecideCreate(productId: 101, quantity: 1, unitPrice: 12.345m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.InvalidUnitPriceDecimalPlaces);
    }

    [Fact]
    public void ApplyCreate_ShouldCreateItemWithCorrectProperties()
    {
        // Arrange & Act
        var item = CartItem.ApplyCreate(productId: 101, quantity: 5, unitPrice: 99.99m);

        // Assert
        item.ProductId.Should().Be(101);
        item.Quantity.Should().Be(5);
        item.UnitPrice.Should().Be(99.99m);
        item.DiscountPercentage.Should().Be(0m);
        item.DiscountedUnitPrice.Should().Be(99.99m);
        item.TotalPrice.Should().Be(499.95m);
        item.OriginalTotalPrice.Should().Be(499.95m);
    }

    #endregion

    #region Change Quantity Tests

    [Fact]
    public void DecideChangeQuantity_WithValidQuantity_ShouldSucceed()
    {
        // Arrange
        var item = CartItem.ApplyCreate(productId: 101, quantity: 5, unitPrice: 99.99m);

        // Act
        var result = item.DecideChangeQuantity(10);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void DecideChangeQuantity_WithZeroQuantity_ShouldFail()
    {
        // Arrange
        var item = CartItem.ApplyCreate(productId: 101, quantity: 5, unitPrice: 99.99m);

        // Act
        var result = item.DecideChangeQuantity(0);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.InvalidQuantity);
    }

    [Fact]
    public void DecideChangeQuantity_WithQuantityExceedingMax_ShouldFail()
    {
        // Arrange
        var item = CartItem.ApplyCreate(productId: 101, quantity: 5, unitPrice: 99.99m);

        // Act
        var result = item.DecideChangeQuantity(101);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.MaxItemQuantityExceeded);
    }

    [Fact]
    public void ApplyChangeQuantity_ShouldUpdateQuantityAndTotalPrice()
    {
        // Arrange
        var item = CartItem.ApplyCreate(productId: 101, quantity: 5, unitPrice: 99.99m);

        // Act
        item.ApplyChangeQuantity(10);

        // Assert
        item.Quantity.Should().Be(10);
        item.TotalPrice.Should().Be(999.90m);
    }

    #endregion

    #region Discount Tests

    [Fact]
    public void DecideApplyDiscount_WithValidDiscount_ShouldSucceed()
    {
        // Arrange
        var item = CartItem.ApplyCreate(productId: 101, quantity: 5, unitPrice: 100.00m);

        // Act
        var result = item.DecideApplyDiscount(10.00m);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void DecideApplyDiscount_WithNegativeDiscount_ShouldFail()
    {
        // Arrange
        var item = CartItem.ApplyCreate(productId: 101, quantity: 5, unitPrice: 100.00m);

        // Act
        var result = item.DecideApplyDiscount(-1.00m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.InvalidDiscountPercentage);
    }

    [Fact]
    public void DecideApplyDiscount_WithDiscountOver100_ShouldFail()
    {
        // Arrange
        var item = CartItem.ApplyCreate(productId: 101, quantity: 5, unitPrice: 100.00m);

        // Act
        var result = item.DecideApplyDiscount(101.00m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.InvalidDiscountPercentage);
    }

    [Fact]
    public void DecideApplyDiscount_WithInvalidDecimalPlaces_ShouldFail()
    {
        // Arrange
        var item = CartItem.ApplyCreate(productId: 101, quantity: 5, unitPrice: 100.00m);

        // Act
        var result = item.DecideApplyDiscount(10.123m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.InvalidDiscountDecimalPlaces);
    }

    [Fact]
    public void DecideApplyDiscount_WithHigherDiscount_ShouldSucceed()
    {
        // Arrange
        var item = CartItem.ApplyCreate(productId: 101, quantity: 5, unitPrice: 100.00m);
        item.ApplyDiscountChange(10.00m);

        // Act
        var result = item.DecideApplyDiscount(20.00m);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void DecideApplyDiscount_WithLowerDiscount_ShouldFail()
    {
        // Arrange
        var item = CartItem.ApplyCreate(productId: 101, quantity: 5, unitPrice: 100.00m);
        item.ApplyDiscountChange(20.00m);

        // Act
        var result = item.DecideApplyDiscount(10.00m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.DiscountCannotBeReduced);
    }

    [Fact]
    public void ApplyDiscountChange_ShouldUpdateDiscountAndPrices()
    {
        // Arrange
        var item = CartItem.ApplyCreate(productId: 101, quantity: 5, unitPrice: 100.00m);

        // Act
        item.ApplyDiscountChange(20.00m);

        // Assert
        item.DiscountPercentage.Should().Be(20.00m);
        item.DiscountedUnitPrice.Should().Be(80.00m);
        item.TotalPrice.Should().Be(400.00m);
        item.OriginalTotalPrice.Should().Be(500.00m);
    }

    #endregion
}
