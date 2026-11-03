using Ardalis.GuardClauses;
using System.Text.Json;

namespace Notino_project.Core.Aggregates
{
    public sealed class Document : AggregateRoot
    {
        private Document(IEnumerable<string> tags, JsonElement data)
        {
            Tags = tags;
            Data = data;
        }

        public IEnumerable<string> Tags { get; private set; }

        public JsonElement Data { get; private set; }

        public static Document Construct(IEnumerable<string> tags, JsonElement data)
        {
            var validatedTags = Validate(tags, data);
            return new Document(validatedTags, data);
        }

        public void Update(IEnumerable<string> tags, JsonElement data)
        {
            var validatedTags = Validate(tags, data);
            Tags = validatedTags;
            Data = data;
        }

        private static string[] Validate(IEnumerable<string> tags, JsonElement data)
        {
            var tagSnapshot = Guard.Against.Null(tags).ToArray();
            Guard.Against.InvalidInput(
                tagSnapshot,
                nameof(tags),
                values => values.Length > 0,
                "Tags collection shouldn't be empty");
            Guard.Against.InvalidInput(
                data,
                nameof(data),
                IsSimplePropertiesOnly,
                "Data must be a non-empty JSON object with string values");

            return tagSnapshot;
        }

        private static bool IsSimplePropertiesOnly(JsonElement data)
        {
            if (data.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            var hasAnyProperty = false;
            foreach (var property in data.EnumerateObject())
            {
                if (property.Value.ValueKind != JsonValueKind.String)
                {
                    return false;
                }

                hasAnyProperty = true;
            }

            return hasAnyProperty;
        }
    }
}