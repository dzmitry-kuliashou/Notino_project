namespace Notino_project.Application.Errors
{
    public static class DocumentValidationErrors
    {
        public const string TagsCollectionShouldNotBeEmpty = "Tags collection shouldn't be empty";
        public const string JsonElementMustBeObject = "The JsonElement must be an object.";
        public const string JsonElementMustHaveAtLeastOneProperty = "The JsonElement must have at least one property";
        public const string JsonPropertyMustBeStringFormat = "The property '{0}' is not a string type.";
    }
}
