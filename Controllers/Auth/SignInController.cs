using AccountManagementSystem.DTOs;
using AccountManagementSystem.Helpers;
using AccountManagementSystem.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Claims;

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
        public async Task<IActionResult> Index([FromBody] SignInModel dto)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { token = "" });
            }
            AccountsModel user = null;

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
                                user = new AccountsModel
                                {
                                    UserId = reader.GetGuid(0),
                                    Email = reader.GetString(1),
                                    FullName = reader.GetString(2),
                                    Role = reader.GetGuid(3)
                                };

                               
                                var tokenValue = _jwtService.GenerateJwtToken(dto.Email);
                                var role = GetRoleByNameById(user.Role);
                                var claims = new List<Claim>
                                    {
                                        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                                        new Claim(ClaimTypes.Email, user.Email),
                                        new Claim(ClaimTypes.Name, user.FullName),
                                        new Claim(ClaimTypes.Role, role)
                                    };

                                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "SignIn");
        }


       private string GetRoleByNameById(Guid id)
        {
            var roles = GetAllRoles();
            foreach (var role in  roles)
            {
                if (role.RoleId == id)
                {
                    return role.RoleName;
                }
            }
            return "";
        }

        private List<RoleModel> GetAllRoles()
        {
            var roles = new List<RoleModel>();
            var connStr = _config.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT RoleId, RoleName FROM AspNetRoles";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    roles.Add(new RoleModel
                    {
                        RoleId = (Guid)reader["RoleId"],
                        RoleName = reader["RoleName"].ToString()
                    });

                }

                conn.Close();
            }
            return roles;
        }  


        }
    }