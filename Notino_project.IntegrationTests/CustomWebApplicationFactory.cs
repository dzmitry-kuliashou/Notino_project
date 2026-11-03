using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Notino_project.Core.Repositories;

namespace Notino_project.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
            services.RemoveAll<IDocumentsReadRepository>();
            services.RemoveAll<IDocumentsWriteRepository>();
            services.AddScoped<IDocumentsReadRepository, ControllerFormattersTests.DocumentsRepositoryStub>();
            services.AddScoped<IDocumentsWriteRepository, ControllerFormattersTests.DocumentsRepositoryStub>();
        });
    }
}
