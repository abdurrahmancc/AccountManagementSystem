using Microsoft.AspNetCore.Mvc;

namespace AccountManagementSystem.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HandleErrorCode(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = "Page not found.";
                    break;
                case 403:
                    ViewBag.ErrorMessage = "Access denied.";
                    break;
                default:
                    ViewBag.ErrorMessage = "An unexpected error occurred.";
                    break;
            }

            return View("Error");
        }

        [Route("Error/General")]
        public IActionResult General()
        {
            ViewBag.ErrorMessage = "An unexpected error occurred.";
            return View("Error");
        }
    }


}
