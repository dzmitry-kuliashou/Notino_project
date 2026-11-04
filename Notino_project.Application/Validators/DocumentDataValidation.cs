using Notino_project.Application.Errors;

namespace Notino_project.Application.Validators
{
    internal static class DocumentDataValidation
    {
        public static void Validate(Dictionary<string, string>? data, Action<string> addFailure)
        {
            if (data is null)
            {
                addFailure(DocumentValidationErrors.DataMustNotBeNull);
                return;
            }

            if (data.Count == 0)
            {
                addFailure(DocumentValidationErrors.DataMustHaveAtLeastOneProperty);
            }

            foreach (var (key, value) in data)
            {
                if (value is null)
                {
                    addFailure(string.Format(
                        DocumentValidationErrors.DataPropertyMustBeStringFormat,
                        key));
                }
            }
        }
    }
}
