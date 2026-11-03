using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;
using Notino_project.Application.Errors;
using Notino_project.Core.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http.Json;

namespace Notino_project.IntegrationTests.ControllerFormattersTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.RemoveAll<IDocumentsReadRepository>();
                services.RemoveAll<IDocumentsWriteRepository>();
                services.AddScoped<IDocumentsReadRepository, DocumentsRepositoryStub>();
                services.AddScoped<IDocumentsWriteRepository, DocumentsRepositoryStub>();
            });
        }
    }

    public class DocumentsFormattersTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public DocumentsFormattersTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("application/json")]
        [InlineData("application/xml")]
        [InlineData("text/xml")]
        [InlineData("application/x-msgpack")]
        public async Task GetById_ShouldReturnCorrectContentType(string acceptHeader)
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("Accept", acceptHeader);

            var response = await client.GetAsync($"documents/{DocumentsRepositoryStub.ExistingDocumentId}");

            response.EnsureSuccessStatusCode();
            Assert.AreEqual(acceptHeader, response.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task GetById_AcceptHeaderIsNotSupported_ShouldReturnApplicationJsonContentType()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("Accept", "application/javascript");

            var response = await client.GetAsync($"documents/{DocumentsRepositoryStub.ExistingDocumentId}");

            response.EnsureSuccessStatusCode();
            Assert.AreEqual("application/json", response.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task Create_InvalidRequest_ReturnsAllValidationErrors()
        {
            var client = _factory.CreateClient();
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
    }
}
