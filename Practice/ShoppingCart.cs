using CSharpFunctionalExtensions;

namespace Practice;

/// <summary>
/// 購物車聚合根。
/// </summary>
public class ShoppingCart : AggregateRoot<Guid>
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

        // Validation 4: 建立 CartItem（內部會驗證 productId, quantity, unitPrice）
        var createResult = CartItem.Create(productId, quantity, unitPrice);
        if (createResult.IsFailure)
            return UnitResult.Failure<Error>(createResult.Error);

        var newItem = createResult.Value;

        // Validation 5: 檢查總數量上限
        if (TotalQuantity + quantity > MaxTotalQuantity)
            return UnitResult.Failure<Error>(Errors.MaxTotalQuantityExceeded);

        // Validation 6: 檢查總金額上限
        var newTotalPrice = _totalPrice + newItem.TotalPrice;
        if (newTotalPrice > MaxTotalPrice)
            return UnitResult.Failure<Error>(Errors.MaxTotalPriceExceeded);

        // 加入商品並更新總金額
        _items.Add(newItem);
        _totalPrice = newTotalPrice;

        AddDomainEvent(
            new CartItemAddedDomainEvent(
                CartId: Id,
                ProductId: productId,
                Quantity: quantity
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

        // 保存舊值用於復原
        var oldQuantity = item.Quantity;
        var oldTotalPrice = item.TotalPrice;

        // Validation 3: 變更數量（CartItem 內部會驗證數量是否有效）
        var changeResult = item.ChangeQuantity(quantity);
        if (changeResult.IsFailure)
            return UnitResult.Failure<Error>(changeResult.Error);

        // Validation 4: 檢查變更後的總數量是否超過上限
        if (TotalQuantity > MaxTotalQuantity)
        {
            // 復原變更
            item.ChangeQuantity(oldQuantity);
            return UnitResult.Failure<Error>(Errors.MaxTotalQuantityExceeded);
        }

        // Validation 5: 檢查變更後的總金額是否超過上限
        var newTotalPrice = _totalPrice - oldTotalPrice + item.TotalPrice;
        if (newTotalPrice > MaxTotalPrice)
        {
            // 復原變更
            item.ChangeQuantity(oldQuantity);
            return UnitResult.Failure<Error>(Errors.MaxTotalPriceExceeded);
        }

        // 更新總金額
        _totalPrice = newTotalPrice;

        AddDomainEvent(
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

        // 移除商品並更新總金額
        _items.Remove(item);
        _totalPrice -= item.TotalPrice;

        AddDomainEvent(
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

        // Validation 3: 套用折扣（CartItem 內部會驗證折扣比例）
        var applyResult = item.ApplyDiscount(discountPercentage);
        if (applyResult.IsFailure)
            return UnitResult.Failure<Error>(applyResult.Error);

        // 更新總金額
        _totalPrice = _totalPrice - oldTotalPrice + item.TotalPrice;

        AddDomainEvent(
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

        _isCheckedOut = true;

        AddDomainEvent(
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

        _items.Clear();
        _totalPrice = 0;

        AddDomainEvent(
            new CartClearedDomainEvent(CartId: Id)
        );

        return UnitResult.Success<Error>();
    }
}
