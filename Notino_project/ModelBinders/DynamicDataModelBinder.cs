using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;

namespace Notino_project.ModelBinders
{
    public class DynamicDataModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var jsonElement = bindingContext.ValueProvider.GetValue("Data").FirstValue;
            if (string.IsNullOrEmpty(jsonElement))
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            try
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonElement, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                bindingContext.Result = ModelBindingResult.Success(data);
            }
            catch (JsonException)
            {
                bindingContext.Result = ModelBindingResult.Failed();
            }

            return Task.CompletedTask;
        }
    }

    public class DynamicDataModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(Dictionary<string, string>))
            {
                return new BinderTypeModelBinder(typeof(DynamicDataModelBinder));
            }

            return null;
        }
    }
}
