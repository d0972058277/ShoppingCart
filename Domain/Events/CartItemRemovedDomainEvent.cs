using Domain.Primitives;

namespace Domain.Events;

/// <summary>
/// 購物車項目已移除的領域事件。
/// </summary>
public sealed record CartItemRemovedDomainEvent(
    Guid CartId,
    int ProductId
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
