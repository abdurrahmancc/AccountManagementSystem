using AccountManagementSystem.data;
using AccountManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Security.Claims;


public class RolePermissionController : Controller
{
    private readonly string _connectionString;
    private readonly AppDbContext _context;

    public RolePermissionController(IConfiguration configuration, AppDbContext context)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
        _context = context;
    }



    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Save(RolePermissionModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

        try
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("sp_ManageRolePermission", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    string action = model.Id == 0 ? "INSERT" : "UPDATE";

                    var pageName = await GetPageNameByIdAsync(model.PageId);

                    cmd.Parameters.AddWithValue("@Action", action);
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@RoleId", model.RoleId);
                    cmd.Parameters.AddWithValue("@PageId", model.PageId);
                    //cmd.Parameters.AddWithValue("@PageName", model.PageName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsAllowed", model.IsAllowed);
                    cmd.Parameters.AddWithValue("@CreatedAt", model.CreatedAt == DateTime.MinValue ? DateTime.Now : model.CreatedAt);
                    cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return Ok(new { success = true });
        }
        catch (SqlException ex)
        {
            if (ex.Message.Contains("already has permission"))
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            return StatusCode(500, new { success = false, message = "Database error occurred.", detail = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An unexpected error occurred.", detail = ex.Message });
        }
    }




    [HttpPost]
    [Authorize(Roles = "Admin")]
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

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        var rolePermissions = new List<RolePermissionModel>();
        var roles = new List<RoleModel>();
        var pages = new List<ApplicationPageModel>();

        using (var conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();

            using (var cmd = new SqlCommand("sp_GetAllRolePermissions", conn))
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
                            PageId = reader["PageId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["PageId"]),
                            PageName = reader["PageName"].ToString(),
                            IsAllowed = (bool)reader["IsAllowed"],
                            CreatedAt = (DateTime)reader["CreatedAt"],
                            UpdatedBy = reader["UpdatedBy"].ToString()
                        });
                    }
                }
            }


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


            using (var cmdPages = new SqlCommand("SELECT PageName, PageId FROM ApplicationPages", conn))
            {
                using (var readerPages = await cmdPages.ExecuteReaderAsync())
                {
                    while (await readerPages.ReadAsync())
                    {
                        pages.Add(new ApplicationPageModel
                        {
                            PageName = readerPages["PageName"].ToString(),
                            PageId = readerPages["PageId"] == DBNull.Value ? 0 : Convert.ToInt32(readerPages["PageId"])
                        });
                    }
                }
            }
        }

        ViewBag.Roles = roles;
        ViewBag.Pages = pages;
        ViewBag.RolePermissions = rolePermissions;

        return View();
    }

    [HttpGet("AllowedPermissions/{roleId}")]
    public async Task<IActionResult> AllowedPermissions(Guid roleId)
    {
        var allowedPermissions = await GetAllowedRolePermissionsByRoleAsync(roleId);
        return Ok(allowedPermissions);
    }


    public async Task<List<RolePermissionModel>> GetRolePermissionsByRoleAsync(Guid roleId)
    {
        var actionParam = new SqlParameter("@Action", "SELECT");
        var roleIdParam = new SqlParameter("@RoleId", roleId);

        var sql = "EXEC sp_ManageRolePermission @Action, @RoleId";

        var permissions = await _context.RolePermissions
            .FromSqlRaw(sql, actionParam, roleIdParam)
            .ToListAsync();

        return permissions;
    }

    public async Task<List<RolePermissionModel>> GetAllowedRolePermissionsByRoleAsync(Guid roleId)
    {
        var allPermissions = await GetRolePermissionsByRoleAsync(roleId);
        return allPermissions.Where(p => p.IsAllowed).ToList();
    }

    private async Task<string> GetPageNameByIdAsync(int pageId)
    {
        if (pageId <= 0)
            return null;

        string pageName = null;

        using (var conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();

            using (var cmd = new SqlCommand("SELECT PageName FROM ApplicationPages WHERE PageId = @PageId", conn))
            {
                cmd.Parameters.AddWithValue("@PageId", pageId);

                var result = await cmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                {
                    pageName = result.ToString();
                }
            }
        }

        return pageName;
    }


}
