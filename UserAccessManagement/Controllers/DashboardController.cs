using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecMan.Interfaces.BL;
using SecMan.Model;
using UserAccessManagement.Filters;
using UserAccessManagement.Handler;

namespace UserAccessManagement.Controllers
{
    [Route("v1/Dashboard")]
    [ApiController]
    [Authorize]
    [TypeFilter(typeof(SecurityActionFilter), Arguments = ["CAN_EDIT_SECURITY"])]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardBL _dashboardBAL;
        private readonly IMediator _mediator;
        public DashboardController(IDashboardBL dashboardBAL, IMediator mediator)
        {
            _dashboardBAL = dashboardBAL;
            _mediator = mediator;
        }

        /// <summary>
        /// Dashboard Summary
        /// </summary>
        /// <returns>An ActionResult containing Dashboard data</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetDashBoard()
        {
            await _mediator.Send(new InfoLogCommand
            {
                Message = "GetDashBoard method called"
            });
            Dashboard result = await _dashboardBAL.GetDashBoardResult();

            if (result == null)
            {
                return NotFound();
            }
            await _mediator.Send(new InfoLogCommand
            {
                Message = "GetDashBoard method response: {@Request}",
                Properties = new object[] { result }
            });
            return Ok(result);
        }
    }
}
