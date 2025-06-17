using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using AccountManagementSystem.Models;

public class ApplicationPagesController : Controller
{
    private readonly string _connectionString;

    public ApplicationPagesController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<IActionResult> Index()
    {
        var pages = new List<ApplicationPageModel>();

        using (var conn = new SqlConnection(_connectionString))
        {
            using (var cmd = new SqlCommand("sp_ManageApplicationPage", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECT");

                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        pages.Add(new ApplicationPageModel
                        {
                            PageId = Convert.ToInt32(reader["PageId"]),
                            PageName = reader["PageName"].ToString(),
                            PageUrl = reader["PageUrl"].ToString(),
                            Description = reader["Description"]?.ToString(),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                            UpdatedAt = reader["UpdatedAt"] as DateTime?
                        });
                    }
                }
            }
        }

        return View(pages);
    }


    [HttpPost]
    public async Task<IActionResult> Save(ApplicationPageModel model)
    {
        string action = model.PageId == 0 ? "INSERT" : "UPDATE";

        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand("sp_ManageApplicationPage", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Action", action);
            cmd.Parameters.AddWithValue("@PageId", model.PageId);
            cmd.Parameters.AddWithValue("@PageName", model.PageName);
            cmd.Parameters.AddWithValue("@PageUrl", model.PageUrl);
            cmd.Parameters.AddWithValue("@Description", model.Description ?? (object)DBNull.Value);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        return RedirectToAction("Index");
    }


    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand("sp_ManageApplicationPage", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Action", "DELETE");
            cmd.Parameters.AddWithValue("@PageId", id);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        return RedirectToAction("Index");
    }


    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ApplicationPageModel page = null;

        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand("sp_ManageApplicationPage", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Action", "SELECT");
            // SELECT SP যদি একক রেকর্ড ফিরায় এমন হলে
            // কিন্তু এখন আমরা SELECT সব পেজ নিচ্ছি তাই একটু আলাদা SP দরকার single select এর জন্য
            // অথবা Controller এ ফিল্টার করতে পারো
        }

        // সহজ উপায়: সব পেজ নিয়ে এসে LINQ দিয়ে filter:
        var pages = new List<ApplicationPageModel>();
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand("sp_ManageApplicationPage", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Action", "SELECT");

            await conn.OpenAsync();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    pages.Add(new ApplicationPageModel
                    {
                        PageId = Convert.ToInt32(reader["PageId"]),
                        PageName = reader["PageName"].ToString(),
                        PageUrl = reader["PageUrl"].ToString(),
                        Description = reader["Description"]?.ToString(),
                        CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                        UpdatedAt = reader["UpdatedAt"] as DateTime?
                    });
                }
            }
        }

        page = pages.FirstOrDefault(p => p.PageId == id);
        if (page == null)
            return NotFound();

        return View(page);
    }

}
