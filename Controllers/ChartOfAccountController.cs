using AccountManagementSystem.Models;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace AccountManagementSystem.Controllers
{
    public class ChartOfAccountController : Controller
    {
            private readonly IConfiguration _configuration;

            public ChartOfAccountController(IConfiguration configuration)
            {
                _configuration = configuration;
            }

        [HttpGet]
        public IActionResult Create()
        {

            var model = new ChartOfAccountCreateViewModel
            {
                ParentList = GetParentChartOfAccounts()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChartOfAccountModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await conn.OpenAsync();

                int parentLevel = 0;

                if (model.ParentId.HasValue)
                {
                    using (SqlCommand getParentCmd = new SqlCommand("SELECT Level FROM ChartOfAccount WHERE Id = @ParentId", conn))
                    {
                        getParentCmd.Parameters.AddWithValue("@ParentId", model.ParentId.Value);
                        var result = await getParentCmd.ExecuteScalarAsync();
                        if (result != null && int.TryParse(result.ToString(), out int lvl))
                        {
                            parentLevel = lvl;
                        }
                    }
                }

                using (SqlCommand cmd = new SqlCommand("sp_InsertChartOfAccount", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@ParentId", (object)model.ParentId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AccountHead", model.AccountHead);
                    cmd.Parameters.AddWithValue("@Code", model.Code);
                    cmd.Parameters.AddWithValue("@IsLastLevel", model.IsLastLevel);
                    cmd.Parameters.AddWithValue("@IsParent", model.IsParent);
                    cmd.Parameters.AddWithValue("@Description", (object)model.Description ?? DBNull.Value);

                    // এখানে মূল userId ব্যাবহার করো createdBy এর জন্য
                    if (!string.IsNullOrEmpty(userId))
                    {
                        cmd.Parameters.AddWithValue("@CreatedBy", Guid.Parse(userId));
                        cmd.Parameters.AddWithValue("@ModifiedBy", Guid.Parse(userId));
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@CreatedBy", DBNull.Value);
                        cmd.Parameters.AddWithValue("@ModifiedBy", DBNull.Value);
                    }

                    cmd.Parameters.AddWithValue("@Level", parentLevel + 1);

                    var newId = await cmd.ExecuteScalarAsync();
                }
            }

            return RedirectToAction("Create");
        }




        private List<SelectListItem> GetParentChartOfAccounts()
        {
            var list = new List<SelectListItem>();

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetParentChartOfAccounts", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new SelectListItem
                            {
                                Value = reader["Id"].ToString(),
                                Text = reader["AccountHead"].ToString()
                            });
                        }
                    }

                    conn.Close();
                }
            }

            return list;
        }

    }
}
