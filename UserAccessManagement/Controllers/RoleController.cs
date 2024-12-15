using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecMan.Data.Exceptions;
using SecMan.Interfaces.BL;
using SecMan.Model;
using System.Net;
using System.Text.Json;
using UserAccessManagement.Filters;
using UserAccessManagement.Handler;

namespace UserAccessManagement.Controllers
{
    /// <summary>
    /// Controller for managing roles within the application.
    /// </summary>
    [Route("v1/roles")]
    [ApiController]
    [Authorize]
    [TypeFilter(typeof(SecurityActionFilter), Arguments = ["CAN_EDIT_SECURITY"])]
    public class RoleController : ControllerBase
    {
        private readonly IRoleBL _roleBAL;
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleController"/> class.
        /// </summary>
        /// <param name="roleBAL">Business logic layer for roles.</param>
        /// <param name="mediator">Logger for capturing logs.</param>
        public RoleController(IRoleBL roleBAL, IMediator mediator, IConfiguration configuration)
        {
            _roleBAL = roleBAL;
            _mediator = mediator;
            _configuration = configuration;
        }

        /// <summary>
        /// Retrieves all roles.
        /// </summary>
        /// <returns>Action result containing the list of roles.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            await _mediator.Send(new InfoLogCommand
            {
                Message = "GetAllRoles method called"
            });

            IEnumerable<GetRoleDto> roles = await _roleBAL.GetAllRolesAsync();
            await _mediator.Send(new InfoLogCommand
            {
                Message = "GetAllRoles method called with response: {@Response}",
                Properties = new object[] { roles }
            });
            return Ok(roles);
        }

        /// <summary>
        /// Retrieves a role by its ID.
        /// </summary>
        /// <param name="id">The ID of the role to retrieve.</param>
        /// <returns>Action result containing the role details.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(ulong id)
        {
            await _mediator.Send(new InfoLogCommand
            {
                Message = "GetRoleById method called with ID: {@Id}",
                Properties = new object[] { id }
            });

            var result = await _roleBAL.GetRoleByIdAsync(id);
            await _mediator.Send(new InfoLogCommand
            {
                Message = "GetRoleById method called with ID: {@Id}",
                Properties = new object[] { result }
            });
            if (result.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(new BadRequest(ResponseConstants.InvalidRequest, result.Message));
            }
            return Ok(result.Data);
        }

        /// <summary>
        /// Creates a new role.
        /// </summary>
        /// <param name="dto">Data transfer object containing role details.</param>
        /// <returns>Action result indicating the outcome of the creation.</returns>
        [HttpPost]
        public async Task<IActionResult> AddRole([FromBody] CreateRole dto)
        {
            await _mediator.Send(new InfoLogCommand
            {
                Message = "AddRole method called with request: {@Request}",
                Properties = new object[] { dto }
            });

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var roleNameResult = await _roleBAL.ValidateRoleNameAsync(dto.Name);
            if (roleNameResult.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(new BadRequest(ResponseConstants.InvalidRequest, roleNameResult.Message));
            }
            var result = await _roleBAL.ExistingRoleName(dto.Name);
            if (result.StatusCode == HttpStatusCode.Conflict)
            {
                return Conflict(new Conflict(ResponseConstants.Conflict, HttpStatusCode.Conflict, result.Message));
            }

            var res = await _roleBAL.AddRoleAsync(dto);
            if (res.StatusCode == HttpStatusCode.Conflict)
                return Conflict(new Conflict(ResponseConstants.Conflict, HttpStatusCode.Conflict, res.Message));

            await _mediator.Send(new InfoLogCommand
            {
                Message = "AddRole method called with Response: {@Response}",
                Properties = new object[] { "Add Role Data is added to json file.", dto }
            });

            return Created(nameof(AddRole), new object());
        }


        /// <summary>
        /// Updates an existing role by its ID.
        /// </summary>
        /// <param name="id">The ID of the role to update.</param>
        /// <param name="addRoleDto">Data transfer object containing updated role details.</param>
        /// <returns>Action result indicating the outcome of the update.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(ulong id, [FromBody] UpdateRole addRoleDto)
        {
            await _mediator.Send(new InfoLogCommand
            {
                Message = "UpdateRole method called with request: {@Request}",
                Properties = new object[] { id, addRoleDto }
            });

            var existingRole = await _roleBAL.GetRoleByIdAsync(id);
            if (existingRole.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound(new NotFound(ResponseConstants.NotFound, existingRole.Message));
            }
            var result = await _roleBAL.ExistingRoleNameWhileUpdation(addRoleDto.Name, id);
            if (result.StatusCode == HttpStatusCode.Conflict)
            {
                return Conflict(new Conflict(ResponseConstants.InvalidRequest, HttpStatusCode.Conflict, result.Message));
            }

            var res = await _roleBAL.UpdateRoleAsync(id, addRoleDto);
            if (res.StatusCode == HttpStatusCode.Conflict)
            {
                return Conflict(new Conflict(ResponseConstants.InvalidRequest, HttpStatusCode.Conflict, res.Message));
            }

            await _mediator.Send(new InfoLogCommand
            {
                Message = "UpdateRole method called with response: {@Response}",
                Properties = new object[] { "Update Role Data is added to json file" }
            });

            return Ok(new UpdatedResponse { Id = id });
        }

        /// <summary>
        /// Deletes a role by its ID.
        /// </summary>
        /// <param name="id">The ID of the role to delete.</param>
        /// <returns>Action result indicating whether the deletion was successful.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(ulong id)
        {
            await _mediator.Send(new InfoLogCommand
            {
                Message = "DeleteRole method called with request: {@Request}",
                Properties = new object[] { id }
            });
            var result = await _roleBAL.GetRoleByIdAsync(id);
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound(new NotFound(ResponseConstants.NotFound, result.Message));
            }

            await _roleBAL.DeleteRoleAsync(id);

            await _mediator.Send(new InfoLogCommand
            {
                Message = "DeleteRole method called with response: {@Response}",
                Properties = new object[] { "DeleteRole Data is added to json file" }
            });
            return NoContent();
        }
    }
}
