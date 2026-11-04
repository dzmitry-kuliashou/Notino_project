using Notino_project.Core.Aggregates;
using Xunit;

namespace Notino_project.UnitTests.AggregateTests
{
    public class DocumentTests
    {
        private static IEnumerable<string> ValidTags() => ["tag"];

        private static Dictionary<string, string> ValidData() =>
            new() { ["name"] = "value" };

        [Fact]
        public void Construct_ValidInputs_CreatesDocumentWithGeneratedIdAndProperties()
        {
            var tags = ValidTags();
            var data = ValidData();

            var document = Document.Construct(tags, data);

            Assert.NotEqual(Guid.Empty, document.Id);
            Assert.Equal(tags, document.Tags);
            Assert.Equal("value", document.Data["name"]);
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
        public void Construct_NullData_ThrowsArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => Document.Construct(ValidTags(), null!));

            Assert.Equal("data", exception.ParamName);
        }

        [Fact]
        public void Construct_EmptyObjectData_ThrowsArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(
                () => Document.Construct(ValidTags(), new Dictionary<string, string>()));

            Assert.Equal("data", exception.ParamName);
        }

        [Fact]
        public void Construct_NullValues_ThrowsArgumentException()
        {
            var data = new Dictionary<string, string> { ["name"] = null! };

            var exception = Assert.Throws<ArgumentException>(() => Document.Construct(ValidTags(), data));

            Assert.Equal("data", exception.ParamName);
        }

        [Fact]
        public void Update_ValidInputs_ChangesTagsAndDataAndPreservesId()
        {
            var document = Document.Construct(ValidTags(), ValidData());
            var id = document.Id;
            var updatedTags = new[] { "updated" };

            var updatedData = new Dictionary<string, string> { ["updated"] = "new-value" };

            document.Update(updatedTags, updatedData);

            Assert.Equal(id, document.Id);
            Assert.Equal(updatedTags, document.Tags);
            Assert.Equal("new-value", document.Data["updated"]);
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
            Assert.Equal("value", document.Data["name"]);
        }

        [Fact]
        public void Update_InvalidData_ThrowsArgumentExceptionAndPreservesState()
        {
            var document = Document.Construct(ValidTags(), ValidData());
            var id = document.Id;

            var invalidData = new Dictionary<string, string> { ["name"] = null! };

            var exception = Assert.Throws<ArgumentException>(() => document.Update(["updated"], invalidData));

            Assert.Equal("data", exception.ParamName);
            Assert.Equal(id, document.Id);
            Assert.Equal(ValidTags(), document.Tags);
            Assert.Equal("value", document.Data["name"]);
        }

        [Fact]
        public void Construct_DataIsCopiedToPreserveAggregateInvariants()
        {
            var data = ValidData();
            var document = Document.Construct(ValidTags(), data);
            data["name"] = "changed";

            Assert.Equal("value", document.Data["name"]);
        }

        [Fact]
        public void Data_CannotBeMutatedThroughExposedCollection()
        {
            var document = Document.Construct(ValidTags(), ValidData());
            var data = Assert.IsAssignableFrom<IDictionary<string, string>>(document.Data);

            Assert.Throws<NotSupportedException>(() => data["name"] = "changed");
            Assert.Equal("value", document.Data["name"]);
        }

        [Fact]
        public void Tags_CannotBeMutatedThroughExposedCollection()
        {
            var document = Document.Construct(ValidTags(), ValidData());
            var tags = Assert.IsAssignableFrom<IList<string>>(document.Tags);

            Assert.Throws<NotSupportedException>(() => tags[0] = "changed");
            Assert.Equal("tag", document.Tags.Single());
        }
    }
}
