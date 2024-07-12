using MessagePack;
using MessagePack.AspNetCoreMvcFormatter;
using MessagePack.Resolvers;
using Microsoft.OpenApi.Models;
using Notino_project.FakeInMemoryRepo.Repositories;
using Notino_project.ModelBinders;
using Notino_project.Repositories.Interfaces.Repositories;
using Notino_project.Services.Interfaces.Services.Documents;
using Notino_project.Services.Services.Documents;
using Notino_project.SwaggerFilters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.RespectBrowserAcceptHeader = true;
    //options.ReturnHttpNotAcceptable = true;

    options.ModelBinderProviders.Add(new DynamicDataModelBinderProvider());

    // Add the MessagePack formatter
    var messagePackSerializerOptions = new MessagePackSerializerOptions(ContractlessStandardResolver.Instance);
    options.OutputFormatters.Add(new MessagePackOutputFormatter(messagePackSerializerOptions));
    options.InputFormatters.Add(new MessagePackInputFormatter(messagePackSerializerOptions));
})
.AddXmlDataContractSerializerFormatters()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
    options.OperationFilter<AddAcceptHeaderOperationFilter>();
});

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IDocumentsService, DocumentsService>();
builder.Services.AddScoped<IDocumentsRepository, FakeInMemoryDocumentsRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Urls.Add("http://*:80");
app.Urls.Add("http://*:8080");
app.Urls.Add("http://*:5000");

app.Run();

public partial class Program { }
