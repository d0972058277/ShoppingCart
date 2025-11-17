using Domain.Models;
using Domain.Errors;
using Domain.Events;
using AwesomeAssertions;

namespace Domain.Tests;

/// <summary>
/// 購物車聚合根單元測試。
/// </summary>
public class ShoppingCartTests
{
    #region Create Tests

    [Fact]
    public void Create_ShouldCreateEmptyCart()
    {
        // Act
        var cart = ShoppingCart.Create();

        // Assert
        cart.Items.Count.Should().Be(0);
        cart.TotalPrice.Should().Be(0m);
        cart.TotalQuantity.Should().Be(0);
        cart.IsCheckedOut.Should().BeFalse();
        cart.Id.Should().NotBe(Guid.Empty);
    }

    #endregion

    #region Add Item Tests

    [Fact]
    public void AddItem_WithValidProduct_ShouldSucceed()
    {
        // Arrange
        var cart = ShoppingCart.Create();

        // Act
        var result = cart.AddItem(productId: 101, quantity: 2, unitPrice: 99.99m);

        // Assert
        result.IsSuccess.Should().BeTrue();
        cart.Items.Count.Should().Be(1);
        cart.TotalPrice.Should().Be(199.98m);
        cart.TotalQuantity.Should().Be(2);
    }

    [Fact]
    public void AddItem_WithMultipleProducts_ShouldSucceed()
    {
        // Arrange
        var cart = ShoppingCart.Create();

        // Act
        var result1 = cart.AddItem(productId: 101, quantity: 2, unitPrice: 99.99m);
        var result2 = cart.AddItem(productId: 102, quantity: 5, unitPrice: 49.50m);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        cart.Items.Count.Should().Be(2);
        cart.TotalPrice.Should().Be(447.48m);
        cart.TotalQuantity.Should().Be(7);
    }

    [Fact]
    public void AddItem_WithDuplicateProduct_ShouldFail()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 101, quantity: 2, unitPrice: 99.99m);

        // Act
        var result = cart.AddItem(productId: 101, quantity: 3, unitPrice: 99.99m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.DuplicateProduct);
        cart.Items.Count.Should().Be(1);
    }

    [Fact]
    public void AddItem_WithInvalidProductId_ShouldFail()
    {
        // Arrange
        var cart = ShoppingCart.Create();

        // Act
        var result = cart.AddItem(productId: 0, quantity: 1, unitPrice: 99.99m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.InvalidProductId);
        cart.Items.Count.Should().Be(0);
    }

    [Fact]
    public void AddItem_WithInvalidQuantity_ShouldFail()
    {
        // Arrange
        var cart = ShoppingCart.Create();

        // Act
        var result = cart.AddItem(productId: 101, quantity: 0, unitPrice: 99.99m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.InvalidQuantity);
    }

    [Fact]
    public void AddItem_WithInvalidUnitPrice_ShouldFail()
    {
        // Arrange
        var cart = ShoppingCart.Create();

        // Act
        var result = cart.AddItem(productId: 101, quantity: 1, unitPrice: 0.001m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.InvalidUnitPrice);
    }

    [Fact]
    public void AddItem_WithInvalidUnitPriceDecimalPlaces_ShouldFail()
    {
        // Arrange
        var cart = ShoppingCart.Create();

        // Act
        var result = cart.AddItem(productId: 101, quantity: 1, unitPrice: 12.345m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.InvalidUnitPriceDecimalPlaces);
    }

    [Fact]
    public void AddItem_WithQuantityExceedingItemMax_ShouldFail()
    {
        // Arrange
        var cart = ShoppingCart.Create();

        // Act
        var result = cart.AddItem(productId: 101, quantity: 101, unitPrice: 10.00m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.MaxItemQuantityExceeded);
    }

    [Fact]
    public void AddItem_ExceedingTotalQuantityLimit_ShouldFail()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 101, quantity: 100, unitPrice: 10.00m);
        cart.AddItem(productId: 102, quantity: 100, unitPrice: 10.00m);
        cart.AddItem(productId: 103, quantity: 100, unitPrice: 10.00m);
        cart.AddItem(productId: 104, quantity: 100, unitPrice: 10.00m);
        cart.AddItem(productId: 105, quantity: 100, unitPrice: 10.00m);
        cart.AddItem(productId: 106, quantity: 100, unitPrice: 10.00m);
        cart.AddItem(productId: 107, quantity: 100, unitPrice: 10.00m);
        cart.AddItem(productId: 108, quantity: 100, unitPrice: 10.00m);
        cart.AddItem(productId: 109, quantity: 100, unitPrice: 10.00m);

        // Act
        var result = cart.AddItem(productId: 110, quantity: 100, unitPrice: 10.00m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.MaxTotalQuantityExceeded);
    }

    [Fact]
    public void AddItem_ExceedingMaxItemsCount_ShouldFail()
    {
        // Arrange
        var cart = ShoppingCart.Create();

        // Add 50 items (the maximum)
        for (int i = 1; i <= 50; i++)
        {
            cart.AddItem(productId: i, quantity: 1, unitPrice: 10.00m);
        }

        // Act
        var result = cart.AddItem(productId: 51, quantity: 1, unitPrice: 10.00m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.MaxItemsCountExceeded);
        cart.Items.Count.Should().Be(50);
    }

    [Fact]
    public void AddItem_ExceedingTotalPriceLimit_ShouldFail()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 101, quantity: 1, unitPrice: 500000.00m);

        // Act
        var result = cart.AddItem(productId: 102, quantity: 1, unitPrice: 600000.00m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.MaxTotalPriceExceeded);
    }

    [Fact]
    public void AddItem_AfterCheckout_ShouldFail()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 101, quantity: 2, unitPrice: 99.99m);
        cart.Checkout();

        // Act
        var result = cart.AddItem(productId: 102, quantity: 1, unitPrice: 49.99m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.CartAlreadyCheckedOut);
    }

    [Fact]
    public void AddItem_ShouldRaiseCartItemAddedEvent()
    {
        // Arrange
        var cart = ShoppingCart.Create();

        // Act
        cart.AddItem(productId: 101, quantity: 2, unitPrice: 99.99m);

        // Assert
        var events = cart.GetDomainEvents();
        events.Count.Should().Be(1);
        var addedEvent = events.ElementAt(0) as CartItemAddedDomainEvent;
        addedEvent.Should().NotBeNull();
        addedEvent!.CartId.Should().Be(cart.Id);
        addedEvent.ProductId.Should().Be(101);
        addedEvent.Quantity.Should().Be(2);
        addedEvent.UnitPrice.Should().Be(99.99m);
    }

    #endregion

    #region Change Item Quantity Tests

    [Fact]
    public void ChangeItemQuantity_WithValidQuantity_ShouldSucceed()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 101, quantity: 2, unitPrice: 99.99m);

        // Act
        var result = cart.ChangeItemQuantity(productId: 101, quantity: 5);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var item = cart.Items.First(i => i.ProductId == 101);
        item.Quantity.Should().Be(5);
        cart.TotalPrice.Should().Be(499.95m);
        cart.TotalQuantity.Should().Be(5);
    }

    [Fact]
    public void ChangeItemQuantity_ForNonExistentItem_ShouldFail()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 101, quantity: 2, unitPrice: 99.99m);

        // Act
        var result = cart.ChangeItemQuantity(productId: 999, quantity: 5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.ItemNotFound);
    }

    [Fact]
    public void ChangeItemQuantity_WithZeroQuantity_ShouldFail()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 101, quantity: 2, unitPrice: 99.99m);

        // Act
        var result = cart.ChangeItemQuantity(productId: 101, quantity: 0);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.InvalidQuantity);
    }

    [Fact]
    public void ChangeItemQuantity_ExceedingItemMax_ShouldFail()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 101, quantity: 2, unitPrice: 99.99m);

        // Act
        var result = cart.ChangeItemQuantity(productId: 101, quantity: 101);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.MaxItemQuantityExceeded);
    }

    [Fact]
    public void ChangeItemQuantity_ExceedingTotalQuantity_ShouldFail()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        // 加入 9 個商品，每個 100 件，總共 900 件
        for (int i = 1; i <= 9; i++)
        {
            cart.AddItem(productId: i * 2, quantity: 100, unitPrice: 10.00m); // 使用偶數 ID
        }

        // 再加入一個商品 50 件，總數變成 950
        cart.AddItem(productId: 20, quantity: 50, unitPrice: 10.00m);

        // Act - 嘗試將商品 20 的數量改為 51，總數會變成 951，不會超過
        // 但改為 100 時差異是 50，總數會變成 1000，超過 999
        var result = cart.ChangeItemQuantity(productId: 20, quantity: 100);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.MaxTotalQuantityExceeded);
    }

    [Fact]
    public void ChangeItemQuantity_ExceedingTotalPrice_ShouldFail()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 101, quantity: 1, unitPrice: 50000.00m);

        // Act
        var result = cart.ChangeItemQuantity(productId: 101, quantity: 21);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.MaxTotalPriceExceeded);
    }

    [Fact]
    public void ChangeItemQuantity_AfterCheckout_ShouldFail()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 101, quantity: 2, unitPrice: 99.99m);
        cart.Checkout();

        // Act
        var result = cart.ChangeItemQuantity(productId: 101, quantity: 5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.CartAlreadyCheckedOut);
    }

    [Fact]
    public void ChangeItemQuantity_ShouldRaiseCartItemQuantityChangedEvent()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 101, quantity: 2, unitPrice: 99.99m);
        cart.DrainDomainEvents();

        // Act
        cart.ChangeItemQuantity(productId: 101, quantity: 5);

        // Assert
        var events = cart.GetDomainEvents();
        events.Count.Should().Be(1);
        var changedEvent = events.ElementAt(0) as CartItemQuantityChangedDomainEvent;
        changedEvent.Should().NotBeNull();
        changedEvent!.CartId.Should().Be(cart.Id);
        changedEvent.ProductId.Should().Be(101);
        changedEvent.Quantity.Should().Be(5);
    }

    #endregion

    #region Remove Item Tests

    [Fact]
    public void RemoveItem_WithExistingItem_ShouldSucceed()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 101, quantity: 2, unitPrice: 99.99m);
        cart.AddItem(productId: 102, quantity: 5, unitPrice: 49.50m);

        // Act
        var result = cart.RemoveItem(productId: 101);

        // Assert
        result.IsSuccess.Should().BeTrue();
        cart.Items.Count.Should().Be(1);
        cart.Items.Any(i => i.ProductId == 101).Should().BeFalse();
        cart.TotalPrice.Should().Be(247.50m);
        cart.TotalQuantity.Should().Be(5);
    }

    [Fact]
    public void RemoveItem_WithNonExistentItem_ShouldFail()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 101, quantity: 2, unitPrice: 99.99m);

        // Act
        var result = cart.RemoveItem(productId: 999);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.ItemNotFound);
    }

    [Fact]
    public void RemoveItem_AfterCheckout_ShouldFail()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 101, quantity: 2, unitPrice: 99.99m);
        cart.Checkout();

        // Act
        var result = cart.RemoveItem(productId: 101);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.CartAlreadyCheckedOut);
    }

    [Fact]
    public void RemoveItem_ShouldRaiseCartItemRemovedEvent()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 101, quantity: 2, unitPrice: 99.99m);
        cart.DrainDomainEvents();

        // Act
        cart.RemoveItem(productId: 101);

        // Assert
        var events = cart.GetDomainEvents();
        events.Count.Should().Be(1);
        var removedEvent = events.ElementAt(0) as CartItemRemovedDomainEvent;
        removedEvent.Should().NotBeNull();
        removedEvent!.CartId.Should().Be(cart.Id);
        removedEvent.ProductId.Should().Be(101);
    }

    #endregion

    #region Apply Discount Tests

    [Fact]
    public void ApplyDiscount_WithValidDiscount_ShouldSucceed()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 101, quantity: 2, unitPrice: 100.00m);

        // Act
        var result = cart.ApplyDiscount(productId: 101, discountPercentage: 10.00m);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var item = cart.Items.First(i => i.ProductId == 101);
        item.DiscountPercentage.Should().Be(10.00m);
        item.DiscountedUnitPrice.Should().Be(90.00m);
        cart.TotalPrice.Should().Be(180.00m);
    }

    [Fact]
    public void ApplyDiscount_WithHigherDiscount_ShouldSucceed()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 101, quantity: 2, unitPrice: 100.00m);
        cart.ApplyDiscount(productId: 101, discountPercentage: 10.00m);

        // Act
        var result = cart.ApplyDiscount(productId: 101, discountPercentage: 20.00m);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var item = cart.Items.First(i => i.ProductId == 101);
        item.DiscountPercentage.Should().Be(20.00m);
        cart.TotalPrice.Should().Be(160.00m);
    }

    [Fact]
    public void ApplyDiscount_WithLowerDiscount_ShouldFail()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 101, quantity: 2, unitPrice: 100.00m);
        cart.ApplyDiscount(productId: 101, discountPercentage: 20.00m);

        // Act
        var result = cart.ApplyDiscount(productId: 101, discountPercentage: 10.00m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.DiscountCannotBeReduced);
    }

    [Fact]
    public void ApplyDiscount_WithInvalidPercentage_ShouldFail()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 101, quantity: 2, unitPrice: 100.00m);

        // Act
        var result = cart.ApplyDiscount(productId: 101, discountPercentage: 101.00m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.InvalidDiscountPercentage);
    }

    [Fact]
    public void ApplyDiscount_ForNonExistentItem_ShouldFail()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 101, quantity: 2, unitPrice: 100.00m);

        // Act
        var result = cart.ApplyDiscount(productId: 999, discountPercentage: 10.00m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.ItemNotFound);
    }

    [Fact]
    public void ApplyDiscount_AfterCheckout_ShouldFail()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 101, quantity: 2, unitPrice: 100.00m);
        cart.Checkout();

        // Act
        var result = cart.ApplyDiscount(productId: 101, discountPercentage: 10.00m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.CartAlreadyCheckedOut);
    }

    [Fact]
    public void ApplyDiscount_ShouldRaiseCartItemDiscountAppliedEvent()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 101, quantity: 2, unitPrice: 100.00m);
        cart.DrainDomainEvents();

        // Act
        cart.ApplyDiscount(productId: 101, discountPercentage: 10.00m);

        // Assert
        var events = cart.GetDomainEvents();
        events.Count.Should().Be(1);
        var discountEvent = events.ElementAt(0) as CartItemDiscountAppliedDomainEvent;
        discountEvent.Should().NotBeNull();
        discountEvent!.CartId.Should().Be(cart.Id);
        discountEvent.ProductId.Should().Be(101);
        discountEvent.DiscountPercentage.Should().Be(10.00m);
    }

    #endregion

    #region Checkout Tests

    [Fact]
    public void Checkout_WithValidCart_ShouldSucceed()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 102, quantity: 2, unitPrice: 99.99m);

        // Act
        var result = cart.Checkout();

        // Assert
        result.IsSuccess.Should().BeTrue();
        cart.IsCheckedOut.Should().BeTrue();
    }

    [Fact]
    public void Checkout_WithEmptyCart_ShouldFail()
    {
        // Arrange
        var cart = ShoppingCart.Create();

        // Act
        var result = cart.Checkout();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.EmptyCart);
    }

    [Fact]
    public void Checkout_AlreadyCheckedOut_ShouldFail()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 102, quantity: 2, unitPrice: 99.99m);
        cart.Checkout();

        // Act
        var result = cart.Checkout();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.CartAlreadyCheckedOut);
    }

    [Fact]
    public void Checkout_ShouldRaiseCartCheckedOutEvent()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 102, quantity: 2, unitPrice: 99.99m);
        cart.DrainDomainEvents();

        // Act
        cart.Checkout();

        // Assert
        var events = cart.GetDomainEvents();
        events.Count.Should().Be(1);
        var checkoutEvent = events.ElementAt(0) as CartCheckedOutDomainEvent;
        checkoutEvent.Should().NotBeNull();
        checkoutEvent!.CartId.Should().Be(cart.Id);
        checkoutEvent.TotalPrice.Should().Be(199.98m);
        checkoutEvent.ItemCount.Should().Be(1);
    }

    #endregion

    #region Clear Tests

    [Fact]
    public void Clear_WithItems_ShouldSucceed()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 101, quantity: 2, unitPrice: 99.99m);
        cart.AddItem(productId: 102, quantity: 5, unitPrice: 49.50m);

        // Act
        var result = cart.Clear();

        // Assert
        result.IsSuccess.Should().BeTrue();
        cart.Items.Count.Should().Be(0);
        cart.TotalPrice.Should().Be(0m);
        cart.TotalQuantity.Should().Be(0);
    }

    [Fact]
    public void Clear_AfterCheckout_ShouldFail()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 102, quantity: 2, unitPrice: 99.99m);
        cart.Checkout();

        // Act
        var result = cart.Clear();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Errors.CartAlreadyCheckedOut);
    }

    [Fact]
    public void Clear_ShouldRaiseCartClearedEvent()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 101, quantity: 2, unitPrice: 99.99m);
        cart.DrainDomainEvents();

        // Act
        cart.Clear();

        // Assert
        var events = cart.GetDomainEvents();
        events.Count.Should().Be(1);
        var clearEvent = events.ElementAt(0) as CartClearedDomainEvent;
        clearEvent.Should().NotBeNull();
        clearEvent!.CartId.Should().Be(cart.Id);
    }

    #endregion

    #region Complex Scenario Tests

    [Fact]
    public void ComplexScenario_AddItemsApplyDiscountAndCheckout_ShouldWorkCorrectly()
    {
        // Arrange
        var cart = ShoppingCart.Create();

        // Act & Assert - Add items
        cart.AddItem(productId: 102, quantity: 2, unitPrice: 99.99m).IsSuccess.Should().BeTrue();
        cart.AddItem(productId: 104, quantity: 5, unitPrice: 49.50m).IsSuccess.Should().BeTrue();

        // Apply discounts
        cart.ApplyDiscount(productId: 102, discountPercentage: 10.00m).IsSuccess.Should().BeTrue();
        cart.ApplyDiscount(productId: 104, discountPercentage: 20.00m).IsSuccess.Should().BeTrue();

        // Change quantity
        cart.ChangeItemQuantity(productId: 102, quantity: 3).IsSuccess.Should().BeTrue();

        // Verify totals
        var expectedTotal = (99.99m * 0.9m * 3) + (49.50m * 0.8m * 5);
        cart.TotalPrice.Should().Be(expectedTotal);
        cart.TotalQuantity.Should().Be(8);

        // Checkout
        cart.Checkout().IsSuccess.Should().BeTrue();
        cart.IsCheckedOut.Should().BeTrue();

        // Verify cannot modify after checkout
        cart.AddItem(productId: 106, quantity: 1, unitPrice: 10.00m).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ComplexScenario_MultipleOperationsGenerateCorrectEvents()
    {
        // Arrange
        var cart = ShoppingCart.Create();

        // Act
        cart.AddItem(productId: 102, quantity: 2, unitPrice: 99.99m);
        cart.AddItem(productId: 104, quantity: 5, unitPrice: 49.50m);
        cart.ApplyDiscount(productId: 102, discountPercentage: 10.00m);
        cart.ChangeItemQuantity(productId: 104, quantity: 3);
        cart.RemoveItem(productId: 102);
        cart.Checkout();

        // Assert
        var events = cart.GetDomainEvents();
        events.Count.Should().Be(6);
        events.ElementAt(0).Should().BeOfType<CartItemAddedDomainEvent>();
        events.ElementAt(1).Should().BeOfType<CartItemAddedDomainEvent>();
        events.ElementAt(2).Should().BeOfType<CartItemDiscountAppliedDomainEvent>();
        events.ElementAt(3).Should().BeOfType<CartItemQuantityChangedDomainEvent>();
        events.ElementAt(4).Should().BeOfType<CartItemRemovedDomainEvent>();
        events.ElementAt(5).Should().BeOfType<CartCheckedOutDomainEvent>();
    }

    [Fact]
    public void ComplexScenario_EdgeCasesForLimits_ShouldEnforceConstraints()
    {
        // Arrange
        var cart = ShoppingCart.Create();

        // Act & Assert - Test maximum item quantity (100)
        cart.AddItem(productId: 102, quantity: 100, unitPrice: 10.00m).IsSuccess.Should().BeTrue();
        cart.ChangeItemQuantity(productId: 102, quantity: 101).IsFailure.Should().BeTrue();

        // Test total quantity limit (999)
        cart.AddItem(productId: 104, quantity: 100, unitPrice: 10.00m).IsSuccess.Should().BeTrue();
        cart.AddItem(productId: 106, quantity: 100, unitPrice: 10.00m).IsSuccess.Should().BeTrue();
        cart.AddItem(productId: 108, quantity: 100, unitPrice: 10.00m).IsSuccess.Should().BeTrue();
        cart.AddItem(productId: 110, quantity: 100, unitPrice: 10.00m).IsSuccess.Should().BeTrue();
        cart.AddItem(productId: 112, quantity: 100, unitPrice: 10.00m).IsSuccess.Should().BeTrue();
        cart.AddItem(productId: 114, quantity: 100, unitPrice: 10.00m).IsSuccess.Should().BeTrue();
        cart.AddItem(productId: 116, quantity: 100, unitPrice: 10.00m).IsSuccess.Should().BeTrue();
        cart.AddItem(productId: 118, quantity: 100, unitPrice: 10.00m).IsSuccess.Should().BeTrue();

        cart.TotalQuantity.Should().Be(900);
        cart.AddItem(productId: 120, quantity: 100, unitPrice: 10.00m).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ComplexScenario_ClearAndReuse_ShouldWorkCorrectly()
    {
        // Arrange
        var cart = ShoppingCart.Create();
        cart.AddItem(productId: 102, quantity: 2, unitPrice: 99.99m);
        cart.AddItem(productId: 104, quantity: 5, unitPrice: 49.50m);

        // Act
        cart.Clear();

        // Assert - Cart should be empty
        cart.Items.Count.Should().Be(0);
        cart.TotalPrice.Should().Be(0m);
        cart.TotalQuantity.Should().Be(0);

        // Can add items again after clearing
        cart.AddItem(productId: 106, quantity: 3, unitPrice: 25.00m).IsSuccess.Should().BeTrue();
        cart.Items.Count.Should().Be(1);
        cart.TotalPrice.Should().Be(75.00m);
    }

    #endregion
}
