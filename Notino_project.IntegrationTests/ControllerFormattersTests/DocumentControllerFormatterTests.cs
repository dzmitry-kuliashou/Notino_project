using Xunit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
}