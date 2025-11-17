using CSharpFunctionalExtensions;

namespace Practice;

/// <summary>
/// 購物車項目實體。
/// </summary>
public class CartItem : Entity<Guid>
{
    private const int MinQuantity = 1;
    private const int MaxQuantity = 100;
    private const decimal MinUnitPrice = 0.01m;
    private const decimal MaxUnitPrice = 999_999.99m;
    private const decimal MinDiscountPercentage = 0m;
    private const decimal MaxDiscountPercentage = 100m;

    /// <summary>
    /// 取得商品識別碼。
    /// </summary>
    public int ProductId { get; private set; }

    /// <summary>
    /// 取得商品數量。
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// 取得單價。
    /// </summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>
    /// 取得折扣百分比（0-100）。
    /// </summary>
    public decimal DiscountPercentage { get; private set; }

    /// <summary>
    /// 取得折扣後的單價。
    /// </summary>
    public decimal DiscountedUnitPrice => UnitPrice * (1 - DiscountPercentage / 100);

    /// <summary>
    /// 取得總價（折扣後）。
    /// </summary>
    public decimal TotalPrice => DiscountedUnitPrice * Quantity;

    /// <summary>
    /// 取得原始總價（折扣前）。
    /// </summary>
    public decimal OriginalTotalPrice => UnitPrice * Quantity;

    /// <summary>
    /// 私有建構函式供內部使用。
    /// </summary>
    private CartItem(int productId, int quantity, decimal unitPrice) : base(Guid.NewGuid())
    {
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        DiscountPercentage = 0;
    }

    /// <summary>
    /// 建立新的購物車項目。
    /// </summary>
    public static Result<CartItem, Error> Create(int productId, int quantity, decimal unitPrice)
    {
        // Validation 1: 檢查商品 ID
        if (productId <= 0)
            return Result.Failure<CartItem, Error>(Errors.InvalidProductId);

        // Validation 2: 檢查數量範圍
        if (quantity < MinQuantity)
            return Result.Failure<CartItem, Error>(Errors.InvalidQuantity);

        if (quantity > MaxQuantity)
            return Result.Failure<CartItem, Error>(Errors.MaxItemQuantityExceeded);

        // Validation 3: 檢查單價範圍
        if (unitPrice < MinUnitPrice)
            return Result.Failure<CartItem, Error>(Errors.InvalidUnitPrice);

        if (unitPrice > MaxUnitPrice)
            return Result.Failure<CartItem, Error>(Errors.MaxUnitPriceExceeded);

        // Validation 4: 檢查單價小數位數（最多 2 位）
        if (Math.Round(unitPrice, 2) != unitPrice)
            return Result.Failure<CartItem, Error>(Errors.InvalidUnitPriceDecimalPlaces);

        return new CartItem(productId, quantity, unitPrice);
    }

    /// <summary>
    /// 變更商品數量。
    /// </summary>
    public UnitResult<Error> ChangeQuantity(int quantity)
    {
        // Validation 1: 檢查數量範圍
        if (quantity < MinQuantity)
            return UnitResult.Failure<Error>(Errors.InvalidQuantity);

        // Validation 2: 檢查數量上限
        if (quantity > MaxQuantity)
            return UnitResult.Failure<Error>(Errors.MaxItemQuantityExceeded);

        Quantity = quantity;
        return UnitResult.Success<Error>();
    }

    /// <summary>
    /// 套用折扣。
    /// </summary>
    public UnitResult<Error> ApplyDiscount(decimal discountPercentage)
    {
        // Validation 1: 檢查折扣範圍
        if (discountPercentage < MinDiscountPercentage)
            return UnitResult.Failure<Error>(Errors.InvalidDiscountPercentage);

        if (discountPercentage > MaxDiscountPercentage)
            return UnitResult.Failure<Error>(Errors.InvalidDiscountPercentage);

        // Validation 2: 檢查折扣小數位數（最多 2 位）
        if (Math.Round(discountPercentage, 2) != discountPercentage)
            return UnitResult.Failure<Error>(Errors.InvalidDiscountDecimalPlaces);

        // Validation 3: 檢查是否已有更優惠的折扣
        if (discountPercentage < DiscountPercentage)
            return UnitResult.Failure<Error>(Errors.DiscountCannotBeReduced);

        DiscountPercentage = discountPercentage;
        return UnitResult.Success<Error>();
    }

    /// <summary>
    /// 更新單價（用於價格變動時）。
    /// </summary>
    public UnitResult<Error> UpdateUnitPrice(decimal newUnitPrice)
    {
        // Validation 1: 檢查單價範圍
        if (newUnitPrice < MinUnitPrice)
            return UnitResult.Failure<Error>(Errors.InvalidUnitPrice);

        if (newUnitPrice > MaxUnitPrice)
            return UnitResult.Failure<Error>(Errors.MaxUnitPriceExceeded);

        // Validation 2: 檢查單價小數位數
        if (Math.Round(newUnitPrice, 2) != newUnitPrice)
            return UnitResult.Failure<Error>(Errors.InvalidUnitPriceDecimalPlaces);

        UnitPrice = newUnitPrice;
        return UnitResult.Success<Error>();
    }

    /// <summary>
    /// 驗證庫存（模擬）。
    /// </summary>
    public UnitResult<Error> ValidateStock()
    {
        // 模擬庫存檢查：假設商品 ID 為偶數的有庫存
        // 這只是示範，實際應該查詢庫存系統
        if (ProductId % 2 == 1 && Quantity > 50)
            return UnitResult.Failure<Error>(Errors.InsufficientStock);

        return UnitResult.Success<Error>();
    }
}
