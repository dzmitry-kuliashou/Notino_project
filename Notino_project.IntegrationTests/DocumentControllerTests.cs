using Microsoft.VisualStudio.TestTools.UnitTesting;
using Notino_project.Application.Errors;
using Notino_project.Contract.Responses;
using Notino_project.IntegrationTests.ControllerFormattersTests;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Notino_project.IntegrationTests;

public sealed class DocumentControllerTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task List_ExplicitQueryValues_BindsAndReturnsPageMetadata()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("documents?p=2&ps=3");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var documentsResponse = await response.Content.ReadFromJsonAsync<DocumentsResponse>()
            ?? throw new InvalidOperationException("Expected a JSON documents response.");
        Assert.AreEqual(2, documentsResponse.CurrentPage);
        Assert.AreEqual(3, documentsResponse.PageSize);
    }

    [Fact]
    public async Task List_OmittedQueryValues_UsesDefaults()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("documents");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var documentsResponse = await response.Content.ReadFromJsonAsync<DocumentsResponse>()
            ?? throw new InvalidOperationException("Expected a JSON documents response.");
        Assert.AreEqual(0, documentsResponse.CurrentPage);
        Assert.AreEqual(10, documentsResponse.PageSize);
    }

    [Fact]
    public async Task Create_InvalidRequest_ReturnsAllValidationErrors()
    {
        var client = factory.CreateClient();
        var request = new
        {
            Tags = Array.Empty<string>(),
            Data = new { },
        };

        var response = await client.PostAsJsonAsync("documents", request);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await response.Content.ReadFromJsonAsync<string[]>()
            ?? throw new InvalidOperationException("Expected a JSON string array of validation errors.");
        CollectionAssert.AreEquivalent(
            new[]
            {
                DocumentValidationErrors.TagsCollectionShouldNotBeEmpty,
                DocumentValidationErrors.JsonElementMustHaveAtLeastOneProperty,
            },
            errors);
    }

    [Fact]
    public async Task Delete_ExistingDocument_ReturnsNoContent()
    {
        var client = factory.CreateClient();

        var response = await client.DeleteAsync($"documents/{DocumentsRepositoryStub.ExistingDocumentId}");

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_UnknownDocument_ReturnsNotFound()
    {
        var client = factory.CreateClient();

        var response = await client.DeleteAsync($"documents/{Guid.NewGuid()}");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
