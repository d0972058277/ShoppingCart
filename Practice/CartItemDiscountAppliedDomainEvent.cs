namespace Practice;

/// <summary>
/// 購物車項目已套用折扣的領域事件。
/// </summary>
public sealed record CartItemDiscountAppliedDomainEvent(
    Guid CartId,
    int ProductId,
    decimal DiscountPercentage
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
