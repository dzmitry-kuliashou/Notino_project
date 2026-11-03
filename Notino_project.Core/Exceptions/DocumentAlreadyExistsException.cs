namespace Notino_project.Core.Exceptions;

public sealed class DocumentAlreadyExistsException(Guid documentId)
    : Exception($"A document with id '{documentId}' already exists.");