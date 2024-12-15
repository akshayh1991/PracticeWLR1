using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SecMan.Interfaces.BL;
using SecMan.Model;

namespace UserAccessManagement.Filters
{
    /// <summary>
    /// This is an action filter to check specific permissions
    /// </summary>
    public class SecurityActionFilter : IActionFilter
    {
        private readonly string _permission;
        private readonly ICurrentUserService _currentUser;

        /// <summary>
        /// Contructor that takes on argument from DI and Another from attribute args
        /// </summary>
        /// <param name="permission"></param>
        /// <param name="currentUser"></param>
        public SecurityActionFilter(string permission, ICurrentUserService currentUser)
        {
            _permission = permission;
            _currentUser = currentUser;
        }


        /// <summary>
        /// This Method will be executed once the contoller method is executed
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }


        /// <summary>
        /// This Method Will be Exected before the controller method is executed
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (_currentUser?.AppPermissions?
                             .SelectMany(x => x.Permissions ?? Enumerable.Empty<Permission>())
                             .Any(x => x.Name == _permission) == false)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
