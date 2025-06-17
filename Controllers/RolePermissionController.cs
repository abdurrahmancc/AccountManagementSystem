using AccountManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Data;


public class RolePermissionController : Controller
{
    private readonly string _connectionString;

    public RolePermissionController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    // Insert or Update - POST
    [HttpPost]
    public async Task<IActionResult> Save(RolePermissionModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        using (var conn = new SqlConnection(_connectionString))
        {
            using (var cmd = new SqlCommand("sp_ManageRolePermission", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Action", model.Id == 0 ? "INSERT" : "UPDATE");
                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@RoleId", model.RoleId);
                cmd.Parameters.AddWithValue("@PageName", model.PageName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IsAllowed", model.IsAllowed);
                cmd.Parameters.AddWithValue("@CreatedAt", model.CreatedAt == DateTime.MinValue ? DateTime.Now : model.CreatedAt);
                cmd.Parameters.AddWithValue("@UpdatedBy", model.UpdatedBy ?? (object)DBNull.Value);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        return Ok(new { success = true });
    }

    // Delete - POST
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid Id");

        using (var conn = new SqlConnection(_connectionString))
        {
            using (var cmd = new SqlCommand("sp_ManageRolePermission", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Action", "DELETE");
                cmd.Parameters.AddWithValue("@Id", id);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        return Ok(new { success = true });
    }

    // Optional: List all RolePermissions (Read)
    public async Task<IActionResult> Index()
    {
        var rolePermissions = new List<RolePermissionModel>();
        var roles = new List<RoleModel>(); // RoleModel = RoleId + RoleName রাখবে

        using (var conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();

            // RolePermissions SP
            using (var cmd = new SqlCommand("sp_ManageRolePermission", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        rolePermissions.Add(new RolePermissionModel
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            RoleId = (Guid)reader["RoleId"],
                            PageName = reader["PageName"].ToString(),
                            IsAllowed = (bool)reader["IsAllowed"],
                            CreatedAt = (DateTime)reader["CreatedAt"],
                            UpdatedBy = reader["UpdatedBy"].ToString()
                        });
                    }
                }
            }

            // Roles SP
            using (var cmdRoles = new SqlCommand("sp_GetAllRoles", conn))
            {
                cmdRoles.CommandType = CommandType.StoredProcedure;

                using (var readerRoles = await cmdRoles.ExecuteReaderAsync())
                {
                    while (await readerRoles.ReadAsync())
                    {
                        roles.Add(new RoleModel
                        {
                            RoleId = (Guid)readerRoles["RoleId"],
                            RoleName = readerRoles["RoleName"].ToString()
                        });
                    }
                }
            }
        }

        // ViewModel-এ দুইটা লিস্ট পাঠাও
        var vm = new RolePermissionsViewModel
        {
            RolePermissions = rolePermissions,
            Roles = roles
        };

        return View(vm);
    }


}
