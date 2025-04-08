using Microsoft.AspNetCore.Mvc;
namespace LinksApp.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult Index(int statusCode)
        {
            Response.Clear();
            Response.StatusCode = statusCode;
            return statusCode switch
            {
                401 => View("UnauthorizedError"),
                403 => View("ForbiddenError"),
                404 => View("PageNotFoundError"),
                500 => View("InternalServerError"),
                503 => View("ServiceUnavailableError"),
                _ => View("GenericError"),
            };
        }
    }
}
