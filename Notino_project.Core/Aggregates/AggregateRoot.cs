namespace Notino_project.Core.Aggregates
{
    public abstract class AggregateRoot
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}
