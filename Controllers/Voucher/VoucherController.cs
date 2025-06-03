using Microsoft.AspNetCore.Mvc;

namespace AccountManagementSystem.Controllers.Voucher
{
    public class VoucherController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
