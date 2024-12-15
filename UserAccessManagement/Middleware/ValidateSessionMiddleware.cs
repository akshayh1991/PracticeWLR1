using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using SecMan.Data.Repository;
using SecMan.Interfaces.BL;
using SecMan.Model;
using System.Net;

namespace UserAccessManagement.Middleware
{
    public class ValidateSessionMiddleware
    {
        private readonly RequestDelegate _next;

        public ValidateSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }


        public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
        {
            Endpoint? endpoint = context.GetEndpoint();
            if (endpoint != null && endpoint.Metadata.Any(m => m is AuthorizeAttribute))
            {
                string? authCookie = context.Request.Cookies["Auth-Cookie"];

                if (string.IsNullOrEmpty(authCookie))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    Unauthorized jsonResponse = BuildUnauthorizedResponse(nameof(HttpStatusCode.Unauthorized), HttpStatusCode.Unauthorized, ResponseConstants.MissingSessionCookie, context);
                    await context.Response.WriteAsJsonAsync(jsonResponse);
                    return;
                }

                AuthenticateResult result = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                if (result.Failure != null && result.Failure?.Message == "Ticket expired")
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    Unauthorized jsonResponse = BuildUnauthorizedResponse(nameof(HttpStatusCode.Unauthorized), HttpStatusCode.Unauthorized, ResponseConstants.ExpiredSessionCookie, context);
                    await context.Response.WriteAsJsonAsync(jsonResponse);
                    return;
                }

                if (!result.Succeeded)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    Unauthorized jsonResponse = BuildUnauthorizedResponse(nameof(HttpStatusCode.Unauthorized), HttpStatusCode.Unauthorized, ResponseConstants.InvalidSessionCookie, context);
                    await context.Response.WriteAsJsonAsync(jsonResponse);
                    return;
                }

                ICurrentUserService currentUserServices = context.RequestServices.GetRequiredService<ICurrentUserService>();
                if (!currentUserServices.IsValidSession)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    Unauthorized jsonResponse = BuildUnauthorizedResponse(nameof(HttpStatusCode.Unauthorized), HttpStatusCode.Unauthorized, ResponseConstants.InvalidSessionCookie, context);
                    await context.Response.WriteAsJsonAsync(jsonResponse);
                    return;
                }
            }
            await _next(context);
        }


        private Unauthorized BuildUnauthorizedResponse(string title, HttpStatusCode statusCode, string detail, HttpContext context)
        {
            return new Unauthorized
            {
                Type = ResponseConstants.GetTypeUrl(context, title),
                Title = title,
                Status = statusCode,
                Detail = detail
            };
        }
    }
}
