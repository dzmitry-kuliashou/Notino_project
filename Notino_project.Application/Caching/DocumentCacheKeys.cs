namespace Notino_project.Application.Caching
{
    public static class DocumentCacheKeys
    {
        public static string For(Guid documentId) => $"documents_{documentId}";
    }
}
