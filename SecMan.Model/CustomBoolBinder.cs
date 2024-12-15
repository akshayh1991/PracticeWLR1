using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SecMan.Model
{
    public class CustomBoolBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            string value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;

            if (bool.TryParse(value, out bool result))
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
