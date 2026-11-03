using FluentValidation;
using Notino_project.Application.Errors;
using Notino_project.Contract.Requests;

namespace Notino_project.Application.Validators
{
    public class UpdateDocumentRequestValidator : AbstractValidator<UpdateDocumentRequest>
    {
        public UpdateDocumentRequestValidator()
        {
            RuleFor(request => request.Tags)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithMessage(DocumentValidationErrors.TagsCollectionShouldNotBeEmpty)
                .NotEmpty()
                .WithMessage(DocumentValidationErrors.TagsCollectionShouldNotBeEmpty);

            RuleFor(request => request.Data).Custom((data, context) =>
                DocumentDataValidation.Validate(data, message => context.AddFailure(message)));
        }
    }
}
