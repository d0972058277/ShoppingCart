namespace Practice;

/// <summary>
/// 購物車項目數量已變更的領域事件。
/// </summary>
public sealed record CartItemQuantityChangedDomainEvent(
    Guid CartId,
    int ProductId,
    int Quantity
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
