using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Notino_project.ModelBinders;
using MessagePack.Resolvers;
using MessagePack;
using MessagePack.AspNetCoreMvcFormatter;
using Notino_project.Services.Interfaces.Services.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Notino_project.IntegrationTests.ControllerFormattersTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddScoped<IDocumentsService, DocumentsServiceStub>();
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

            var response = await client.GetAsync($"documents/{DocumentsServiceStub.ExistingDocumentId}");

            response.EnsureSuccessStatusCode();
            Assert.AreEqual(acceptHeader, response.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task GetById_AcceptHeaderIsNotSupported_ShouldReturnApplicationJsonContentType()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("Accept", "application/javascript");

            var response = await client.GetAsync($"documents/{DocumentsServiceStub.ExistingDocumentId}");

            response.EnsureSuccessStatusCode();
            Assert.AreEqual("application/json", response.Content.Headers.ContentType.MediaType);
        }
    }
}
