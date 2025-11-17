using CSharpFunctionalExtensions;

namespace Practice;

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
        // Validation 1: 檢查購物車是否已結帳
        if (_isCheckedOut)
            return UnitResult.Failure<Error>(Errors.CartAlreadyCheckedOut);

        // Validation 2: 檢查是否為重複商品
        if (_items.Any(i => i.ProductId == productId))
            return UnitResult.Failure<Error>(Errors.DuplicateProduct);

        // Validation 3: 檢查購物車項目數量上限
        if (_items.Count >= MaxItemsCount)
            return UnitResult.Failure<Error>(Errors.MaxItemsCountExceeded);

        // Validation 4: 驗證是否可以建立 CartItem（使用 Decide，不建立物件）
        var decideResult = CartItem.DecideCreate(productId, quantity, unitPrice);
        if (decideResult.IsFailure)
            return UnitResult.Failure<Error>(decideResult.Error);

        // Validation 5: 檢查總數量上限
        if (TotalQuantity + quantity > MaxTotalQuantity)
            return UnitResult.Failure<Error>(Errors.MaxTotalQuantityExceeded);

        // Validation 6: 計算並檢查總金額上限
        var discountedUnitPrice = unitPrice; // 新項目折扣為 0
        var itemTotalPrice = discountedUnitPrice * quantity;
        var newTotalPrice = _totalPrice + itemTotalPrice;
        if (newTotalPrice > MaxTotalPrice)
            return UnitResult.Failure<Error>(Errors.MaxTotalPriceExceeded);

        // 所有驗證通過，發布事件（事件會透過 Apply 修改狀態）
        RaiseEvent(
            new CartItemAddedDomainEvent(
                CartId: Id,
                ProductId: productId,
                Quantity: quantity,
                UnitPrice: unitPrice
            )
        );

        return UnitResult.Success<Error>();
    }

    /// <summary>
    /// 變更購物車項目的數量。
    /// </summary>
    public UnitResult<Error> ChangeItemQuantity(int productId, int quantity)
    {
        // Validation 1: 檢查購物車是否已結帳
        if (_isCheckedOut)
            return UnitResult.Failure<Error>(Errors.CartAlreadyCheckedOut);

        // Validation 2: 檢查商品是否存在
        var item = _items.SingleOrDefault(i => i.ProductId == productId);
        if (item is null)
            return UnitResult.Failure<Error>(Errors.ItemNotFound);

        // 保存舊值用於計算
        var oldQuantity = item.Quantity;
        var oldTotalPrice = item.TotalPrice;

        // Validation 3: 驗證數量變更是否有效（使用 Decide，不改變狀態）
        var decideResult = item.DecideChangeQuantity(quantity);
        if (decideResult.IsFailure)
            return UnitResult.Failure<Error>(decideResult.Error);

        // Validation 4: 檢查變更後的總數量是否超過上限
        var quantityDiff = quantity - oldQuantity;
        if (TotalQuantity + quantityDiff > MaxTotalQuantity)
            return UnitResult.Failure<Error>(Errors.MaxTotalQuantityExceeded);

        // Validation 5: 計算變更後的總金額並檢查是否超過上限
        var newItemTotalPrice = item.DiscountedUnitPrice * quantity;
        var newTotalPrice = _totalPrice - oldTotalPrice + newItemTotalPrice;
        if (newTotalPrice > MaxTotalPrice)
            return UnitResult.Failure<Error>(Errors.MaxTotalPriceExceeded);

        // 所有驗證通過，發布事件（事件會透過 Apply 修改狀態）
        RaiseEvent(
            new CartItemQuantityChangedDomainEvent(
                CartId: Id,
                ProductId: productId,
                Quantity: quantity
            )
        );

        return UnitResult.Success<Error>();
    }

    /// <summary>
    /// 移除購物車項目。
    /// </summary>
    public UnitResult<Error> RemoveItem(int productId)
    {
        // Validation 1: 檢查購物車是否已結帳
        if (_isCheckedOut)
            return UnitResult.Failure<Error>(Errors.CartAlreadyCheckedOut);

        // Validation 2: 檢查商品是否存在
        var item = _items.SingleOrDefault(i => i.ProductId == productId);
        if (item is null)
            return UnitResult.Failure<Error>(Errors.ItemNotFound);

        // 發布事件（事件會透過 Apply 修改狀態）
        RaiseEvent(
            new CartItemRemovedDomainEvent(
                CartId: Id,
                ProductId: productId
            )
        );

        return UnitResult.Success<Error>();
    }

    /// <summary>
    /// 套用折扣到指定商品。
    /// </summary>
    public UnitResult<Error> ApplyDiscount(int productId, decimal discountPercentage)
    {
        // Validation 1: 檢查購物車是否已結帳
        if (_isCheckedOut)
            return UnitResult.Failure<Error>(Errors.CartAlreadyCheckedOut);

        // Validation 2: 檢查商品是否存在
        var item = _items.SingleOrDefault(i => i.ProductId == productId);
        if (item is null)
            return UnitResult.Failure<Error>(Errors.ItemNotFound);

        // 保存舊值
        var oldTotalPrice = item.TotalPrice;

        // Validation 3: 驗證折扣是否有效（使用 Decide，不改變狀態）
        var decideResult = item.DecideApplyDiscount(discountPercentage);
        if (decideResult.IsFailure)
            return UnitResult.Failure<Error>(decideResult.Error);

        // 所有驗證通過，發布事件（事件會透過 Apply 修改狀態）
        RaiseEvent(
            new CartItemDiscountAppliedDomainEvent(
                CartId: Id,
                ProductId: productId,
                DiscountPercentage: discountPercentage
            )
        );

        return UnitResult.Success<Error>();
    }

    /// <summary>
    /// 結帳購物車。
    /// </summary>
    public UnitResult<Error> Checkout()
    {
        // Validation 1: 檢查購物車是否已結帳
        if (_isCheckedOut)
            return UnitResult.Failure<Error>(Errors.CartAlreadyCheckedOut);

        // Validation 2: 檢查購物車是否為空
        if (_items.Count == 0)
            return UnitResult.Failure<Error>(Errors.EmptyCart);

        // Validation 3: 檢查所有商品是否都有庫存（模擬）
        foreach (var item in _items)
        {
            var stockCheckResult = item.ValidateStock();
            if (stockCheckResult.IsFailure)
                return UnitResult.Failure<Error>(stockCheckResult.Error);
        }

        // 發布事件（事件會透過 Apply 修改狀態）
        RaiseEvent(
            new CartCheckedOutDomainEvent(
                CartId: Id,
                TotalPrice: _totalPrice,
                ItemCount: _items.Count
            )
        );

        return UnitResult.Success<Error>();
    }

    /// <summary>
    /// 清空購物車。
    /// </summary>
    public UnitResult<Error> Clear()
    {
        // Validation 1: 檢查購物車是否已結帳
        if (_isCheckedOut)
            return UnitResult.Failure<Error>(Errors.CartAlreadyCheckedOut);

        // 發布事件（事件會透過 Apply 修改狀態）
        RaiseEvent(
            new CartClearedDomainEvent(CartId: Id)
        );

        return UnitResult.Success<Error>();
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
        var item = _items.Single(i => i.ProductId == e.ProductId);
        var oldTotalPrice = item.TotalPrice;
        item.ApplyChangeQuantity(e.Quantity);
        _totalPrice = _totalPrice - oldTotalPrice + item.TotalPrice;
    }

    private void Apply(CartItemRemovedDomainEvent e)
    {
        var item = _items.Single(i => i.ProductId == e.ProductId);
        _items.Remove(item);
        _totalPrice -= item.TotalPrice;
    }

    private void Apply(CartItemDiscountAppliedDomainEvent e)
    {
        var item = _items.Single(i => i.ProductId == e.ProductId);
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
}
