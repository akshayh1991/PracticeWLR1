using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SecMan.Model
{
    public class CustomIntBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            string value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;

            if (int.TryParse(value, out int result))
            {
                bindingContext.Result = ModelBindingResult.Success(result);
            }
            else if (string.IsNullOrEmpty(value))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
            }
            else
            {
                bindingContext.ModelState.AddModelError($"{char.ToLower(bindingContext.ModelName[0])}{bindingContext.ModelName[1..]}", $"Invalid {char.ToLower(bindingContext.ModelName[0])}{bindingContext.ModelName[1..]} value");
            }

            return Task.CompletedTask;
        }
    }
}
