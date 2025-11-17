using CSharpFunctionalExtensions;
using Domain.Primitives;
using Domain.Errors;
using Domain.Events;

namespace Domain.Models;

/// <summary>
/// 購物車聚合根。
/// </summary>
public class ShoppingCart : EventSourcedAggregateRoot<Guid>
{
    private const int MaxItemsCount = 50;
    private const int MaxTotalQuantity = 999;
    private const decimal MaxTotalPrice = 1_000_000m;

    private readonly List<CartItem> _items = new();
    private decimal _totalPrice;
    private bool _isCheckedOut;

    /// <summary>
    /// 取得購物車項目的唯讀集合。
    /// </summary>
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    /// <summary>
    /// 取得購物車總金額。
    /// </summary>
    public decimal TotalPrice => _totalPrice;

    /// <summary>
    /// 取得購物車是否已結帳。
    /// </summary>
    public bool IsCheckedOut => _isCheckedOut;

    /// <summary>
    /// 取得購物車總件數。
    /// </summary>
    public int TotalQuantity => _items.Sum(i => i.Quantity);

    /// <summary>
    /// 私有建構函式供內部使用。
    /// </summary>
    private ShoppingCart(Guid id) : base(id)
    {
        _totalPrice = 0;
        _isCheckedOut = false;
    }

    /// <summary>
    /// 建立新的購物車。
    /// </summary>
    public static ShoppingCart Create()
    {
        return new ShoppingCart(Guid.NewGuid());
    }

    /// <summary>
    /// 加入商品到購物車。
    /// </summary>
    public UnitResult<Error> AddItem(int productId, int quantity, decimal unitPrice)
    {
        return ValidateNotCheckedOut()
            .Bind(() => ValidateNotDuplicateProduct(productId))
            .Bind(() => ValidateMaxItemsCount())
            .Bind(() => CartItem.DecideCreate(productId, quantity, unitPrice))
            .Bind(() => ValidateTotalQuantity(quantity))
            .Bind(() => ValidateTotalPriceForAdd(quantity, unitPrice))
            .Tap(() => RaiseEvent(
                new CartItemAddedDomainEvent(
                    CartId: Id,
                    ProductId: productId,
                    Quantity: quantity,
                    UnitPrice: unitPrice
                )
            ));
    }

    /// <summary>
    /// 變更購物車項目的數量。
    /// </summary>
    public UnitResult<Error> ChangeItemQuantity(int productId, int quantity)
    {
        return ValidateNotCheckedOut()
            .Bind(() => FindItem(productId))
            .Bind(item => item.DecideChangeQuantity(quantity)
                .Bind(() => ValidateTotalQuantity(quantity - item.Quantity))
                .Bind(() => ValidateTotalPriceForChange(item, quantity))
            )
            .Tap(() => RaiseEvent(
                new CartItemQuantityChangedDomainEvent(
                    CartId: Id,
                    ProductId: productId,
                    Quantity: quantity
                )
            ));
    }

    /// <summary>
    /// 移除購物車項目。
    /// </summary>
    public UnitResult<Error> RemoveItem(int productId)
    {
        return ValidateNotCheckedOut()
            .Bind(() => FindItem(productId))
            .Tap(_ => RaiseEvent(
                new CartItemRemovedDomainEvent(
                    CartId: Id,
                    ProductId: productId
                )
            ));
    }

    /// <summary>
    /// 套用折扣到指定商品。
    /// </summary>
    public UnitResult<Error> ApplyDiscount(int productId, decimal discountPercentage)
    {
        return ValidateNotCheckedOut()
            .Bind(() => FindItem(productId))
            .Bind(item => item.DecideApplyDiscount(discountPercentage))
            .Tap(() => RaiseEvent(
                new CartItemDiscountAppliedDomainEvent(
                    CartId: Id,
                    ProductId: productId,
                    DiscountPercentage: discountPercentage
                )
            ));
    }

    /// <summary>
    /// 結帳購物車。
    /// </summary>
    public UnitResult<Error> Checkout()
    {
        return ValidateNotCheckedOut()
            .Bind(() => ValidateNotEmpty())
            .Tap(() => RaiseEvent(
                new CartCheckedOutDomainEvent(
                    CartId: Id,
                    TotalPrice: _totalPrice,
                    ItemCount: _items.Count
                )
            ));
    }

    /// <summary>
    /// 清空購物車。
    /// </summary>
    public UnitResult<Error> Clear()
    {
        return ValidateNotCheckedOut()
            .Tap(() => RaiseEvent(
                new CartClearedDomainEvent(CartId: Id)
            ));
    }

    /// <summary>
    /// 應用事件修改狀態（只修改狀態，不做驗證、不做計算）。
    /// </summary>
    protected override void Apply(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case CartItemAddedDomainEvent e:
                Apply(e);
                break;
            case CartItemQuantityChangedDomainEvent e:
                Apply(e);
                break;
            case CartItemRemovedDomainEvent e:
                Apply(e);
                break;
            case CartItemDiscountAppliedDomainEvent e:
                Apply(e);
                break;
            case CartCheckedOutDomainEvent e:
                Apply(e);
                break;
            case CartClearedDomainEvent e:
                Apply(e);
                break;
        }
    }

    private void Apply(CartItemAddedDomainEvent e)
    {
        var newItem = CartItem.ApplyCreate(e.ProductId, e.Quantity, e.UnitPrice);
        _items.Add(newItem);
        _totalPrice += newItem.TotalPrice;
    }

    private void Apply(CartItemQuantityChangedDomainEvent e)
    {
        var item = GetItemByProductId(e.ProductId);
        var oldTotalPrice = item.TotalPrice;
        item.ApplyChangeQuantity(e.Quantity);
        _totalPrice = _totalPrice - oldTotalPrice + item.TotalPrice;
    }

    private void Apply(CartItemRemovedDomainEvent e)
    {
        var item = GetItemByProductId(e.ProductId);
        _items.Remove(item);
        _totalPrice -= item.TotalPrice;
    }

    private void Apply(CartItemDiscountAppliedDomainEvent e)
    {
        var item = GetItemByProductId(e.ProductId);
        var oldPrice = item.TotalPrice;
        item.ApplyDiscountChange(e.DiscountPercentage);
        _totalPrice = _totalPrice - oldPrice + item.TotalPrice;
    }

    private void Apply(CartCheckedOutDomainEvent _)
    {
        _isCheckedOut = true;
    }

    private void Apply(CartClearedDomainEvent _)
    {
        _items.Clear();
        _totalPrice = 0;
    }

    #region Validation Methods

    private UnitResult<Error> ValidateNotCheckedOut()
    {
        if (_isCheckedOut)
            return UnitResult.Failure<Error>(Domain.Errors.Errors.CartAlreadyCheckedOut);

        return UnitResult.Success<Error>();
    }

    private UnitResult<Error> ValidateNotDuplicateProduct(int productId)
    {
        if (_items.Any(i => i.ProductId == productId))
            return UnitResult.Failure<Error>(Domain.Errors.Errors.DuplicateProduct);

        return UnitResult.Success<Error>();
    }

    private UnitResult<Error> ValidateMaxItemsCount()
    {
        if (_items.Count >= MaxItemsCount)
            return UnitResult.Failure<Error>(Domain.Errors.Errors.MaxItemsCountExceeded);

        return UnitResult.Success<Error>();
    }

    private UnitResult<Error> ValidateTotalQuantity(int quantityDifference)
    {
        if (TotalQuantity + quantityDifference > MaxTotalQuantity)
            return UnitResult.Failure<Error>(Domain.Errors.Errors.MaxTotalQuantityExceeded);

        return UnitResult.Success<Error>();
    }

    private UnitResult<Error> ValidateTotalPriceForAdd(int quantity, decimal unitPrice)
    {
        var itemTotalPrice = unitPrice * quantity;
        var newTotalPrice = _totalPrice + itemTotalPrice;

        if (newTotalPrice > MaxTotalPrice)
            return UnitResult.Failure<Error>(Domain.Errors.Errors.MaxTotalPriceExceeded);

        return UnitResult.Success<Error>();
    }

    private Result<CartItem, Error> FindItem(int productId)
    {
        var item = _items.SingleOrDefault(i => i.ProductId == productId);
        if (item is null)
            return Result.Failure<CartItem, Error>(Domain.Errors.Errors.ItemNotFound);

        return Result.Success<CartItem, Error>(item);
    }

    private CartItem GetItemByProductId(int productId)
    {
        return _items.Single(i => i.ProductId == productId);
    }

    private UnitResult<Error> ValidateTotalPriceForChange(CartItem item, int newQuantity)
    {
        var oldTotalPrice = item.TotalPrice;
        var newItemTotalPrice = item.CalculateTotalPriceWithQuantity(newQuantity);
        var newTotalPrice = _totalPrice - oldTotalPrice + newItemTotalPrice;

        if (newTotalPrice > MaxTotalPrice)
            return UnitResult.Failure<Error>(Domain.Errors.Errors.MaxTotalPriceExceeded);

        return UnitResult.Success<Error>();
    }

    private UnitResult<Error> ValidateNotEmpty()
    {
        if (_items.Count == 0)
            return UnitResult.Failure<Error>(Domain.Errors.Errors.EmptyCart);

        return UnitResult.Success<Error>();
    }

    #endregion
}
