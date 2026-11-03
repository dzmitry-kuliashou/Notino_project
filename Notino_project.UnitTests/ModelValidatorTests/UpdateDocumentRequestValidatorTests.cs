using FluentValidation.Results;
using Notino_project.Contract.Requests;
using Notino_project.Validators;
using System.Text.Json;
using Xunit;

namespace Notino_project.UnitTests.ModelValidatorTests
{
    public class UpdateDocumentRequestValidatorTests
    {
        private readonly UpdateDocumentRequestValidator _validator = new();

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
            Assert.Equal(nameof(UpdateDocumentRequest.Tags), error.PropertyName);
            Assert.Equal("Tags collection shouldn't be empty", error.ErrorMessage);
        }

        [Fact]
        public void Validate_InvalidData_ReturnsValidationErrors()
        {
            var nonObjectResult = ValidateData("42");
            Assert.Equal("The JsonElement must be an object.", Assert.Single(nonObjectResult.Errors).ErrorMessage);

            var emptyObjectResult = ValidateData("{}");
            Assert.Equal("The JsonElement must have at least one property", Assert.Single(emptyObjectResult.Errors).ErrorMessage);

            var nonStringValueResult = ValidateData("""{"optional":42}""");
            Assert.Equal("The property 'optional' is not a string type.", Assert.Single(nonStringValueResult.Errors).ErrorMessage);
        }

        private ValidationResult ValidateData(string json)
        {
            using var data = JsonDocument.Parse(json);
            var request = CreateValidRequest();
            request.Data = data.RootElement.Clone();

            return _validator.Validate(request);
        }

        private static UpdateDocumentRequest CreateValidRequest()
        {
            using var data = JsonDocument.Parse("""{"something":"This is data"}""");

            return new UpdateDocumentRequest
            {
                Tags = ["a"],
                Data = data.RootElement.Clone(),
            };
        }
    }
}
