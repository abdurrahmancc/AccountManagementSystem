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

        public async Task<IActionResult> Index()
        {
            var allAccounts = await GetAllAccountsAsync();
            var treeData = BuildTree(allAccounts);
            return View(treeData);
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
                var viewModel = new ChartOfAccountCreateViewModel
                {
                    ParentList = GetParentChartOfAccounts()
                };
                return View(viewModel);
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

            return RedirectToAction("create", "ChartOfAccount");
        }

        public async Task<List<ViewChartOfAccountModel>> GetAllAccountsAsync()
        {
            var accounts = new List<ViewChartOfAccountModel>();
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT Id, ParentId, AccountHead, Code FROM ChartOfAccount", conn))
                {
                    conn.Open();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            accounts.Add(new ViewChartOfAccountModel
                            {
                                Id = reader.GetInt32(0),
                                ParentId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                                AccountHead = reader.GetString(2),
                                Code = reader.GetString(3)
                            });
                        }
                    }
                }
            }
            return accounts;
        }



        [HttpGet]
        public async Task<IActionResult> GetNextAccountCode(int? parentId)
        {
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await conn.OpenAsync();

                string newCode = "";

                if (parentId.HasValue)
                {
                    string parentCode = "";

                    // 1. Get parent code
                    using (SqlCommand getParentCodeCmd = new SqlCommand("SELECT Code FROM ChartOfAccount WHERE Id = @ParentId", conn))
                    {
                        getParentCodeCmd.Parameters.AddWithValue("@ParentId", parentId.Value);
                        var parentCodeResult = await getParentCodeCmd.ExecuteScalarAsync();
                        parentCode = parentCodeResult?.ToString() ?? "";
                    }

                    // 2. Get max child suffix under this parent
                    using (SqlCommand getMaxChildCodeCmd = new SqlCommand(
                        "SELECT MAX(Code) FROM ChartOfAccount WHERE ParentId = @ParentId AND Code LIKE @CodePrefix + '%'", conn))
                    {
                        getMaxChildCodeCmd.Parameters.AddWithValue("@ParentId", parentId.Value);
                        getMaxChildCodeCmd.Parameters.AddWithValue("@CodePrefix", parentCode);

                        var maxChildCodeResult = await getMaxChildCodeCmd.ExecuteScalarAsync();

                        int nextSuffix = 1;

                        if (maxChildCodeResult != null && maxChildCodeResult != DBNull.Value)
                        {
                            string lastCode = maxChildCodeResult.ToString();

                            // extract suffix part only
                            string suffix = lastCode.Substring(parentCode.Length);
                            if (int.TryParse(suffix, out int parsedSuffix))
                            {
                                nextSuffix = parsedSuffix + 1;
                            }
                        }

                        newCode = parentCode + nextSuffix.ToString("D2"); // Always 2 digits
                    }
                }
                else
                {
                    // Root level
                    using (SqlCommand getMaxRootCodeCmd = new SqlCommand("SELECT MAX(Code) FROM ChartOfAccount WHERE ParentId IS NULL", conn))
                    {
                        var maxRootCodeResult = await getMaxRootCodeCmd.ExecuteScalarAsync();
                        int nextRootCode = 1;

                        if (maxRootCodeResult != null && maxRootCodeResult != DBNull.Value)
                        {
                            if (int.TryParse(maxRootCodeResult.ToString(), out int parsed))
                            {
                                nextRootCode = parsed + 1;
                            }
                        }

                        newCode = nextRootCode.ToString("D2"); // Always 2 digits
                    }
                }

                return Json(new { code = newCode });
            }
        }



        private List<ViewChartOfAccountModel> BuildTree(List<ViewChartOfAccountModel> flatList, int? parentId = null)
        {
            return flatList
                .Where(x => x.ParentId == parentId)
                .Select(x => new ViewChartOfAccountModel
                {
                    Id = x.Id,
                    ParentId = x.ParentId,
                    Code = x.Code,
                    AccountHead = x.AccountHead,
                    Children = BuildTree(flatList, x.Id)
                }).ToList();
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
