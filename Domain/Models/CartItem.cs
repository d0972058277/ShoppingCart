using CSharpFunctionalExtensions;
using Domain.Primitives;
using Domain.Errors;

namespace Domain.Models;

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
    /// 決定是否可以建立新的購物車項目。
    /// </summary>
    public static UnitResult<Error> DecideCreate(int productId, int quantity, decimal unitPrice)
    {
        return ValidateProductId(productId)
            .Bind(() => ValidateQuantity(quantity))
            .Bind(() => ValidateUnitPrice(unitPrice));
    }

    /// <summary>
    /// 套用建立購物車項目（不可失敗）。
    /// </summary>
    public static CartItem ApplyCreate(int productId, int quantity, decimal unitPrice)
    {
        return new CartItem(productId, quantity, unitPrice);
    }

    /// <summary>
    /// 決定是否可以變更商品數量。
    /// </summary>
    public UnitResult<Error> DecideChangeQuantity(int quantity)
    {
        return ValidateQuantity(quantity);
    }

    /// <summary>
    /// 套用數量變更（不可失敗）。
    /// </summary>
    public void ApplyChangeQuantity(int quantity)
    {
        Quantity = quantity;
    }

    /// <summary>
    /// 決定是否可以套用折扣。
    /// </summary>
    public UnitResult<Error> DecideApplyDiscount(decimal discountPercentage)
    {
        return ValidateDiscountRange(discountPercentage)
            .Bind(() => ValidateDiscountDecimalPlaces(discountPercentage))
            .Bind(() => ValidateDiscountNotReduced(discountPercentage));
    }

    /// <summary>
    /// 套用折扣變更（不可失敗）。
    /// </summary>
    public void ApplyDiscountChange(decimal discountPercentage)
    {
        DiscountPercentage = discountPercentage;
    }

    /// <summary>
    /// 決定是否可以更新單價。
    /// </summary>
    public UnitResult<Error> DecideUpdateUnitPrice(decimal newUnitPrice)
    {
        return ValidateUnitPrice(newUnitPrice);
    }

    /// <summary>
    /// 套用單價更新（不可失敗）。
    /// </summary>
    public void ApplyUpdateUnitPrice(decimal newUnitPrice)
    {
        UnitPrice = newUnitPrice;
    }

    /// <summary>
    /// 驗證庫存（模擬）。
    /// </summary>
    public UnitResult<Error> ValidateStock()
    {
        // 模擬庫存檢查：假設商品 ID 為偶數的有庫存
        // 這只是示範，實際應該查詢庫存系統
        if (ProductId % 2 == 1 && Quantity > 50)
            return UnitResult.Failure<Error>(Domain.Errors.Errors.InsufficientStock);

        return UnitResult.Success<Error>();
    }

    #region Validation Methods

    private static UnitResult<Error> ValidateProductId(int productId)
    {
        if (productId <= 0)
            return UnitResult.Failure<Error>(Domain.Errors.Errors.InvalidProductId);

        return UnitResult.Success<Error>();
    }

    private static UnitResult<Error> ValidateQuantity(int quantity)
    {
        if (quantity < MinQuantity)
            return UnitResult.Failure<Error>(Domain.Errors.Errors.InvalidQuantity);

        if (quantity > MaxQuantity)
            return UnitResult.Failure<Error>(Domain.Errors.Errors.MaxItemQuantityExceeded);

        return UnitResult.Success<Error>();
    }

    private static UnitResult<Error> ValidateUnitPrice(decimal unitPrice)
    {
        if (unitPrice < MinUnitPrice)
            return UnitResult.Failure<Error>(Domain.Errors.Errors.InvalidUnitPrice);

        if (unitPrice > MaxUnitPrice)
            return UnitResult.Failure<Error>(Domain.Errors.Errors.MaxUnitPriceExceeded);

        if (Math.Round(unitPrice, 2) != unitPrice)
            return UnitResult.Failure<Error>(Domain.Errors.Errors.InvalidUnitPriceDecimalPlaces);

        return UnitResult.Success<Error>();
    }

    private static UnitResult<Error> ValidateDiscountRange(decimal discountPercentage)
    {
        if (discountPercentage < MinDiscountPercentage)
            return UnitResult.Failure<Error>(Domain.Errors.Errors.InvalidDiscountPercentage);

        if (discountPercentage > MaxDiscountPercentage)
            return UnitResult.Failure<Error>(Domain.Errors.Errors.InvalidDiscountPercentage);

        return UnitResult.Success<Error>();
    }

    private static UnitResult<Error> ValidateDiscountDecimalPlaces(decimal discountPercentage)
    {
        if (Math.Round(discountPercentage, 2) != discountPercentage)
            return UnitResult.Failure<Error>(Domain.Errors.Errors.InvalidDiscountDecimalPlaces);

        return UnitResult.Success<Error>();
    }

    private UnitResult<Error> ValidateDiscountNotReduced(decimal discountPercentage)
    {
        if (discountPercentage < DiscountPercentage)
            return UnitResult.Failure<Error>(Domain.Errors.Errors.DiscountCannotBeReduced);

        return UnitResult.Success<Error>();
    }

    #endregion
}
