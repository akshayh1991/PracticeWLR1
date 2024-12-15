using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecMan.Interfaces.BL;
using SecMan.Model;
using System.Net;
using UserAccessManagement.Filters;
using UserAccessManagement.Handler;

namespace UserAccessManagement.Controllers
{
    /// <summary>
    /// This controller contains API's for system settings
    /// </summary>
    [Route("v1/Settings")]
    [ApiController]
    [Authorize]
    [TypeFilter(typeof(SecurityActionFilter), Arguments = ["CAN_EDIT_COMMON_SYSTEM_FEATURES"])]
    [ProducesResponseType(typeof(ServerError), 500)]
    [ProducesResponseType(typeof(Unauthorized), 401)]
    [ProducesResponseType(typeof(Forbidden), 403)]
    public class SettingsController : ControllerBase
    {
        private readonly ISystemFeatureBL _systemFeatureBL;
        private readonly IMediator _mediator;


        /// <summary>
        /// This is an constructor for business layer DI
        /// </summary>
        /// <param name="systemFeatureBL"></param>
        public SettingsController(ISystemFeatureBL systemFeatureBL, IMediator mediator)
        {
            _systemFeatureBL = systemFeatureBL;
            _mediator = mediator;
        }


        /// <summary>
        /// Fetch System Settings
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(SystemPolicies), 200)]
        [ProducesResponseType(204)]
        public async Task<IActionResult> GetSettingsAsync()
        {
            // these are system settings, will be desplayed only once's that are common.
            await _mediator.Send(new InfoLogCommand
            {
                Message = "GetSettingsAsync method called ",
                Properties = new object[] { }
            });
            bool isCommon = true;
            ServiceResponse<List<SystemPolicies>>? res = await _systemFeatureBL.GetSystemPoliciesAsync(isCommon);

            if (res?.Data == null || res.Data?.Count == 0)
                return NoContent();

            await _mediator.Send(new InfoLogCommand
            {
                Message = "GetSettingsAsync method called with response ",
                Properties = new object[] { res?.Data }
            });

            return Ok(res?.Data);
        }



        /// <summary>
        /// Fetch System Settings by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(List<SystemPolicyData>), 200)]
        public async Task<IActionResult> GetSettingsByIdAsync(ulong id)
        {
            // these are system settings, will be desplayed only once's that are common.
            await _mediator.Send(new InfoLogCommand
            {
                Message = "GetSettingsByIdAsync method called with Request : {@Request}",
                Properties = new object[] { id }
            });
            bool isCommon = true;
            ServiceResponse<List<SystemPolicyData>>? res = await _systemFeatureBL.GetSystemPolicyByIdAsync(id, isCommon);

            if (res?.StatusCode == HttpStatusCode.NotFound)
                return NotFound(new NotFound(ResponseConstants.NotFound, res.Message));

            await _mediator.Send(new InfoLogCommand
            {
                Message = "GetSettingsByIdAsync method called with Response : {@Response}",
                Properties = new object[] { res?.Data }
            });
            return Ok(res?.Data);
        }


        /// <summary>
        /// Update System Setting
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(List<SystemPolicyData>), 200)]
        public async Task<IActionResult> UpdateSettingsByIdAsync(ulong id, List<UpdateSystemPolicyData> model)
        {
            // these are system settings, will be desplayed only once's that are common.
            await _mediator.Send(new InfoLogCommand
            {
                Message = "UpdateSettingsByIdAsync method called with Request : {@Request}",
                Properties = new object[] { model }
            });
            bool isCommon = true;
            ServiceResponse<List<UpdatedResponse>>? res = await _systemFeatureBL.UpdateSystemPolicyByIdAsync(id, model, isCommon, modelState: ModelState);
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            if (res?.StatusCode == HttpStatusCode.NotFound)
                return NotFound(new NotFound(ResponseConstants.NotFound, res.Message));
            if (res?.StatusCode == HttpStatusCode.BadRequest)
                return BadRequest(new BadRequest(ResponseConstants.InvalidRequest, res.Message));
            await _mediator.Send(new InfoLogCommand
            {
                Message = "UpdateSettingsByIdAsync method called with Response : {@Response}",
                Properties = new object[] { res?.Data }
            });
            return Ok(res.Data.Select(x => new UpdatedResponse { Id = x.Id }).ToList());
        }
    }
}
