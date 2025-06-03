using AccountManagementSystem.DTOs;
using AccountManagementSystem.Helpers;
using AccountManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AccountManagementSystem.Controllers.Auth
{
    public class SignInController : Controller
    {
        private readonly IConfiguration _config;
        private readonly JwtService _jwtService;

        public SignInController(IConfiguration config, JwtService jwtService)
        {
            _config = config;
            _jwtService = jwtService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index([FromBody] SignInModel dto)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { token = "" });
            }

            try
            {
                using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    using (var cmd = new SqlCommand("sp_LoginUser", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Email", dto.Email);
                        cmd.Parameters.AddWithValue("@Password", dto.Password);

                        conn.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var tokenValue = _jwtService.GenerateJwtToken(dto.Email);
                                return Json(new { token = tokenValue });
                            }
                            else
                            {
                                return Json(new { token = "" });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { token = "", error = "Something went wrong." });
            }
        }


    }
}