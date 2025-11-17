namespace Domain.Primitives;

/// <summary>
/// 代表系統中發生的領域事件。
/// 領域事件用於在聚合和限界上下文之間傳遞變更。
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// 取得領域事件發生的 UTC 時間戳記。
    /// </summary>
    DateTime OccurredOn { get; }
}
