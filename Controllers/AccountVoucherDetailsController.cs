using AccountManagementSystem.Enums;
using AccountManagementSystem.Models;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AccountManagementSystem.Controllers
{
    public class AccountVoucherDetailsController : Controller
    {
        private readonly IConfiguration _configuration;

        public AccountVoucherDetailsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // VoucherDetails লিস্ট দেখানোর জন্য (যেমন Details page এ দেখাবে)
        public IActionResult Index(int voucherId)
        {
            var details = new List<AccountVoucherDetailsModels>();

            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("SELECT * FROM VoucherDetails WHERE VoucherId = @VoucherId", conn);
            cmd.Parameters.AddWithValue("@VoucherId", voucherId);
            conn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                details.Add(new AccountVoucherDetailsModels
                {
                    Id = (int)reader["Id"],
                    VoucherId = (int)reader["VoucherId"],
                    TransactionType = (TransactionType)reader.GetInt32(reader.GetOrdinal("TransactionType")),
                    AccountHeadId = (int)reader["AccountHeadId"],
                    DebitAmount = (decimal)reader["DebitAmount"],
                    CreditAmount = (decimal)reader["CreditAmount"],
                    Description = reader["Description"] as string
                });
            }

            ViewBag.VoucherId = voucherId;
            return View(details);
        }

        public IActionResult Create(int voucherId, int id = 0)
        {
            var model = new AccountVoucherDetailsModels { VoucherId = voucherId };

            if (id > 0)
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                using var cmd = new SqlCommand("SELECT * FROM VoucherDetails WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    model.Id = (int)reader["Id"];
                    model.VoucherId = (int)reader["VoucherId"];
                    model.TransactionType = (TransactionType)reader.GetInt32(reader.GetOrdinal("TransactionType"));
                    model.AccountHeadId = (int)reader["AccountHeadId"];
                    model.DebitAmount = (decimal)reader["DebitAmount"];
                    model.CreditAmount = (decimal)reader["CreditAmount"];
                    model.Description = reader["Description"] as string;
                }
            }

            ViewBag.AccountHeadList = GetAccountHeadList();

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AccountVoucherDetailsModels model)
        {
            if (ModelState.IsValid)
            {
                if (model.VoucherId <= 0)
                {
                    TempData["ErrorMessage"] = "Please save the voucher first before adding details.";
                    ViewBag.AccountHeadList = GetAccountHeadList();
                    return View(model);
                }

                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                using var cmd = new SqlCommand("sp_SaveVoucherDetail", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                var paramId = new SqlParameter("@Id", SqlDbType.Int) { Direction = ParameterDirection.InputOutput, Value = model.Id };
                cmd.Parameters.Add(paramId);

                cmd.Parameters.AddWithValue("@VoucherId", model.VoucherId);
                cmd.Parameters.AddWithValue("@TransactionType", (int)model.TransactionType);
                cmd.Parameters.AddWithValue("@AccountHeadId", model.AccountHeadId);
                cmd.Parameters.AddWithValue("@DebitAmount", model.DebitAmount);
                cmd.Parameters.AddWithValue("@CreditAmount", model.CreditAmount);
                cmd.Parameters.AddWithValue("@Description", (object)model.Description ?? DBNull.Value);

                conn.Open();

                // Voucher existence check
                using var checkCmd = new SqlCommand("SELECT COUNT(1) FROM AccountVouchers WHERE VoucherId = @VoucherId", conn);
                checkCmd.Parameters.AddWithValue("@VoucherId", model.VoucherId);
                int count = (int)checkCmd.ExecuteScalar();

                if (count == 0)
                {
                    TempData["ErrorMessage"] = "Voucher does not exist.";
                    ViewBag.AccountHeadList = GetAccountHeadList();
                    return View(model);
                }

                cmd.ExecuteNonQuery();

                model.Id = (int)paramId.Value;

                TempData["SuccessMessage"] = "Voucher detail saved successfully.";
                return RedirectToAction("Index", new { voucherId = model.VoucherId });
            }

            // Model validation fail হলে dropdown পুনরায় লোড করতে হবে
            ViewBag.AccountHeadList = GetAccountHeadList();
            return View(model);
        }



        private List<SelectListItem> GetAccountHeadList()
        {
            var accountHeads = new List<SelectListItem>();

            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("SELECT Id, AccountHead FROM ChartOfAccount WHERE IsActive = 1 ORDER BY AccountHead", conn);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                accountHeads.Add(new SelectListItem
                {
                    Value = reader["Id"].ToString(),
                    Text = reader["AccountHead"].ToString()
                });
            }

            return accountHeads;
        }

    }
}