using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using AccountManagementSystem.Models;

public class RoleManagementController : Controller
{
    private readonly string _connectionString;

    public RoleManagementController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<IActionResult> Index()
    {
        var roles = new List<RoleManagementModel>();

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

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
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
}

