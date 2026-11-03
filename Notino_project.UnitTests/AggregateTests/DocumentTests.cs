using Notino_project.Core.Aggregates;
using System.Text.Json;
using Xunit;

namespace Notino_project.UnitTests.AggregateTests
{
    public class DocumentTests
    {
        private static IEnumerable<string> ValidTags() => ["tag"];

        private static JsonElement ValidData()
        {
            using var document = JsonDocument.Parse("""{"name":"value"}""");
            return document.RootElement.Clone();
        }

        [Fact]
        public void Construct_ValidInputs_CreatesDocumentWithGeneratedIdAndProperties()
        {
            var tags = ValidTags();
            var data = ValidData();

            var document = Document.Construct(tags, data);

            Assert.NotEqual(Guid.Empty, document.Id);
            Assert.Equal(tags, document.Tags);
            Assert.Equal("value", document.Data.GetProperty("name").GetString());
            Assert.False(document.IsDeleted);
        }

        [Fact]
        public void Delete_ActiveDocument_MarksItDeleted()
        {
            var document = Document.Construct(ValidTags(), ValidData());

            Assert.False(document.IsDeleted);

            document.Delete();

            Assert.True(document.IsDeleted);
        }

        [Fact]
        public void Construct_EmptyTags_ThrowsArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(() => Document.Construct([], ValidData()));

            Assert.Equal("tags", exception.ParamName);
        }

        [Fact]
        public void Construct_NonObjectData_ThrowsArgumentException()
        {
            using var jsonDocument = JsonDocument.Parse("""["value"]""");
            var data = jsonDocument.RootElement.Clone();

            var exception = Assert.Throws<ArgumentException>(() => Document.Construct(ValidTags(), data));

            Assert.Equal("data", exception.ParamName);
        }

        [Fact]
        public void Construct_EmptyObjectData_ThrowsArgumentException()
        {
            using var jsonDocument = JsonDocument.Parse("{}");
            var data = jsonDocument.RootElement.Clone();

            var exception = Assert.Throws<ArgumentException>(() => Document.Construct(ValidTags(), data));

            Assert.Equal("data", exception.ParamName);
        }

        [Fact]
        public void Construct_NonStringValues_ThrowsArgumentException()
        {
            using var jsonDocument = JsonDocument.Parse("""{"name":1}""");
            var data = jsonDocument.RootElement.Clone();

            var exception = Assert.Throws<ArgumentException>(() => Document.Construct(ValidTags(), data));

            Assert.Equal("data", exception.ParamName);
        }

        [Fact]
        public void Update_ValidInputs_ChangesTagsAndDataAndPreservesId()
        {
            var document = Document.Construct(ValidTags(), ValidData());
            var id = document.Id;
            var updatedTags = new[] { "updated" };

            using var jsonDocument = JsonDocument.Parse("""{"updated":"new-value"}""");
            var updatedData = jsonDocument.RootElement.Clone();

            document.Update(updatedTags, updatedData);

            Assert.Equal(id, document.Id);
            Assert.Equal(updatedTags, document.Tags);
            Assert.Equal("new-value", document.Data.GetProperty("updated").GetString());
        }

        [Fact]
        public void Update_EmptyTags_ThrowsArgumentExceptionAndPreservesState()
        {
            var document = Document.Construct(ValidTags(), ValidData());
            var id = document.Id;

            var exception = Assert.Throws<ArgumentException>(() => document.Update([], ValidData()));

            Assert.Equal("tags", exception.ParamName);
            Assert.Equal(id, document.Id);
            Assert.Equal(ValidTags(), document.Tags);
            Assert.Equal("value", document.Data.GetProperty("name").GetString());
        }

        [Fact]
        public void Update_InvalidData_ThrowsArgumentExceptionAndPreservesState()
        {
            var document = Document.Construct(ValidTags(), ValidData());
            var id = document.Id;

            using var jsonDocument = JsonDocument.Parse("""{"name":1}""");
            var invalidData = jsonDocument.RootElement.Clone();

            var exception = Assert.Throws<ArgumentException>(() => document.Update(["updated"], invalidData));

            Assert.Equal("data", exception.ParamName);
            Assert.Equal(id, document.Id);
            Assert.Equal(ValidTags(), document.Tags);
            Assert.Equal("value", document.Data.GetProperty("name").GetString());
        }
    }
}
