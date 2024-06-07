using ImageGenerator.API.Errors;
using Microsoft.AspNetCore.Mvc;

namespace ImageGenerator.API.Controllers
{
    // When trying reach Endpoint not existing, will redirect to this..
    [Route("errors/{code}")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorsController : ControllerBase
    {
        public ActionResult Error(int code)
        {
            return NotFound(new ApiResponse(code));
        }
    }
}
