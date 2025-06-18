using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using AccountManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

public class RoleManagementController : Controller
{
    private readonly string _connectionString;

    public RoleManagementController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        var roles = new List<RoleManagementModel>();
       var data = User.FindFirst(ClaimTypes.Role)?.Value;
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand("sp_ManageAspNetRoles", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Action", "SELECT");

            await conn.OpenAsync();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    roles.Add(new RoleManagementModel
                    {
                        RoleId = (Guid)reader["RoleId"],
                        RoleName = reader["RoleName"].ToString(),
                        Description = reader["Description"].ToString(),
                        Discriminator = reader["Discriminator"]?.ToString()
                    });
                }
            }
        }

        return View(roles);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Save(RoleManagementModel model)
    {
        string action = model.RoleId == Guid.Empty ? "INSERT" : "UPDATE";
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand("sp_ManageAspNetRoles", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Action", action);
            cmd.Parameters.AddWithValue("@RoleId", model.RoleId == Guid.Empty ? Guid.NewGuid() : model.RoleId);
            cmd.Parameters.AddWithValue("@RoleName", model.RoleName);
            cmd.Parameters.AddWithValue("@Description", model.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Discriminator", model.Discriminator ?? (object)DBNull.Value);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand("sp_ManageAspNetRoles", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Action", "DELETE");
            cmd.Parameters.AddWithValue("@RoleId", id);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        return RedirectToAction("Index");
    }


    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(Guid id)
    {
        RoleManagementModel model = null;

        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand("sp_ManageAspNetRoles", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Action", "SELECT");

            await conn.OpenAsync();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    if ((Guid)reader["RoleId"] == id)
                    {
                        model = new RoleManagementModel
                        {
                            RoleId = (Guid)reader["RoleId"],
                            RoleName = reader["RoleName"].ToString(),
                            Description = reader["Description"].ToString(),
                            Discriminator = reader["Discriminator"]?.ToString()
                        };
                        break;
                    }
                }
            }
        }

        if (model == null)
        {
            return NotFound();
        }

        return View("Edit", model);
    }



    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(RoleManagementModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand("sp_ManageAspNetRoles", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Action", "UPDATE");
            cmd.Parameters.AddWithValue("@RoleId", model.RoleId);
            cmd.Parameters.AddWithValue("@RoleName", model.RoleName);
            cmd.Parameters.AddWithValue("@Discriminator", model.Discriminator ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Description", model.Description ?? (object)DBNull.Value);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        return RedirectToAction("Index");
    }


}

