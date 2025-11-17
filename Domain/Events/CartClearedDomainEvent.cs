using Domain.Primitives;

namespace Domain.Events;

/// <summary>
/// 購物車已清空的領域事件。
/// </summary>
public sealed record CartClearedDomainEvent(
    Guid CartId
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
