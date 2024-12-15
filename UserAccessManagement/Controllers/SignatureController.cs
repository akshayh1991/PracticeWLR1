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
    /// Signature controller
    /// </summary>
    /// 
    [Authorize]
    [Route("v1/signatures")]
    [ApiController]
    public class SignatureController : ControllerBase
    {
        private readonly ISignatureBL _signatureBL;
        private readonly IMediator _mediator;

        /// <summary>
        /// Signature Contsructor
        /// </summary>
        /// <param name="signatureBL"></param>
        /// <param name="mediator"></param>
        public SignatureController(ISignatureBL signatureBL, IMediator mediator)
        {
            _signatureBL = signatureBL;
            _mediator = mediator;
        }

        /// <summary>
        /// Verify logged in user password
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>

        [HttpPost("verify")]
        [TypeFilter(typeof(SecurityActionFilter), Arguments = ["CAN_SIGN"])]
        public async Task<IActionResult> VerifySignature([FromBody] VerifySignature request)
        {
            await _mediator.Send(new InfoLogCommand
            {
                Message = "VerifySignature method request: {@Request}",
                Properties = new object[] { request }
            });

            if (request == null)
            {
                return BadRequest(new BadRequest(ResponseConstants.InvalidRequest, ResponseConstants.RequestEmpty));                
            }

            ApiResponse result = await _signatureBL.VerifySignatureAsync(request.Password, request.Note);
            await _mediator.Send(new InfoLogCommand
            {
                Message = "VerifySignature method response: {@Response}",
                Properties = new object[] { result }
            });

            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return NoContent();
            }
            return BadRequest(new BadRequest(ResponseConstants.InvalidRequest, ResponseConstants.FailedSignatureVerified));
        }

        /// <summary>
        /// Verify authorizer credentials
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("authorize")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> AuthorizeSignature(Authorize request)
        {
            await _mediator.Send(new InfoLogCommand
            {
                Message = "AuthorizeSignature method request: {@Request}",
                Properties = new object[] { request }
            });

            if (request == null)
            {
                return BadRequest(new BadRequest(ResponseConstants.InvalidRequest, ResponseConstants.RequestEmpty));
            }

            if (request.IsNote && string.IsNullOrWhiteSpace(request.Note))
            {
                return BadRequest(new BadRequest(ResponseConstants.InvalidRequest, ResponseConstants.NoteRequired));
            }

            if (request.IsAuthorize && (string.IsNullOrEmpty(request.UserName) || (string.IsNullOrEmpty(request.Password))))
            {
                return BadRequest(new BadRequest(ResponseConstants.InvalidRequest, ResponseConstants.IsAuthorizeUsernamePasswordRequired));
            }
            ApiResponse result = await _signatureBL.SignatureAuthorizeAsync(request);

            await _mediator.Send(new InfoLogCommand
            {
                Message = "AuthorizeSignature method response: {@Response}",
                Properties = new object[] { result }
            });
            if (result.StatusCode == HttpStatusCode.Forbidden)
            {
                return Forbid();
            }
            if (result.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(new BadRequest(ResponseConstants.InvalidRequest, result.Message));              
            }
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound(new NotFound(ResponseConstants.NotFound, result.Message));
            }

            if (result.StatusCode == HttpStatusCode.OK)
            {
                return NoContent();
            }
            else
            {
                return Problem(ResponseConstants.Status500InternalServerError,result.Message);
            }
        }
    }
}

