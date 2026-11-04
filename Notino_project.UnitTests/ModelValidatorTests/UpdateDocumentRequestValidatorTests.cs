using FluentValidation.Results;
using Notino_project.Application.Errors;
using Notino_project.Application.Validators;
using Notino_project.Contract.Requests;
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
            Assert.Equal(DocumentValidationErrors.TagsCollectionShouldNotBeEmpty, error.ErrorMessage);
        }

        [Fact]
        public void Validate_InvalidData_ReturnsValidationErrors()
        {
            var nullDataResult = ValidateData(null);
            Assert.Equal(
                DocumentValidationErrors.DataMustNotBeNull,
                Assert.Single(nullDataResult.Errors).ErrorMessage);

            var emptyDataResult = ValidateData(new Dictionary<string, string>());
            Assert.Equal(
                DocumentValidationErrors.DataMustHaveAtLeastOneProperty,
                Assert.Single(emptyDataResult.Errors).ErrorMessage);

            var nullValueResult = ValidateData(new Dictionary<string, string> { ["optional"] = null! });
            Assert.Equal(
                string.Format(DocumentValidationErrors.DataPropertyMustBeStringFormat, "optional"),
                Assert.Single(nullValueResult.Errors).ErrorMessage);
        }

        private ValidationResult ValidateData(Dictionary<string, string>? data)
        {
            var request = CreateValidRequest();
            request.Data = data!;

            return _validator.Validate(request);
        }

        private static UpdateDocumentRequest CreateValidRequest()
        {
            return new UpdateDocumentRequest
            {
                Tags = ["a"],
                Data = new Dictionary<string, string> { ["something"] = "This is data" },
            };
        }
    }
}
