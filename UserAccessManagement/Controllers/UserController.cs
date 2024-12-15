using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecMan.BL.Common;
using SecMan.Interfaces.BL;
using SecMan.Model;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Net;
using UserAccessManagement.Filters;
using UserAccessManagement.Handler;

namespace UserAccessManagement.Controllers
{
    /// <summary>
    /// This Controller contains all the user CRUD API's
    /// </summary>
    [ApiController]
    [Route("v1/users")]
    [ProducesResponseType(typeof(BadRequest), 400)]
    [ProducesResponseType(typeof(ServerError), 500)]
    [ProducesResponseType(typeof(Unauthorized), 401)]
    [ProducesResponseType(typeof(Forbidden), 403)]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserBL _userBAL;
        private readonly IMediator _mediator;
        /// <summary>
        /// User Controller Constructor for DI
        /// </summary>
        /// <param name="userBAL"></param>
        /// <param name="mediator"></param>
        public UsersController(IUserBL userBAL, IMediator mediator)
        {
            _userBAL = userBAL;
            _mediator = mediator;
        }

        /// <summary>
        /// Create User
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(User), 201)]
        [ProducesResponseType(typeof(Conflict), 409)]
        [TypeFilter(typeof(SecurityActionFilter), Arguments = ["CAN_EDIT_SECURITY"])]
        public async Task<ActionResult> AddUserAsync(CreateUser model)
        {
            await _mediator.Send(new InfoLogCommand
            {
                Message = "AddUserAsync method called with request: {@Request}",
                Properties = new object[] { model }
            });

            ServiceResponse<User> res = await _userBAL.AddUserAsync(model);

            if (res.StatusCode == HttpStatusCode.Conflict)
                return Conflict(new Conflict(ResponseConstants.Conflict, HttpStatusCode.Conflict, res.Message));

            if (res.StatusCode == HttpStatusCode.BadRequest)
                return BadRequest(new BadRequest(ResponseConstants.InvalidRequest, res.Message));

            await _mediator.Send(new InfoLogCommand
            {
                Message = "AddUserAsync method response: {@Request}",
                Properties = new object[] { res.Data! }
            });


            return Created(nameof(AddUserAsync), new object());
        }

        /// <summary>
        /// List Users
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<User>), 200)]
        [ProducesResponseType(204)]
        [TypeFilter(typeof(SecurityActionFilter), Arguments = ["CAN_EDIT_SECURITY"])]
        public async Task<ActionResult> GetUsersAsync([FromQuery] UsersFilterDto model)
        {
            await _mediator.Send(new InfoLogCommand
            {
                Message = "GetUsersAsync method request: {@Request}",
                Properties = new object[] { model }
            });
            ServiceResponse<UsersWithCountDto> res = await _userBAL.GetUsersAsync(model);

            Log.Information("Adding total user count to response headers,User Count {@Usercount}", res?.Data?.UserCount);
            Response.Headers.Append(ResponseHeaders.TotalCount, res?.Data?.UserCount.ToString());

            Log.Information("Finishing API {APIName}", nameof(GetUsersAsync));

            await _mediator.Send(new InfoLogCommand
            {
                Message = "GetUsersAsync method response: {@Request}",
                Properties = new object[] { res?.Data?.Users! }
            });

            return Ok(res?.Data?.Users);
        }

        /// <summary>
        /// Enquire user by id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(List<User>), 200)]
        [TypeFilter(typeof(SecurityActionFilter), Arguments = ["CAN_EDIT_SECURITY"])]
        public async Task<ActionResult> GetUserByIdAsync(ulong userId)
        {
            await _mediator.Send(new InfoLogCommand
            {
                Message = "GetUserByIdAsync method request: {@Request}",
                Properties = new object[] { userId }
            });

            ServiceResponse<User> res = await _userBAL.GetUserByIdAsync(userId);

            if (res.StatusCode == HttpStatusCode.NotFound)
                return NotFound(new NotFound(ResponseConstants.NotFound, res.Message));

            await _mediator.Send(new InfoLogCommand
            {
                Message = "GetUserByIdAsync method request: {@Request}",
                Properties = new object[] { res.Data! }
            });

            return Ok(res.Data);
        }

        /// <summary>
        /// Update user by id
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPut("{userId}")]
        [ProducesResponseType(typeof(List<User>), 200)]
        [TypeFilter(typeof(SecurityActionFilter), Arguments = ["CAN_EDIT_SECURITY"])]
        public async Task<ActionResult> UpdateUserAsync(UpdateUser model, ulong userId)
        {
            await _mediator.Send(new InfoLogCommand
            {
                Message = "UpdateUserAsync method request: {@Request}",
                Properties = new object[] { model, userId }
            });

            ServiceResponse<User> res = await _userBAL.UpdateUserAsync(model, userId);

            if (res.StatusCode == HttpStatusCode.NotFound)
                return NotFound(new NotFound(ResponseConstants.NotFound, res.Message));
            if (res.StatusCode == HttpStatusCode.BadRequest)
                return BadRequest(new BadRequest(ResponseConstants.InvalidRequest, res.Message));
            if (res.StatusCode == HttpStatusCode.Forbidden)
                return Forbid();
            if (res.StatusCode == HttpStatusCode.Conflict)
                return Conflict(new Conflict(ResponseConstants.Conflict, HttpStatusCode.Conflict, res.Message));

            await _mediator.Send(new InfoLogCommand
            {
                Message = "UpdateUserAsync method response: {@Request}",
                Properties = new object[] { res.Data! }
            });

            return Ok(new UpdatedResponse { Id = userId });
        }

        /// <summary>
        /// Delete user by id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpDelete("{userId}")]
        [ProducesResponseType(204)]
        [TypeFilter(typeof(SecurityActionFilter), Arguments = ["CAN_EDIT_SECURITY"])]
        public async Task<ActionResult> DeleteUserAsync(ulong userId)
        {
            await _mediator.Send(new InfoLogCommand
            {
                Message = "DeleteUserAsync method request: {@Request}",
                Properties = new object[] { userId }
            });

            ApiResponse res = await _userBAL.DeleteUserAsync(userId);

            if (res.StatusCode == HttpStatusCode.NotFound)
                return NotFound(new NotFound(ResponseConstants.NotFound, res.Message));

            return NoContent();
        }


        /// <summary>
        /// Retire user by id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("{userId}/retire")]
        [ProducesResponseType(204)]
        [TypeFilter(typeof(SecurityActionFilter), Arguments = ["CAN_EDIT_SECURITY"])]
        public async Task<IActionResult> RetireUserAsync(ulong userId)
        {
            await _mediator.Send(new InfoLogCommand
            {
                Message = "RetireUserAsync method request: {@Request}",
                Properties = new object[] { userId }
            });
            ApiResponse res = await _userBAL.RetireUserAsync(userId);

            if (res.StatusCode == HttpStatusCode.NotFound)
                return NotFound(new NotFound(ResponseConstants.NotFound, res.Message));
            if (res.StatusCode == HttpStatusCode.Conflict)
                return Conflict(new Conflict(ResponseConstants.Conflict, res.StatusCode, res.Message));

            return NoContent();
        }


        /// <summary>
        /// Unlock user by id
        /// </summary>
        /// <param name="id">Unlock the user by Id</param>
        /// <param name="changePasswordOnLogin">Change Password on login</param>
        /// <returns></returns>
        [HttpPost("{id}/unlock")]
        [ProducesResponseType(204)]
        [TypeFilter(typeof(SecurityActionFilter), Arguments = ["CAN_EDIT_SECURITY"])]
        public async Task<IActionResult> UnlockUserAsync(ulong id, [Required] bool changePasswordOnLogin)
        {
            await _mediator.Send(new InfoLogCommand
            {
                Message = "UnlockUserAsync method request: {@Request}",
                Properties = new object[] { id }
            });
            ApiResponse res = await _userBAL.UnlockUserAsync(id, changePasswordOnLogin);

            if (res.StatusCode == HttpStatusCode.NotFound)
                return NotFound(new NotFound(ResponseConstants.NotFound, res.Message));

            await _mediator.Send(new InfoLogCommand
            {
                Message = "UnlockUserAsync method response: {@Response}",
                Properties = new object[] { res }
            });

            return NoContent();
        }


        /// <summary>
        /// Update user preferred language
        /// </summary>
        /// <param name="languageCode"></param>
        /// <returns></returns>
        [HttpPost("languages/{languageCode}")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> LanguageCodeAsync([Required(ErrorMessage = ValidationConstants.InvalidLanguageError),ValidateLanguage] string? languageCode)
        {
            await _mediator.Send(new InfoLogCommand
            {
                Message = "LanguageCodeAsync method request: {@Request}",
                Properties = new object[] { languageCode }
            });
            ApiResponse res = await _userBAL.UpdateUserLanguageAsync(languageCode);
            await _mediator.Send(new InfoLogCommand
            {
                Message = "LanguageCodeAsync method response: {@Response}",
                Properties = new object[] { res }
            });
            if (res.StatusCode == HttpStatusCode.NotFound)
                return NotFound(new NotFound(ResponseConstants.NotFound, res.Message));
            return NoContent();
        }
    }
}
