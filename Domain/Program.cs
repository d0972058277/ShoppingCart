using Domain.Models;

Console.WriteLine("=== 購物車複雜驗證示範 ===\n");

// 建立新的購物車
var cart = ShoppingCart.Create();
Console.WriteLine($"✓ 購物車已建立 (ID: {cart.Id})\n");

// ====== 情境 1: 基本商品加入與驗證 ======
Console.WriteLine("====== 情境 1: 基本商品加入與驗證 ======");

// 加入正常商品
var addResult1 = cart.AddItem(productId: 101, quantity: 2, unitPrice: 99.99m);
Console.WriteLine(addResult1.IsSuccess
    ? "✓ 已加入商品 101 (數量: 2, 單價: $99.99)"
    : $"✗ 加入失敗: {addResult1.Error.Message}");

// 加入另一個商品
var addResult2 = cart.AddItem(productId: 102, quantity: 5, unitPrice: 49.50m);
Console.WriteLine(addResult2.IsSuccess
    ? "✓ 已加入商品 102 (數量: 5, 單價: $49.50)"
    : $"✗ 加入失敗: {addResult2.Error.Message}");

// 嘗試加入重複商品（驗證失敗）
var addResult3 = cart.AddItem(productId: 101, quantity: 3, unitPrice: 99.99m);
Console.WriteLine(addResult3.IsSuccess
    ? "✓ 已加入商品 101"
    : $"✗ 加入失敗: {addResult3.Error.Message}");

Console.WriteLine($"\n目前購物車: {cart.Items.Count} 項商品, 總金額: ${cart.TotalPrice:N2}\n");

// ====== 情境 2: 數量與價格驗證 ======
Console.WriteLine("====== 情境 2: 數量與價格驗證 ======");

// 嘗試加入無效單價（驗證失敗）
var addResult4 = cart.AddItem(productId: 103, quantity: 1, unitPrice: 0.001m);
Console.WriteLine(addResult4.IsSuccess
    ? "✓ 已加入商品 103"
    : $"✗ 加入失敗: {addResult4.Error.Message}");

// 嘗試加入小數位數過多的單價（驗證失敗）
var addResult5 = cart.AddItem(productId: 104, quantity: 1, unitPrice: 12.345m);
Console.WriteLine(addResult5.IsSuccess
    ? "✓ 已加入商品 104"
    : $"✗ 加入失敗: {addResult5.Error.Message}");

// 嘗試加入數量超過上限的商品（驗證失敗）
var addResult6 = cart.AddItem(productId: 105, quantity: 101, unitPrice: 10.00m);
Console.WriteLine(addResult6.IsSuccess
    ? "✓ 已加入商品 105"
    : $"✗ 加入失敗: {addResult6.Error.Message}");

Console.WriteLine();

// ====== 情境 3: 折扣套用與驗證 ======
Console.WriteLine("====== 情境 3: 折扣套用與驗證 ======");

// 套用正常折扣
var discountResult1 = cart.ApplyDiscount(productId: 101, discountPercentage: 10.00m);
Console.WriteLine(discountResult1.IsSuccess
    ? "✓ 已對商品 101 套用 10% 折扣"
    : $"✗ 套用失敗: {discountResult1.Error.Message}");

// 套用更高的折扣（允許）
var discountResult2 = cart.ApplyDiscount(productId: 101, discountPercentage: 20.00m);
Console.WriteLine(discountResult2.IsSuccess
    ? "✓ 已對商品 101 套用 20% 折扣"
    : $"✗ 套用失敗: {discountResult2.Error.Message}");

// 嘗試降低折扣（驗證失敗）
var discountResult3 = cart.ApplyDiscount(productId: 101, discountPercentage: 15.00m);
Console.WriteLine(discountResult3.IsSuccess
    ? "✓ 已對商品 101 套用 15% 折扣"
    : $"✗ 套用失敗: {discountResult3.Error.Message}");

// 嘗試套用無效折扣（驗證失敗）
var discountResult4 = cart.ApplyDiscount(productId: 102, discountPercentage: 101.00m);
Console.WriteLine(discountResult4.IsSuccess
    ? "✓ 已對商品 102 套用折扣"
    : $"✗ 套用失敗: {discountResult4.Error.Message}");

Console.WriteLine($"\n目前購物車總金額: ${cart.TotalPrice:N2}\n");

// ====== 情境 4: 數量變更與上限驗證 ======
Console.WriteLine("====== 情境 4: 數量變更與上限驗證 ======");

// 正常變更數量
var changeResult1 = cart.ChangeItemQuantity(productId: 101, quantity: 10);
Console.WriteLine(changeResult1.IsSuccess
    ? "✓ 商品 101 數量已變更為 10"
    : $"✗ 變更失敗: {changeResult1.Error.Message}");

// 嘗試變更為無效數量（驗證失敗）
var changeResult2 = cart.ChangeItemQuantity(productId: 101, quantity: 0);
Console.WriteLine(changeResult2.IsSuccess
    ? "✓ 商品 101 數量已變更為 0"
    : $"✗ 變更失敗: {changeResult2.Error.Message}");

// 嘗試變更為超過上限的數量（驗證失敗）
var changeResult3 = cart.ChangeItemQuantity(productId: 102, quantity: 150);
Console.WriteLine(changeResult3.IsSuccess
    ? "✓ 商品 102 數量已變更為 150"
    : $"✗ 變更失敗: {changeResult3.Error.Message}");

Console.WriteLine();

// ====== 情境 5: 總數量與總金額上限驗證 ======
Console.WriteLine("====== 情境 5: 總數量與總金額上限驗證 ======");

// 加入高價商品
var addResult7 = cart.AddItem(productId: 201, quantity: 1, unitPrice: 50000.00m);
Console.WriteLine(addResult7.IsSuccess
    ? "✓ 已加入商品 201 (單價: $50,000)"
    : $"✗ 加入失敗: {addResult7.Error.Message}");

// 嘗試加入會超過總金額上限的商品（驗證失敗）
var addResult8 = cart.AddItem(productId: 202, quantity: 1, unitPrice: 999999.99m);
Console.WriteLine(addResult8.IsSuccess
    ? "✓ 已加入商品 202"
    : $"✗ 加入失敗: {addResult8.Error.Message}");

Console.WriteLine($"\n目前購物車: 總數量 {cart.TotalQuantity}, 總金額 ${cart.TotalPrice:N2}\n");

// ====== 情境 6: 顯示購物車詳細內容 ======
Console.WriteLine("====== 情境 6: 購物車詳細內容 ======");
foreach (var item in cart.Items)
{
    Console.WriteLine($"商品 {item.ProductId}:");
    Console.WriteLine($"  數量: {item.Quantity}");
    Console.WriteLine($"  單價: ${item.UnitPrice:N2}");
    Console.WriteLine($"  折扣: {item.DiscountPercentage}%");
    Console.WriteLine($"  折扣後單價: ${item.DiscountedUnitPrice:N2}");
    Console.WriteLine($"  小計: ${item.TotalPrice:N2}");
}
Console.WriteLine($"\n總計: ${cart.TotalPrice:N2}\n");

// ====== 情境 7: 結帳驗證 ======
Console.WriteLine("====== 情境 7: 結帳驗證 ======");

// 正常結帳
var checkoutResult1 = cart.Checkout();
Console.WriteLine(checkoutResult1.IsSuccess
    ? "✓ 購物車已成功結帳"
    : $"✗ 結帳失敗: {checkoutResult1.Error.Message}");

// 嘗試再次結帳（驗證失敗）
var checkoutResult2 = cart.Checkout();
Console.WriteLine(checkoutResult2.IsSuccess
    ? "✓ 購物車已結帳"
    : $"✗ 結帳失敗: {checkoutResult2.Error.Message}");

// 嘗試在結帳後加入商品（驗證失敗）
var addResult9 = cart.AddItem(productId: 301, quantity: 1, unitPrice: 10.00m);
Console.WriteLine(addResult9.IsSuccess
    ? "✓ 已加入商品 301"
    : $"✗ 加入失敗: {addResult9.Error.Message}");

Console.WriteLine();

// ====== 情境 8: 空購物車結帳驗證 ======
Console.WriteLine("====== 情境 8: 空購物車結帳驗證 ======");
var emptyCart = ShoppingCart.Create();
var checkoutResult3 = emptyCart.Checkout();
Console.WriteLine(checkoutResult3.IsSuccess
    ? "✓ 空購物車已結帳"
    : $"✗ 結帳失敗: {checkoutResult3.Error.Message}");

Console.WriteLine();

// ====== 顯示領域事件 ======
Console.WriteLine("====== 領域事件記錄 ======");
var events = cart.GetDomainEvents();
foreach (var evt in events)
{
    Console.WriteLine($"• {evt.GetType().Name} at {evt.OccurredOn:yyyy-MM-dd HH:mm:ss}");
}
Console.WriteLine($"\n總共 {events.Count} 個領域事件");

Console.WriteLine("\n====== 驗證總結 ======");
Console.WriteLine("✓ 展示了 20+ 種驗證情境");
Console.WriteLine("✓ 包含商品加入、數量變更、折扣套用、結帳等多種操作");
Console.WriteLine("✓ 驗證了數量、價格、折扣、狀態等多個維度的限制");
