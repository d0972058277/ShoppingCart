# ShoppingCart - Event Sourcing DDD Implementation

這是一個使用 **Functional Programming + Railway Oriented Programming + Event Sourcing** 風格的 DDD 實作範例。

## 核心設計理念

本專案展現了三個範式的完美融合：

- **Functional Programming (FP)**：純函數驗證邏輯
- **Railway Oriented Programming (ROP)**：可組合的錯誤處理鏈
- **Event Sourcing (ES)**：事件驅動的狀態管理
- **Domain-Driven Design (DDD)**：領域模型與業務規則封裝

## 架構模式

### Decider Pattern

本專案實作了標準的 **Decider Pattern**，由三個核心元素組成：

```
Command → Decide (驗證) → Event (事實) → Apply (狀態變更)
          ↑ Railway 在這裡    ↑ Tap 在這裡   ↑ 事件處理器
```

#### 1. **Decide (決策函數)**
接收命令和當前狀態，執行驗證，返回事件或錯誤。

```csharp
public UnitResult<Error> AddItem(int productId, int quantity, decimal unitPrice)
{
    return ValidateNotCheckedOut()
        .Bind(() => ValidateNotDuplicateProduct(productId))
        .Bind(() => ValidateMaxItemsCount())
        .Bind(() => CartItem.DecideCreate(productId, quantity, unitPrice))
        .Bind(() => ValidateTotalQuantity(quantity))
        .Bind(() => ValidateTotalPriceForAdd(quantity, unitPrice))
        .Tap(() => RaiseEvent(new CartItemAddedDomainEvent(...)));
}
```

#### 2. **Apply (演化函數)**
接收事件，修改狀態（純狀態變更，無驗證）。

```csharp
private void Apply(CartItemAddedDomainEvent e)
{
    var newItem = CartItem.ApplyCreate(e.ProductId, e.Quantity, e.UnitPrice);
    _items.Add(newItem);
    _totalPrice += newItem.TotalPrice;
}
```

#### 3. **Initial State (初始狀態)**
定義聚合的初始狀態。

```csharp
public static ShoppingCart Create()
{
    return new ShoppingCart(Guid.NewGuid());
}
```

## Railway Oriented Programming

### 設計原則

**Railway 階段僅做驗證以及彙整 Domain Event 必要的資訊**

驗證鏈就像火車軌道，任何一站失敗就切換到「失敗軌道」：

```csharp
return ValidateNotCheckedOut()           // 🚂 第一站檢查點
    .Bind(() => ValidateNotDuplicateProduct(productId))  // 🚂 第二站檢查點
    .Bind(() => ValidateMaxItemsCount())                 // 🚂 第三站檢查點
    .Bind(() => CartItem.DecideCreate(...))              // 🚂 第四站檢查點
    .Bind(() => ValidateTotalQuantity(quantity))         // 🚂 第五站檢查點
    .Bind(() => ValidateTotalPriceForAdd(...))           // 🚂 最後檢查點
    .Tap(() => RaiseEvent(...));  // ✅ 所有檢查點都通過才到達終點站
```

- **成功軌道**：一路綠燈 → `Tap` 執行
- **失敗軌道**：任一紅燈 → 短路返回錯誤

### 為什麼 Tap 在最後？

刻意將 `Tap` 放到最後一步，確保：

1. **事件只在所有驗證通過後才產生**
2. **避免部分成功的狀態**
3. **事件即承諾** - 事件代表「已經發生的事實」
4. **與 Apply 的對稱性** - Apply 不做驗證，只修改狀態
5. **事務邊界清晰** - Success = 有事件，Failure = 無副作用

```csharp
// ✅ 正確：Railway 只做驗證
.Bind(() => ValidateTotalPriceForAdd(quantity, unitPrice))  // 純函數
.Tap(() => RaiseEvent(new CartItemAddedDomainEvent(...)))   // 副作用

// ❌ 錯誤：在 Railway 中修改狀態
.Bind(() => {
    _items.Add(newItem);  // 💥 副作用！破壞純函數特性！
    return UnitResult.Success<Error>();
})
```

## 領域模型

### ShoppingCart (聚合根)

購物車聚合根，負責管理購物車項目和整體業務規則。

**核心業務規則：**
- 最多 50 種不同商品
- 總數量上限 999
- 總金額上限 1,000,000
- 結帳後不可修改
- 空購物車不可結帳

**主要操作：**
- `AddItem` - 加入商品
- `ChangeItemQuantity` - 變更數量
- `RemoveItem` - 移除商品
- `ApplyDiscount` - 套用折扣
- `Checkout` - 結帳
- `Clear` - 清空購物車

參考：[ShoppingCart.cs](Domain/Models/ShoppingCart.cs)

### CartItem (實體)

購物車項目實體，代表單一商品項目。

**核心業務規則：**
- 數量範圍：1-100
- 單價範圍：0.01-999,999.99
- 折扣百分比：0-100（最多兩位小數）
- 折扣只能增加不能減少

**價格計算：**
- `DiscountedUnitPrice` - 折扣後單價
- `TotalPrice` - 總價（折扣後）
- `OriginalTotalPrice` - 原始總價（折扣前）

參考：[CartItem.cs](Domain/Models/CartItem.cs)

## 職責劃分

| 階段 | 職責 | 副作用 | 特性 |
|------|------|--------|------|
| `Decide` (驗證鏈) | 驗證規則、收集事件所需資訊 | ❌ 無 | 純函數、可組合 |
| `Tap` (產生事件) | 產生 Domain Event | ✅ 有 | 唯一副作用點 |
| `Apply` (狀態變更) | 套用事件修改狀態 | ✅ 有 | 不可失敗、確定性 |

## 純函數驗證

所有驗證方法都是純函數：

```csharp
private UnitResult<Error> ValidateNotCheckedOut()
{
    if (_isCheckedOut)
        return UnitResult.Failure<Error>(Errors.CartAlreadyCheckedOut);

    return UnitResult.Success<Error>();
}
```

**特性：**
- ✅ 無副作用（不修改狀態）
- ✅ 確定性（相同輸入 = 相同輸出）
- ✅ 可組合（透過 `Bind` 串接）
- ✅ 可測試（不依賴外部狀態）

## Decide-Apply 分離

### CartItem 的範例

**Decide 函數** - 決定是否可以執行操作：
```csharp
public static UnitResult<Error> DecideCreate(int productId, int quantity, decimal unitPrice)
{
    return ValidateProductId(productId)
        .Bind(() => ValidateQuantity(quantity))
        .Bind(() => ValidateUnitPrice(unitPrice));
}

public UnitResult<Error> DecideApplyDiscount(decimal discountPercentage)
{
    return ValidateDiscountRange(discountPercentage)
        .Bind(() => ValidateDiscountDecimalPlaces(discountPercentage))
        .Bind(() => ValidateDiscountNotReduced(discountPercentage));
}
```

**Apply 函數** - 套用操作（不可失敗）：
```csharp
public static CartItem ApplyCreate(int productId, int quantity, decimal unitPrice)
{
    return new CartItem(productId, quantity, unitPrice);
}

public void ApplyDiscountChange(decimal discountPercentage)
{
    DiscountPercentage = discountPercentage;
}
```

## 事件溯源特性

### 可重播性 (Replayability)

透過重播事件序列，可以重建聚合的任何歷史狀態：

```csharp
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
```

### 事件包含所有必要資訊

```csharp
new CartItemAddedDomainEvent(
    CartId: Id,           // 從當前狀態
    ProductId: productId, // 從命令參數
    Quantity: quantity,   // 從命令參數
    UnitPrice: unitPrice  // 從命令參數
)
```

Railway 階段已經驗證並準備好所有資訊，`Tap` 只需要「組裝」事件。

## 狀態封裝

- 所有欄位都是 `private`，對外只暴露 `public` 唯讀屬性
- 狀態變更只能透過 `Apply` 方法（由事件觸發）
- 建構函式是 `private`，只能透過靜態工廠方法建立

```csharp
public class ShoppingCart : EventSourcedAggregateRoot<Guid>
{
    private readonly List<CartItem> _items = new();
    private decimal _totalPrice;
    private bool _isCheckedOut;

    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();
    public decimal TotalPrice => _totalPrice;
    public bool IsCheckedOut => _isCheckedOut;

    private ShoppingCart(Guid id) : base(id) { }

    public static ShoppingCart Create()
    {
        return new ShoppingCart(Guid.NewGuid());
    }
}
```

## 可測試性

### 測試 Decide（純函數）

```csharp
[Fact]
public void AddItem_WhenExceedsMaxTotalQuantity_ShouldFail()
{
    var cart = ShoppingCart.Create();

    var result = cart.AddItem(productId: 1, quantity: 999, unitPrice: 100m);

    Assert.True(result.IsFailure);
    Assert.Equal(Errors.MaxTotalQuantityExceeded, result.Error);
}
```

### 測試 Apply（事件重播）

```csharp
[Fact]
public void Apply_WithEventSequence_ShouldReconstructState()
{
    var events = new[] {
        new CartItemAddedDomainEvent(...),
        new CartItemQuantityChangedDomainEvent(...)
    };

    var cart = ShoppingCart.ReplayEvents(events);

    Assert.Equal(expectedTotal, cart.TotalPrice);
}
```

## 設計優點

✅ **原子性** - 要麼所有驗證都過，要麼完全不變
✅ **一致性** - 事件永遠代表有效的狀態轉換
✅ **可預測性** - 呼叫端可以信任 `IsSuccess` 的結果
✅ **可重播性** - 事件序列保證能重建有效狀態
✅ **可測試性** - 純函數易於測試，事件可重播驗證
✅ **可讀性** - 驗證邏輯清晰，流程一目了然
✅ **可維護性** - 責任分離清晰，易於擴展

## 使用技術

- C# 12
- .NET 8
- [CSharpFunctionalExtensions](https://github.com/vkhorikov/CSharpFunctionalExtensions) - Railway Oriented Programming
- xUnit - 單元測試

## 專案結構

```
Shopcart/
├── Domain/
│   ├── Models/
│   │   ├── ShoppingCart.cs      # 購物車聚合根
│   │   └── CartItem.cs          # 購物車項目實體
│   ├── Events/                   # Domain Events
│   ├── Errors/                   # Domain Errors
│   └── Primitives/              # 基礎建構元件
└── Domain.Tests/                # 單元測試
```

## 參考資料

- [Railway Oriented Programming](https://fsharpforfunandprofit.com/rop/) - Scott Wlaschin
- [Decider Pattern](https://thinkbeforecoding.com/post/2021/12/17/functional-event-sourcing-decider) - Jérémie Chassaing
- [Domain-Driven Design](https://www.domainlanguage.com/ddd/) - Eric Evans
- [Event Sourcing](https://martinfowler.com/eaaDev/EventSourcing.html) - Martin Fowler

## 授權

MIT License
