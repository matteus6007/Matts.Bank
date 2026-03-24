using System.Net;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MattsBank.Api.Controllers
{
    public class ErrorsController : ControllerBase
    {
        [Route("/error")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Error()
        {
            Exception? exception = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;

            var statusCode = HttpStatusCode.InternalServerError;

            if (exception != null)
            {
                if (exception is UnauthorizedAccessException) statusCode = HttpStatusCode.Unauthorized;
                else if (exception is ArgumentException) statusCode = HttpStatusCode.BadRequest;
            }

            return Problem(title: exception?.Message, statusCode: (int)statusCode);
        }
    }
}
