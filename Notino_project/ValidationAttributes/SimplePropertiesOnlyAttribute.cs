using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Notino_project.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class SimplePropertiesOnlyAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var hasAnyValidAttribute = false;

            if (value is not JsonElement jsonElement)
            {
                return new ValidationResult("The value must be a JsonElement.");
            }

            if (jsonElement.ValueKind != JsonValueKind.Object)
            {
                return new ValidationResult("The JsonElement must be an object.");
            }

            foreach (JsonProperty property in jsonElement.EnumerateObject())
            {
                if (!IsSimpleJsonValue(property.Value))
                {
                    return new ValidationResult($"The property '{property.Name}' is not a string type.");
                }

                hasAnyValidAttribute = true;
            }

            if (!hasAnyValidAttribute)
            {
                return new ValidationResult($"The JsonElement must have at least one property");
            }

            return ValidationResult.Success;
        }

        private bool IsSimpleJsonValue(JsonElement jsonElement)
        {
            return jsonElement.ValueKind switch
            {
                JsonValueKind.String => true,
                _ => false,
            };
        }
    }
}
