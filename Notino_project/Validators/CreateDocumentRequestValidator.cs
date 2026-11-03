using FluentValidation;
using System.Text.Json;
using Notino_project.Contract.Requests;

namespace Notino_project.Validators
{
    public class CreateDocumentRequestValidator : AbstractValidator<CreateDocumentRequest>
    {
        public CreateDocumentRequestValidator()
        {
            RuleFor(request => request.Tags)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithMessage("Tags collection shouldn't be empty")
                .NotEmpty()
                .WithMessage("Tags collection shouldn't be empty");

            RuleFor(request => request.Data).Custom((data, context) =>
            {
                if (data.ValueKind != JsonValueKind.Object)
                {
                    context.AddFailure("The JsonElement must be an object.");
                    return;
                }

                var hasProperties = false;
                foreach (var property in data.EnumerateObject())
                {
                    hasProperties = true;
                    if (property.Value.ValueKind != JsonValueKind.String)
                    {
                        context.AddFailure($"The property '{property.Name}' is not a string type.");
                    }
                }

                if (!hasProperties)
                {
                    context.AddFailure("The JsonElement must have at least one property");
                }
            });
        }
    }
}
