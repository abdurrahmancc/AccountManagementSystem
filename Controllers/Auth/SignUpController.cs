using AccountManagementSystem.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AccountManagementSystem.Controllers.Auth
{
    public class SignUpController : Controller
    {
        private readonly IConfiguration _config;

        public SignUpController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(SignUpDto dto)
        {
            if (ModelState.IsValid)
            {
                var id = Guid.NewGuid();

                using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    var cmd = new SqlCommand("sp_RegisterUser", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@FullName", dto.FullName);
                    cmd.Parameters.AddWithValue("@Email", dto.Email);
                    cmd.Parameters.AddWithValue("@Password", dto.Password);
                    cmd.Parameters.AddWithValue("@Role", "Viewer");

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                return RedirectToAction("Index", "Signin");
            }

            return View(dto);
        }
    }
}
