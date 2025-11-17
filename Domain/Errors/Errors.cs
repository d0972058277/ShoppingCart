namespace Domain.Errors;

/// <summary>
/// 定義購物車領域的錯誤清單。
/// </summary>
public static class Errors
{
    // ShoppingCart 相關錯誤
    public static readonly Error DuplicateProduct = new(
        "ShoppingCart.DuplicateProduct",
        "商品已存在於購物車中"
    );

    public static readonly Error ItemNotFound = new(
        "ShoppingCart.ItemNotFound",
        "購物車中找不到此商品"
    );

    public static readonly Error CartAlreadyCheckedOut = new(
        "ShoppingCart.CartAlreadyCheckedOut",
        "購物車已結帳，無法進行修改"
    );

    public static readonly Error MaxItemsCountExceeded = new(
        "ShoppingCart.MaxItemsCountExceeded",
        "購物車項目數量已達上限（最多 50 項）"
    );

    public static readonly Error MaxTotalQuantityExceeded = new(
        "ShoppingCart.MaxTotalQuantityExceeded",
        "購物車總數量已達上限（最多 999 件）"
    );

    public static readonly Error MaxTotalPriceExceeded = new(
        "ShoppingCart.MaxTotalPriceExceeded",
        "購物車總金額已達上限（最多 $1,000,000）"
    );

    public static readonly Error EmptyCart = new(
        "ShoppingCart.EmptyCart",
        "購物車為空，無法結帳"
    );

    // CartItem 相關錯誤
    public static readonly Error InvalidQuantity = new(
        "CartItem.InvalidQuantity",
        "數量必須大於 0"
    );

    public static readonly Error InvalidProductId = new(
        "CartItem.InvalidProductId",
        "商品識別碼必須大於 0"
    );

    public static readonly Error MaxItemQuantityExceeded = new(
        "CartItem.MaxItemQuantityExceeded",
        "單一商品數量已達上限（最多 100 件）"
    );

    public static readonly Error InvalidUnitPrice = new(
        "CartItem.InvalidUnitPrice",
        "單價必須大於 $0.01"
    );

    public static readonly Error MaxUnitPriceExceeded = new(
        "CartItem.MaxUnitPriceExceeded",
        "單價已達上限（最多 $999,999.99）"
    );

    public static readonly Error InvalidUnitPriceDecimalPlaces = new(
        "CartItem.InvalidUnitPriceDecimalPlaces",
        "單價最多只能有 2 位小數"
    );

    public static readonly Error InvalidDiscountPercentage = new(
        "CartItem.InvalidDiscountPercentage",
        "折扣百分比必須介於 0 到 100 之間"
    );

    public static readonly Error InvalidDiscountDecimalPlaces = new(
        "CartItem.InvalidDiscountDecimalPlaces",
        "折扣百分比最多只能有 2 位小數"
    );

    public static readonly Error DiscountCannotBeReduced = new(
        "CartItem.DiscountCannotBeReduced",
        "無法降低已套用的折扣"
    );
}
