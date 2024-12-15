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
    /// This controller contains API's for Security Policy
    /// </summary>
    [Route("v1/security-policies")]
    [ApiController]
    [Authorize]
    [TypeFilter(typeof(SecurityActionFilter), Arguments = ["CAN_EDIT_SYSTEM_FEATURES"])]
    [ProducesResponseType(typeof(ServerError), 500)]
    [ProducesResponseType(typeof(Unauthorized), 401)]
    [ProducesResponseType(typeof(Forbidden), 403)]
    public class SecurityPolicyController : ControllerBase
    {
        private readonly ISystemFeatureBL _systemFeatureBL;
        private readonly IMediator _mediator;


        /// <summary>
        /// This is an constructor for business layer DI
        /// </summary>
        /// <param name="systemFeatureBL"></param>
        public SecurityPolicyController(ISystemFeatureBL systemFeatureBL, IMediator mediator)
        {
            _systemFeatureBL = systemFeatureBL;
            _mediator = mediator;
        }


        /// <summary>
        /// Fetch Security Policies
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(SystemPolicies), 200)]
        [ProducesResponseType(204)]
        public async Task<IActionResult> GetSecurityPoliciesAsync()
        {
            await _mediator.Send(new InfoLogCommand
            {
                Message = "GetSecurityPoliciesAsync method called ",
                Properties = new object[] { }
            });
            // these are security policies, will be displayed both common and non common.
            bool isCommon = false;
            ServiceResponse<List<SystemPolicies>>? res = await _systemFeatureBL.GetSystemPoliciesAsync(isCommon);

            if (res?.Data == null || res.Data?.Count == 0)
                return NoContent();
            await _mediator.Send(new InfoLogCommand
            {
                Message = "GetSecurityPoliciesAsync method called with response ",
                Properties = new object[] { res?.Data }
            });

            return Ok(res?.Data);
        }


        /// <summary>
        /// Fetch Security Policy by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(List<SystemPolicyData>), 200)]
        public async Task<IActionResult> GetSecurityPolicyByIdAsync(ulong id)
        {
            // these are security policies, will be displayed both common and non common.
            await _mediator.Send(new InfoLogCommand
            {
                Message = "GetSecurityPolicyByIdAsync method called with Request : {@Request}",
                Properties = new object[] { id }
            });
            bool isCommon = false;
            ServiceResponse<List<SystemPolicyData>>? res = await _systemFeatureBL.GetSystemPolicyByIdAsync(id, isCommon);

            if (res?.StatusCode == HttpStatusCode.NotFound)
                return NotFound(new NotFound(ResponseConstants.NotFound, res.Message));

            await _mediator.Send(new InfoLogCommand
            {
                Message = "GetSecurityPolicyByIdAsync method called with Response : {@Response}",
                Properties = new object[] { res?.Data }
            });
            return Ok(res?.Data);
        }


        /// <summary>
        /// Update Security Policy
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(List<SystemPolicyData>), 200)]
        public async Task<IActionResult> UpdateSystemPolicyByIdAsync(ulong id, List<UpdateSystemPolicyData> model)
        {
            // these are security policies, will be displayed both common and non common.
            await _mediator.Send(new InfoLogCommand
            {
                Message = "UpdateSystemPolicyByIdAsync method called with Request : {@Request}",
                Properties = new object[] { model }
            });
            bool isCommon = false;
            ServiceResponse<List<UpdatedResponse>>? res = await _systemFeatureBL.UpdateSystemPolicyByIdAsync(id, model, isCommon, modelState: ModelState);
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            if (res?.StatusCode == HttpStatusCode.NotFound)
                return NotFound(new NotFound(ResponseConstants.NotFound, res.Message));
            if (res.StatusCode == HttpStatusCode.BadRequest)
                return BadRequest(new BadRequest(ResponseConstants.InvalidRequest, res.Message));
            await _mediator.Send(new InfoLogCommand
            {
                Message = "UpdateSystemPolicyByIdAsync method called with Response : {@Response}",
                Properties = new object[] { res?.Data }
            });
            return Ok(res.Data.Select(x => new UpdatedResponse { Id = x.Id }).ToList());
        }
    }
}
