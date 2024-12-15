using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecMan.BL;
using SecMan.Interfaces.BL;
using SecMan.Model;
using System.Net;
using UserAccessManagement.Filters;
using UserAccessManagement.Handler;

namespace UserAccessManagement.Controllers
{
    [Route("v1/devices")]
    [ApiController]
    [Authorize]
    public class DevicesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IDeviceBL _deviceBL;

        public DevicesController(IMediator mediator,IDeviceBL deviceBL)
        {
            _mediator = mediator;
            _deviceBL = deviceBL;
        }

        /// <summary>
        /// Add device
        /// </summary>
        /// <param name="createDevice"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(User), 201)]
        [ProducesResponseType(typeof(Conflict), 409)]
        [TypeFilter(typeof(SecurityActionFilter), Arguments = ["CAN_EDIT_SECURITY"])]
        public async Task<IActionResult> AddDevice([FromBody] CreateDevice createDevice)
        {
            await _mediator.Send(new InfoLogCommand
            {
                Message = "AddDevice method called with request: {@Request}",
                Properties = new object[] { createDevice }
            });

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            ServiceResponse<GetDevice> res = await _deviceBL.AddDeviceAsync(createDevice);

            if (res.StatusCode == HttpStatusCode.Conflict)
                return Conflict(new Conflict(ResponseConstants.Conflict, HttpStatusCode.Conflict, res.Message));

            if (res.StatusCode == HttpStatusCode.BadRequest)
                return BadRequest(new BadRequest(ResponseConstants.InvalidRequest, res.Message));
            await _mediator.Send(new InfoLogCommand
            {
                Message = "AddDevice method called with Response: {@Response}",
                Properties = new object[] { "Add Role Data is added to json file.", res }
            });
            return Created(nameof(AddDevice), new object());
        }

        /// <summary>
        /// Updates an existing role by its ID.
        /// </summary>
        /// <param name="id">The ID of the role to update.</param>
        /// <param name="addRoleDto">Data transfer object containing updated role details.</param>
        /// <returns>Action result indicating the outcome of the update.</returns>
        [HttpPut("{id}")]
        [TypeFilter(typeof(SecurityActionFilter), Arguments = ["CAN_EDIT_SECURITY"])]
        public async Task<IActionResult> UpdateDevice(ulong id, [FromBody] UpdateDevice updateDevice)
        {
            await _mediator.Send(new InfoLogCommand
            {
                Message = "UpdateDevice method called with request: {@Request}",
                Properties = new object[] { id, updateDevice }
            });
            
            var res = await _deviceBL.UpdateDeviceAsync(id, updateDevice);
            if (res.StatusCode == HttpStatusCode.BadRequest)
                return BadRequest(new BadRequest(ResponseConstants.InvalidRequest, res.Message));
            if (res.StatusCode==HttpStatusCode.NotFound)
            {
                return NotFound(new NotFound(ResponseConstants.NotFound, res.Message));
            }
            if (res.StatusCode == HttpStatusCode.Conflict)
            {
                return Conflict(new Conflict(ResponseConstants.InvalidRequest, HttpStatusCode.Conflict, res.Message));
            }

            await _mediator.Send(new InfoLogCommand
            {
                Message = "UpdateDevice method called with response: {@Response}",
                Properties = new object[] { "Update Role Data is added to json file" }
            });

            return Ok(new UpdatedResponse { Id = id });
        }

        /// <summary>
        /// Delete user by id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [TypeFilter(typeof(SecurityActionFilter), Arguments = ["CAN_EDIT_SECURITY"])]
        public async Task<ActionResult> DeleteDevice(ulong id)
        {
            await _mediator.Send(new InfoLogCommand
            {
                Message = "DeleteDevice method request: {@Request}",
                Properties = new object[] { id }
            });

            ApiResponse res = await _deviceBL.DeleteDeviceAsync(id);

            if (res.StatusCode == HttpStatusCode.NotFound)
                return NotFound(new NotFound(ResponseConstants.NotFound, res.Message));

            return NoContent();
        }
    }
}
