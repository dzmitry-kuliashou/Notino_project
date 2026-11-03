namespace Notino_project.Application.Caching
{
    public static class DocumentCachePolicy
    {
        public static readonly TimeSpan AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan SlidingExpiration = TimeSpan.FromMinutes(2);
    }
}
