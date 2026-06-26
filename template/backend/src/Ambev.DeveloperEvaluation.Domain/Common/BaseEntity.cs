namespace Ambev.DeveloperEvaluation.Domain.Common;

public class BaseEntity<TId> : IComparable<BaseEntity<TId>>
    where TId : struct, IComparable<TId>
{
    public TId Id { get; set; }

    public int CompareTo(BaseEntity<TId>? other)
    {
        if (other == null)
        {
            return 1;
        }

        return other.Id.CompareTo(Id);
    }
}
