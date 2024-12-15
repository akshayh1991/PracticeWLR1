using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecMan.Interfaces.BL;
using SecMan.Model;
using System.Net;
using UserAccessManagement.Handler;

namespace UserAccessManagement.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("v1/apps")]
    public class ApplicationLauncherController : ControllerBase
    {
        private readonly IApplicationLauncherBL _applicationLauncherBL;
        private readonly IMediator _mediator;
        public ApplicationLauncherController(IApplicationLauncherBL applicationLauncherBL, IMediator mediator)
        {
            _applicationLauncherBL = applicationLauncherBL;
            _mediator = mediator;
        }


        /// <summary>
        /// List of Installed Apps
        /// </summary>
        /// <returns>A list of installed applications along with the version.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Applications), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetInstalledApplications()
        {
            try
            {
                ApplicationLauncherResponse result = await _applicationLauncherBL.GetInstalledApplicationsAsync();
                await _mediator.Send(new InfoLogCommand
                {
                    Message = "GetInstalledApplications method called",
                });

                if (result == null || result.InstalledApps == null || result.InstalledApps.Count == 0)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new ServerError("Not Found", HttpStatusCode.NotFound, "Something went wrong, please try again later."));
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                await _mediator.Send(new ErrorLogCommand
                {
                    Message = "GetInstalledApplications method called with exception" + ex.InnerException + ex.Message,
                    Exception = ex.InnerException
                });
                return StatusCode(StatusCodes.Status500InternalServerError, new ServerError("Internal Server Error", HttpStatusCode.InternalServerError, "An error occurred while processing your request."));
            }
        }
    }
}
