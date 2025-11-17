namespace Practice;

/// <summary>
/// Event Sourcing 聚合根基類。
/// 透過事件重建狀態，事件為唯一真相來源。
/// </summary>
public abstract class EventSourcedAggregateRoot<TId> : AggregateRoot<TId>
    where TId : notnull, IComparable<TId>
{
    /// <summary>
    /// 版本號（用於樂觀並發控制）。
    /// </summary>
    public int Version { get; private set; }

    protected EventSourcedAggregateRoot(TId id) : base(id)
    {
        Version = -1; // 新建 Aggregate，尚未持久化
    }

    /// <summary>
    /// 應用事件並新增到未提交事件列表（用於建立新事件）。
    /// </summary>
    /// <param name="domainEvent">領域事件。</param>
    protected void RaiseEvent(IDomainEvent domainEvent)
    {
        Apply(domainEvent);          // 修改內部狀態
        Version++;                   // 遞增版本號
        AddDomainEvent(domainEvent); // 加入待發布列表
    }

    /// <summary>
    /// 從歷史事件重建狀態（用於事件溯源）。
    /// </summary>
    /// <param name="domainEvents">歷史事件序列。</param>
    public void Load(IEnumerable<IDomainEvent> domainEvents)
    {
        foreach (var @event in domainEvents)
        {
            Apply(@event);          // 修改內部狀態
            Version++;              // 遞增版本號
        }
    }

    /// <summary>
    /// 應用事件修改狀態（子類實作）。
    /// ⚠️ Apply 方法只修改狀態，不做驗證、不做計算。
    /// </summary>
    /// <param name="domainEvent">領域事件。</param>
    protected abstract void Apply(IDomainEvent domainEvent);
}