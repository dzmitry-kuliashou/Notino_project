namespace Notino_project.Core.Exceptions;

public class AggregateAlreadyExistsException : Exception
{
    public AggregateAlreadyExistsException(Guid aggregateId)
        : this(aggregateId, $"An aggregate with id '{aggregateId}' already exists.")
    {
    }

    protected AggregateAlreadyExistsException(Guid aggregateId, string message)
        : base(message)
    {
        AggregateId = aggregateId;
    }

    public Guid AggregateId { get; }
}
