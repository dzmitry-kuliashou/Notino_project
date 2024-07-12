using Notino_project.Dtos.Document;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Xunit;

namespace Notino_project.UnitTests.ModelValidatorTests
{
    //We can similarly test UpdateDocumentDto model
    public class CreateDocumentDtoValidationTests
    {
        [Fact]
        public void TagsPropertyIsEmpty_ReturnsValidationError()
        {
            var jsonInput = @"{
                        ""data"": 
                        {
                            ""something"": ""This is data""
                        }
                    }";

            var doc = JsonDocument.Parse(jsonInput);
            var root = doc.RootElement;
            var data = root.GetProperty("data");

            var createDocumentModel = new CreateDocumentDto
            {
                Tags = [],
                Data = data
            };

            var validationResults = ValidateModel(createDocumentModel);

            Assert.Single(validationResults);
            Assert.Equal("Tags collection shouldn't be empty", validationResults[0].ErrorMessage);
        }

        //We can similarly test other invalid states of "Data" property
        [Fact]
        public void DataPropertyContainsInvalidProperties_ReturnValidationError()
        {
            var jsonInput = @"{
                        ""data"": 
                        {
                            ""something"": ""This is data"",
                            ""optional"": 42
                        }
                    }";

            var doc = JsonDocument.Parse(jsonInput);
            var root = doc.RootElement;
            var data = root.GetProperty("data");

            var createDocumentModel = new CreateDocumentDto
            {
                Tags = ["a"],
                Data = data
            };

            var validationResults = ValidateModel(createDocumentModel);

            Assert.Single(validationResults);
            Assert.Equal("The property 'optional' is not a string type.", validationResults[0].ErrorMessage);
        }

        //Test another invalid model's states

        [Fact]
        public void ModelIsValid_ShouldReturnNoValidationResults()
        {
            var jsonInput = @"{
                        ""data"": 
                        {
                            ""something"": ""This is data""
                        }
                    }";

            var doc = JsonDocument.Parse(jsonInput);
            var root = doc.RootElement;
            var data = root.GetProperty("data");

            var createDocumentModel = new CreateDocumentDto
            {
                Tags = ["a"],
                Data = data
            };

            var validationResults = ValidateModel(createDocumentModel);

            Assert.Empty(validationResults);
        }

        private List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model, serviceProvider: null, items: null);
            Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);
            return validationResults;
        }
    }
}
