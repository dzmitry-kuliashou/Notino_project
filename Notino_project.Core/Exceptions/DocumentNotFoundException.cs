namespace Notino_project.Core.Exceptions;

public sealed class DocumentNotFoundException(Guid documentId)
    : Exception($"A document with id '{documentId}' was not found.");