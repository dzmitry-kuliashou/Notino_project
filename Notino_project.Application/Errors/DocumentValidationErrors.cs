namespace Notino_project.Application.Errors
{
    public static class DocumentValidationErrors
    {
        public const string TagsCollectionShouldNotBeEmpty = "Tags collection shouldn't be empty";
        public const string DataMustNotBeNull = "Data must not be null.";
        public const string DataMustHaveAtLeastOneProperty = "Data must have at least one property.";
        public const string DataPropertyMustBeStringFormat = "The property '{0}' must have a string value.";
    }
}
