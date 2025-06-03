using Microsoft.AspNetCore.Mvc;

namespace AccountManagementSystem.Controllers.Chart
{
    public class ChartOfAccountsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
