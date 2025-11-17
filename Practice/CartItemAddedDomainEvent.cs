namespace Practice;

/// <summary>
/// 購物車項目已加入的領域事件。
/// </summary>
public sealed record CartItemAddedDomainEvent(
    Guid CartId,
    int ProductId,
    int Quantity
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
