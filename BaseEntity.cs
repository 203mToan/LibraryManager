namespace MyApi
{
    public abstract class BaseEntity<T>
    {
        public T Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
