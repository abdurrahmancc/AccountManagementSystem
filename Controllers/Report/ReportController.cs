using Microsoft.AspNetCore.Mvc;

namespace AccountManagementSystem.Controllers.Report
{
    public class ReportController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
