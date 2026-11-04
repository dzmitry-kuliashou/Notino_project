using Ardalis.GuardClauses;
using System.Collections.ObjectModel;

namespace Notino_project.Core.Aggregates
{
    public sealed class Document : AggregateRoot
    {
        private Document(List<string> tags, Dictionary<string, string> data)
        {
            _tags = tags;
            _data = data;
        }

        private List<string> _tags;
        private Dictionary<string, string> _data;

        public IReadOnlyList<string> Tags => Array.AsReadOnly([.. _tags]);

        public IReadOnlyDictionary<string, string> Data =>
            new ReadOnlyDictionary<string, string>(
                new Dictionary<string, string>(_data, StringComparer.Ordinal));

        public static Document Construct(IEnumerable<string> tags, Dictionary<string, string> data)
        {
            var (validatedTags, validatedData) = Validate(tags, data);
            return new Document(validatedTags, validatedData);
        }

        public void Update(IEnumerable<string> tags, Dictionary<string, string> data)
        {
            var (validatedTags, validatedData) = Validate(tags, data);
            _tags = validatedTags;
            _data = validatedData;
        }

        private static (List<string> Tags, Dictionary<string, string> Data) Validate(
            IEnumerable<string> tags,
            Dictionary<string, string> data)
        {
            var tagSnapshot = Guard.Against.Null(tags).ToList();
            Guard.Against.InvalidInput(
                tagSnapshot,
                nameof(tags),
                values => values.Count > 0,
                "Tags collection shouldn't be empty");
            var dataSnapshot = new Dictionary<string, string>(Guard.Against.Null(data), StringComparer.Ordinal);
            Guard.Against.InvalidInput(
                dataSnapshot,
                nameof(data),
                IsValidData,
                "Data must be a non-empty dictionary with string values");

            return (tagSnapshot, dataSnapshot);
        }

        private static bool IsValidData(Dictionary<string, string> data) =>
            data.Count > 0 && data.Values.All(value => value is not null);
    }
}