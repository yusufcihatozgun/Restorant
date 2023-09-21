using Microsoft.AspNetCore.Mvc;

namespace Restorant.Web.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Error(int? statusCode = null)
        {
            if (statusCode.HasValue)
            {
                if (statusCode.Value == 403 || statusCode.Value == 404 || statusCode.Value == 500)
                {
                    var viewName = "Error" + statusCode.ToString();
                    return View(viewName);
                }
            }
            return View();
        }

        public IActionResult Error403()
        {
            return View();
        }
        public IActionResult Error404()
        {
            return View();
        }
        public IActionResult Error500()
        {
            return View();
        }
    }
}
