using Notino_project.Application.Errors;
using System.Text.Json;

namespace Notino_project.Application.Validators
{
    internal static class DocumentDataValidation
    {
        public static void Validate(JsonElement data, Action<string> addFailure)
        {
            if (data.ValueKind != JsonValueKind.Object)
            {
                addFailure(DocumentValidationErrors.JsonElementMustBeObject);
                return;
            }

            var hasProperties = false;
            foreach (var property in data.EnumerateObject())
            {
                hasProperties = true;
                if (property.Value.ValueKind != JsonValueKind.String)
                {
                    addFailure(string.Format(
                        DocumentValidationErrors.JsonPropertyMustBeStringFormat,
                        property.Name));
                }
            }

            if (!hasProperties)
            {
                addFailure(DocumentValidationErrors.JsonElementMustHaveAtLeastOneProperty);
            }
        }
    }
}
