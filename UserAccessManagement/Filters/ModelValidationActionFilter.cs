using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SecMan.Model;
using System.Collections.Immutable;

namespace UserAccessManagement.Filters
{
    /// <summary>
    /// 
    /// </summary>
    public class ModelValidationActionFilter : IActionFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
            context.HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            if (context.Result is ObjectResult objectResult && objectResult.Value is CommonResponse commonResponse)
            {
                commonResponse.Type = ResponseConstants.GetTypeUrl(context.HttpContext, commonResponse.Type);

                context.Result = new ObjectResult(commonResponse)
                {
                    StatusCode = objectResult.StatusCode,
                    ContentTypes = objectResult.ContentTypes
                };
            }
            if (!context.ModelState.IsValid)
            {
                List<InvalidParams?> errors = context.ModelState
                        .Where(x => x.Value != null && x.Value.Errors.Count > 0 && x.Key != "model" && x.Key != null && x.Key != string.Empty)
                        .Select(x =>
                        {
                            string paramSource = GetParameterSource(context, x.Key);

                            string? customMessage = null;
                            var parameter = context.ActionDescriptor.Parameters.FirstOrDefault(p => p.Name == x.Key);
                            if (parameter != null && IsNumericType(parameter.ParameterType))
                            {
                                customMessage = $"Invalid {char.ToLower(x.Key[0])}{x.Key[1..]} value";
                            }

                            InvalidParams? res = null;
                            if (x.Value is not null)
                            {
                                res = new InvalidParams
                                {
                                    In = paramSource,
                                    Name = $"{char.ToLower(x.Key[0])}{x.Key[1..]}",
                                    Reason = GetFormatterErrorString(x.Value.Errors, customMessage, x.Key)
                                };
                            }
                            return res;
                        })
                        .Where(x => x != null)
                        .ToList();

                BadRequest result = new BadRequest("Invalid Request", "Provided input request parameter is not valid.", errors);
                result.Type = ResponseConstants.GetTypeUrl(context.HttpContext, "validation-error");

                context.Result = new BadRequestObjectResult(result);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                List<InvalidParams?> errors = context.ModelState
                        .Where(x => x.Value != null && x.Value.Errors.Count > 0 && x.Key != "model" && x.Key!=null && x.Key!=string.Empty)
                        .Select(x =>
                        {
                            string paramSource = GetParameterSource(context, x.Key);

                            string? customMessage = null;
                            var parameter = context.ActionDescriptor.Parameters.FirstOrDefault(p => p.Name == x.Key);
                            if (parameter != null && IsNumericType(parameter.ParameterType))
                            {
                                customMessage = $"Invalid {char.ToLower(x.Key[0])}{x.Key[1..]} value";
                            }

                            InvalidParams? res = null;
                            if (x.Value is not null)
                            {
                                res = new InvalidParams
                                {
                                    In = paramSource,
                                    Name = $"{char.ToLower(x.Key[0])}{x.Key[1..]}",
                                    Reason = GetFormatterErrorString(x.Value.Errors, customMessage, x.Key)
                                };
                            }
                            return res;
                        })
                        .Where(x => x != null)
                        .ToList();

                BadRequest result = new BadRequest("Invalid Request", "Provided input request parameter is not valid.", errors);
                result.Type = ResponseConstants.GetTypeUrl(context.HttpContext, "validation-error");

                context.Result = new BadRequestObjectResult(result);
            }
        }



        private string GetFormatterErrorString(ModelErrorCollection errors, string? customMessage, string key)
        {
            key = key.Split('.').Last();
            var errorMessages = new List<string>();
            foreach (var e in errors)
            {
                if(e.ErrorMessage.Contains("Error converting value"))
                {
                    errorMessages.Add($"Invalid {key} value");
                }
                else if (e.ErrorMessage.Contains("field is required"))
                {
                    errorMessages.Add($"{key} cannot be empty");
                }
                else
                {
                    errorMessages.Add(e.ErrorMessage);
                }
            }
            var res = customMessage ?? string.Join(", ", errorMessages);
            return res;
        }


        private static bool IsNumericType(Type type)
        {
            // Handle nullable types
            Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            // Check for numeric types
            return underlyingType == typeof(byte) ||
                   underlyingType == typeof(sbyte) ||
                   underlyingType == typeof(short) ||
                   underlyingType == typeof(ushort) ||
                   underlyingType == typeof(int) ||
                   underlyingType == typeof(uint) ||
                   underlyingType == typeof(long) ||
                   underlyingType == typeof(ulong) ||
                   underlyingType == typeof(float) ||
                   underlyingType == typeof(double) ||
                   underlyingType == typeof(decimal);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string GetParameterSource(ActionExecutingContext context, string key)
        {

            Microsoft.AspNetCore.Mvc.Abstractions.ParameterDescriptor? pathParameter = context.ActionDescriptor.Parameters
                        .FirstOrDefault(p => p.BindingInfo?.BindingSource == BindingSource.Path);


            if (pathParameter != null)
            {
                foreach (string modelStateKey in context.ModelState.Keys)
                {
                    if (modelStateKey.Equals(key, StringComparison.OrdinalIgnoreCase) && pathParameter.Name == key)
                    {
                        return "path";
                    }
                }
            }



            Microsoft.AspNetCore.Mvc.Abstractions.ParameterDescriptor? queryParameter = context.ActionDescriptor.Parameters
            .FirstOrDefault(p => p.BindingInfo?.BindingSource == BindingSource.Query);

            if (queryParameter != null)
            {
                foreach (string modelStateKey in context.ModelState.Keys)
                {
                    if (modelStateKey.Equals(key, StringComparison.OrdinalIgnoreCase) && queryParameter.Name == key)
                    {
                        return "query";
                    }
                }
            }



            Microsoft.AspNetCore.Mvc.Abstractions.ParameterDescriptor? bodyParameter = context.ActionDescriptor.Parameters
                .FirstOrDefault(p => p.BindingInfo?.BindingSource == BindingSource.Body);

            if (bodyParameter != null)
            {
                foreach (string modelStateKey in context.ModelState.Keys)
                {
                    if (modelStateKey.Equals(key, StringComparison.OrdinalIgnoreCase) ||
                        modelStateKey.Equals($"{bodyParameter.Name}.{key}", StringComparison.OrdinalIgnoreCase))
                    {
                        return "body";
                    }
                }
            }



            return "unknown";
        }



        private static string GetParameterSource(ActionExecutedContext context, string key)
        {

            Microsoft.AspNetCore.Mvc.Abstractions.ParameterDescriptor? pathParameter = context.ActionDescriptor.Parameters
                        .FirstOrDefault(p => p.BindingInfo?.BindingSource == BindingSource.Path);


            if (pathParameter != null)
            {
                foreach (string modelStateKey in context.ModelState.Keys)
                {
                    if (modelStateKey.Equals(key, StringComparison.OrdinalIgnoreCase) && pathParameter.Name == key)
                    {
                        return "path";
                    }
                }
            }



            Microsoft.AspNetCore.Mvc.Abstractions.ParameterDescriptor? queryParameter = context.ActionDescriptor.Parameters
            .FirstOrDefault(p => p.BindingInfo?.BindingSource == BindingSource.Query);

            if (queryParameter != null)
            {
                foreach (string modelStateKey in context.ModelState.Keys)
                {
                    if (modelStateKey.Equals(key, StringComparison.OrdinalIgnoreCase) && queryParameter.Name == key)
                    {
                        return "query";
                    }
                }
            }



            Microsoft.AspNetCore.Mvc.Abstractions.ParameterDescriptor? bodyParameter = context.ActionDescriptor.Parameters
                .FirstOrDefault(p => p.BindingInfo?.BindingSource == BindingSource.Body);

            if (bodyParameter != null)
            {
                foreach (string modelStateKey in context.ModelState.Keys)
                {
                    if (modelStateKey.Equals(key, StringComparison.OrdinalIgnoreCase) ||
                        modelStateKey.Equals($"{bodyParameter.Name}.{key}", StringComparison.OrdinalIgnoreCase))
                    {
                        return "body";
                    }
                }
            }



            return "unknown";
        }
    }
}
