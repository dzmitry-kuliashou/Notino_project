using FluentValidation.Results;
using Notino_project.Application.Errors;
using Notino_project.Application.Validators;
using Notino_project.Contract.Requests;
using System.Text.Json;
using Xunit;

namespace Notino_project.UnitTests.ModelValidatorTests
{
    public class CreateDocumentRequestValidatorTests
    {
        private readonly CreateDocumentRequestValidator _validator = new();

        [Fact]
        public void Validate_ValidRequest_ReturnsNoValidationErrors()
        {
            var request = CreateValidRequest();

            var result = _validator.Validate(request);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Validate_EmptyTags_ReturnsTagsValidationError()
        {
            var request = CreateValidRequest();
            request.Tags = [];

            var result = _validator.Validate(request);

            var error = Assert.Single(result.Errors);
            Assert.Equal(nameof(CreateDocumentRequest.Tags), error.PropertyName);
            Assert.Equal(DocumentValidationErrors.TagsCollectionShouldNotBeEmpty, error.ErrorMessage);
        }

        [Fact]
        public void Validate_InvalidData_ReturnsValidationErrors()
        {
            var nonObjectResult = ValidateData("42");
            Assert.Equal(
                DocumentValidationErrors.JsonElementMustBeObject,
                Assert.Single(nonObjectResult.Errors).ErrorMessage);

            var emptyObjectResult = ValidateData("{}");
            Assert.Equal(
                DocumentValidationErrors.JsonElementMustHaveAtLeastOneProperty,
                Assert.Single(emptyObjectResult.Errors).ErrorMessage);

            var nonStringValueResult = ValidateData("""{"optional":42}""");
            Assert.Equal(
                string.Format(DocumentValidationErrors.JsonPropertyMustBeStringFormat, "optional"),
                Assert.Single(nonStringValueResult.Errors).ErrorMessage);
        }

        private ValidationResult ValidateData(string json)
        {
            using var data = JsonDocument.Parse(json);
            var request = CreateValidRequest();
            request.Data = data.RootElement.Clone();

            return _validator.Validate(request);
        }

        private static CreateDocumentRequest CreateValidRequest()
        {
            using var data = JsonDocument.Parse("""{"something":"This is data"}""");

            return new CreateDocumentRequest
            {
                Tags = ["a"],
                Data = data.RootElement.Clone(),
            };
        }
    }
}
