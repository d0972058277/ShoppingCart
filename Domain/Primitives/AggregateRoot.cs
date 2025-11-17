using CSharpFunctionalExtensions;

namespace Domain.Primitives;

/// <summary>
/// 所有聚合根的基底類別。
/// 聚合是一致性邊界，負責管理領域事件並確保業務不變量。
/// </summary>
/// <typeparam name="TId">聚合根識別碼的型別。</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull, IComparable<TId>
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// 初始化 <see cref="AggregateRoot{TId}"/> 類別的新執行個體。
    /// </summary>
    /// <param name="id">聚合根的唯一識別碼。</param>
    protected AggregateRoot(TId id) : base(id)
    {
    }

    /// <summary>
    /// 將領域事件加入聚合的事件集合。
    /// 這些事件將在聚合持久化後發布。
    /// </summary>
    /// <param name="domainEvent">要加入的領域事件。</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// 取得所有領域事件，但不清空內部集合。
    /// 此方法用於檢視當前的領域事件，不會影響事件集合的狀態。
    /// </summary>
    /// <returns>領域事件的唯讀集合。</returns>
    public IReadOnlyCollection<IDomainEvent> GetDomainEvents()
    {
        return _domainEvents.ToList().AsReadOnly();
    }

    /// <summary>
    /// 取得所有領域事件並清空內部集合。
    /// 此方法應在聚合持久化後呼叫以發布事件。
    /// </summary>
    /// <returns>領域事件的唯讀集合。</returns>
    public IReadOnlyCollection<IDomainEvent> DrainDomainEvents()
    {
        var events = _domainEvents.ToList();
        _domainEvents.Clear();
        return events;
    }
}
