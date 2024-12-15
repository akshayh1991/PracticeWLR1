using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SecMan.BL;
using SecMan.Model;
using System.Net;

namespace UserAccessManagement.Controllers
{
    [Route("v1")]
    [ApiController]
    [Authorize]
    public class ReviewChangesController : ControllerBase
    {
        private readonly IReviewerBl _reviewerBl;
        private readonly IHttpContextAccessor _httpContext;

        public ReviewChangesController(IReviewerBl reviewerBl, IHttpContextAccessor httpContext)
        {
            _reviewerBl = reviewerBl;
            _httpContext = httpContext;
        }


        [HttpGet("unsaved-changes")]
        public async Task<ActionResult> GetUnsavedChanges()
        {
            SecMan.Model.ServiceResponse<JObject> res = await _reviewerBl.ReadJsonData();
            return Ok(res.Data);
        }



        [HttpPost("save-changes")]
        public async Task<ActionResult> SaveChanges(JObject model)
        {
            SecMan.Model.ApiResponse res = await _reviewerBl.SaveUnsavedChanges(model);
            if (res.StatusCode == HttpStatusCode.InternalServerError)
            {
                return Problem(res.Message, type: ResponseConstants.GetTypeUrl(_httpContext.HttpContext, "internal-server-error"),title:ResponseConstants.Status500InternalServerError);
            }
            return NoContent();
        }


        [HttpPut("save-changes")]
        public async Task<ActionResult> SaveUnsavedJsonChanges(JObject model)
        {
            await _reviewerBl.SaveUnsavedJsonChanges(model);
            return NoContent();
        }
    }
}
