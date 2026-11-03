using Microsoft.AspNetCore.Mvc;
using Moq;
using Notino_project.Application.Queries;
using Notino_project.Contract.Requests;
using Notino_project.Contract.Responses;
using Notino_project.Core.Aggregates;
using Notino_project.Core.Repositories;
using System.Text.Json;
using Xunit;

namespace Notino_project.UnitTests.ApplicationTests
{
    public class ListDocumentsQueryHandlerTests
    {
        [Fact]
        public async Task List_ReturnsMappedItemsAndPaginationMetadata()
        {
            var firstId = Guid.NewGuid();
            var secondId = Guid.NewGuid();
            var documents = new[]
            {
                CreateDocument(firstId, "first"),
                CreateDocument(secondId, "second")
            };
            var (handler, repository) = CreateHandler(documents, totalCount: 5);

            var result = await handler.Handle(
                new ListDocumentsQuery(CreateValidParams()),
                CancellationToken.None);

            var response = Assert.IsType<OkObjectResult>(result);
            var documentsResponse = Assert.IsType<DocumentsResponse>(response.Value);
            Assert.Equal(3, documentsResponse.TotalPageCount);
            Assert.Equal(1, documentsResponse.CurrentPage);
            Assert.Equal(2, documentsResponse.PageSize);
            Assert.True(documentsResponse.HasPrevious);
            Assert.True(documentsResponse.HasNext);
            Assert.Collection(
                documentsResponse.Items,
                item =>
                {
                    Assert.Equal(firstId, item.Id);
                    Assert.Equal("first", item.Data["name"]);
                },
                item =>
                {
                    Assert.Equal(secondId, item.Id);
                    Assert.Equal("second", item.Data["name"]);
                });
            repository.Verify(mock => mock.List(1, 2), Times.Once);
        }

        [Theory]
        [InlineData("invalid", "5", 0, 5)]
        [InlineData("2", "invalid", 2, 10)]
        [InlineData("-1", "0", 0, 10)]
        [InlineData("2147483647", "2", int.MaxValue / 2, 2)]
        public async Task List_InvalidParams_UsesDefaults(
            string page,
            string pageSize,
            int expectedPage,
            int expectedPageSize)
        {
            var parameters = new ListParams { Page = page, PageSize = pageSize };
            var (handler, repository) = CreateHandler();

            var result = await handler.Handle(
                new ListDocumentsQuery(parameters),
                CancellationToken.None);

            var response = Assert.IsType<OkObjectResult>(result);
            var documentsResponse = Assert.IsType<DocumentsResponse>(response.Value);
            Assert.Equal(expectedPage, documentsResponse.CurrentPage);
            Assert.Equal(expectedPageSize, documentsResponse.PageSize);
            repository.Verify(mock => mock.List(expectedPage, expectedPageSize), Times.Once);
        }

        [Fact]
        public async Task List_OutOfRangePage_ReturnsEmptyOkResponse()
        {
            var parameters = new ListParams { Page = "4", PageSize = "2" };
            var (handler, repository) = CreateHandler(totalCount: 5);

            var result = await handler.Handle(
                new ListDocumentsQuery(parameters),
                CancellationToken.None);

            var response = Assert.IsType<OkObjectResult>(result);
            var documentsResponse = Assert.IsType<DocumentsResponse>(response.Value);
            Assert.Empty(documentsResponse.Items);
            Assert.Equal(3, documentsResponse.TotalPageCount);
            Assert.Equal(4, documentsResponse.CurrentPage);
            Assert.Equal(2, documentsResponse.PageSize);
            Assert.True(documentsResponse.HasPrevious);
            Assert.False(documentsResponse.HasNext);
            repository.Verify(mock => mock.List(4, 2), Times.Once);
        }

        private static ListParams CreateValidParams() =>
            new() { Page = "1", PageSize = "2" };

        private static Document CreateDocument(Guid id, string name)
        {
            using var jsonDocument = JsonDocument.Parse(
                JsonSerializer.Serialize(new Dictionary<string, string> { ["name"] = name }));
            var document = Document.Construct(["test"], jsonDocument.RootElement.Clone());
            document.Id = id;
            return document;
        }

        private static (ListDocumentsQueryHandler Handler, Mock<IDocumentsReadRepository> Repository)
            CreateHandler(IReadOnlyList<Document>? documents = null, long totalCount = 0)
        {
            var repository = new Mock<IDocumentsReadRepository>();
            repository.Setup(mock => mock.List(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((Documents: documents ?? Array.Empty<Document>(), TotalCount: totalCount));

            return (new ListDocumentsQueryHandler(repository.Object), repository);
        }
    }
}
