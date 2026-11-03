namespace Notino_project.Core.Aggregates
{
    public abstract class AggregateRoot
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public bool IsDeleted { get; private set; }

        public void Delete()
        {
            IsDeleted = true;
        }
    }
}
