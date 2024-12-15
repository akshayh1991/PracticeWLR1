using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace UserAccessManagement.Swagger
{
    /// <summary>
    /// This Class is added as auth filter for swagger
    /// which add an auth functionality in swagger
    /// </summary>
    public class AuthOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            bool isAuthorized = context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ||
                               context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

            if (!isAuthorized) return;

            OpenApiSecurityScheme jwtBearerSchema = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Cookie" }
            };

            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new()
                {
                    [jwtBearerSchema] = Array.Empty<string>()
                }
            };
        }
    }
}
