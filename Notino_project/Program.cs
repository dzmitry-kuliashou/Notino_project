using MessagePack;
using MessagePack.AspNetCoreMvcFormatter;
using MessagePack.Resolvers;
using Microsoft.OpenApi.Models;
using Notino_project.Application.Queries;
using Notino_project.Application.Validators;
using Notino_project.Contract.Examples;
using Notino_project.Infrastructure;
using Notino_project.Infrastructure.Repositories;
using FluentValidation;
using Swashbuckle.AspNetCore.Filters;
using MongoDocument = Notino_project.Infrastructure.StorageModels.Document;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.RespectBrowserAcceptHeader = true;
    //options.ReturnHttpNotAcceptable = true;

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
builder.Services.AddSwaggerExamplesFromAssemblyOf<CreateDocumentExample>();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
    options.ExampleFilters();
});

builder.Services.AddMemoryCache();

var mongoSettings = builder.Configuration
    .GetSection(MongoDbSettings.SectionName)
    .Get<MongoDbSettings>()
    ?? throw new InvalidOperationException("MongoDB settings are missing.");
builder.Services.AddMongoDb<MongoDocument, DocumentsReadRepository, DocumentsWriteRepository>(mongoSettings);

builder.Services.AddMediatR(configuration =>
    configuration.RegisterServicesFromAssembly(typeof(GetByIdQuery).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(CreateDocumentRequestValidator).Assembly);

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
