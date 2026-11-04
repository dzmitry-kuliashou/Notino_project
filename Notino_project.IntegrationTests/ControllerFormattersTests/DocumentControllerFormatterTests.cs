using Xunit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace Notino_project.IntegrationTests.ControllerFormattersTests;

public class DocumentsFormattersTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Theory]
    [InlineData("application/json")]
    [InlineData("application/xml")]
    [InlineData("text/xml")]
    [InlineData("application/x-msgpack")]
    public async Task GetById_ShouldReturnCorrectContentType(string acceptHeader)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Accept", acceptHeader);

        var response = await client.GetAsync($"documents/{DocumentsRepositoryStub.ExistingDocumentId}");

        response.EnsureSuccessStatusCode();
        Assert.AreEqual(acceptHeader, response.Content.Headers.ContentType.MediaType);
    }

    [Fact]
    public async Task GetById_AcceptHeaderIsNotSupported_ShouldReturnApplicationJsonContentType()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Accept", "application/javascript");

        var response = await client.GetAsync($"documents/{DocumentsRepositoryStub.ExistingDocumentId}");

        response.EnsureSuccessStatusCode();
        Assert.AreEqual("application/json", response.Content.Headers.ContentType.MediaType);
    }

    [Fact]
    public async Task Swagger_GetAndWriteOperations_ShouldExposeSupportedResponseContentTypes()
    {
        var client = factory.CreateClient();
        using var document = JsonDocument.Parse(await client.GetStringAsync("/swagger/v1/swagger.json"));
        var paths = document.RootElement.GetProperty("paths");
        var expectedOperations = new[]
        {
            (Path: "/documents", Method: "get", Status: "200"),
            (Path: "/documents/{id}", Method: "get", Status: "200"),
            (Path: "/documents", Method: "post", Status: "201"),
            (Path: "/documents/{id}", Method: "put", Status: "200")
        };
        var supportedContentTypes = new[]
        {
            "application/json",
            "application/xml",
            "text/xml",
            "application/x-msgpack"
        };

        foreach (var (path, method, status) in expectedOperations)
        {
            var operation = paths.GetProperty(path).GetProperty(method);
            var content = operation.GetProperty("responses").GetProperty(status).GetProperty("content");

            foreach (var contentType in supportedContentTypes)
            {
                Assert.IsTrue(content.TryGetProperty(contentType, out _), $"{method.ToUpperInvariant()} {path}: {contentType}");
            }

            Assert.IsFalse(operation.TryGetProperty("parameters", out var parameters) &&
                            parameters.EnumerateArray().Any(parameter =>
                                parameter.GetProperty("name").GetString() == "Accept"));
        }

        var deleteResponses = paths.GetProperty("/documents/{id}").GetProperty("delete").GetProperty("responses");
        Assert.IsTrue(deleteResponses.TryGetProperty("204", out _));
        Assert.IsTrue(deleteResponses.TryGetProperty("404", out _));
    }
}