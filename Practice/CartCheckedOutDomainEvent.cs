namespace Practice;

/// <summary>
/// 購物車已結帳的領域事件。
/// </summary>
public sealed record CartCheckedOutDomainEvent(
    Guid CartId,
    decimal TotalPrice,
    int ItemCount
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
