using AccountManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;

namespace AccountManagementSystem.Controllers
{
    public class AccountVoucherController : Controller
    {
        private readonly IConfiguration _configuration;

        public AccountVoucherController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Authorize(Roles = "Admin,Accountant")]
        public IActionResult Create()
        {
            var model = new AccountVoucherWithDetailsModel
            {
                VoucherDate = DateTime.Today
            };
            ViewBag.AccountHeadList = GetAccountHeadList();

            return View(model);
        }


        [HttpPost]
        [Authorize(Roles = "Admin,Accountant")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AccountVoucherWithDetailsModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                    using var cmd = new SqlCommand("sp_SaveAccountVoucherWithDetails", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@VoucherDate", model.VoucherDate);
                    cmd.Parameters.AddWithValue("@VoucherType", (int)model.VoucherType);
                    cmd.Parameters.AddWithValue("@ReferenceNo", (object)model.ReferenceNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Note", (object)model.Note ?? DBNull.Value);

                    // Prepare DataTable for TVP
                    var detailTable = new DataTable();
                    detailTable.Columns.Add("TransactionType", typeof(int));
                    detailTable.Columns.Add("AccountHeadId", typeof(int));
                    detailTable.Columns.Add("DebitAmount", typeof(decimal));
                    detailTable.Columns.Add("CreditAmount", typeof(decimal));
                    detailTable.Columns.Add("Description", typeof(string));

                    foreach (var detail in model.Details)
                    {
                        detailTable.Rows.Add(
                            (int)detail.TransactionType,
                            detail.AccountHeadId,
                            detail.DebitAmount,
                            detail.CreditAmount,
                            (object)detail.Description ?? DBNull.Value);
                    }

                    var tvpParam = cmd.Parameters.AddWithValue("@Details", detailTable);
                    tvpParam.SqlDbType = SqlDbType.Structured;
                    tvpParam.TypeName = "dbo.AccountVoucherDetailsType";

                    var paramVoucherId = new SqlParameter("@VoucherId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    var paramVoucherNumber = new SqlParameter("@VoucherNumber", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(paramVoucherId);
                    cmd.Parameters.Add(paramVoucherNumber);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    int newVoucherId = (int)paramVoucherId.Value;
                    string newVoucherNumber = paramVoucherNumber.Value.ToString();
                    TempData["SuccessMessage"] = $"Voucher saved with number: {newVoucherNumber}";
                    //return RedirectToAction("Details", new { id = newVoucherId });
                    return RedirectToAction("create", "AccountVoucher");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }

            ViewBag.AccountHeadList = GetAccountHeadList();
            return View(model);
        }



        [HttpGet]
        public IActionResult GetNewVoucherNumber(int voucherType)
        {
            // voucherType: 1=Journal, 2=Payment, 3=Receipt

            string prefix = voucherType switch
            {
                1 => "JV",
                2 => "PV",
                3 => "RV",
                _ => "XX"
            };

            var today = DateTime.Now;
            string datePart = today.ToString("MMdd");
            int lastSerial = 0;

            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand(@"
    SELECT ISNULL(MAX(CAST(RIGHT(VoucherNumber, 4) AS INT)), 0)
    FROM AccountVoucher
    WHERE LEFT(VoucherNumber, 2) = @prefix AND SUBSTRING(VoucherNumber, 3, 4) = @datePart", conn);


            cmd.Parameters.AddWithValue("@prefix", prefix);
            cmd.Parameters.AddWithValue("@datePart", datePart);

            conn.Open();
            var result = cmd.ExecuteScalar();
            if (result != DBNull.Value && result != null)
                lastSerial = Convert.ToInt32(result);

            lastSerial++; // next serial

            string newVoucherNumber = prefix + datePart + lastSerial.ToString("D4");

            return Json(new { voucherNumber = newVoucherNumber });
        }



        public IActionResult Details()
        {
            var list = new List<AccountVoucherModel>();

            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("SELECT VoucherId, VoucherDate, VoucherType, VoucherNumber, ReferenceNo, Note, CreatedAt FROM AccountVoucher ORDER BY VoucherDate DESC", conn);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new AccountVoucherModel
                {
                    VoucherId = (int)reader["VoucherId"],
                    VoucherDate = (DateTime)reader["VoucherDate"],
                    VoucherType = (AccountManagementSystem.Enums.VoucherType)(int)reader["VoucherType"],
                    VoucherNumber = reader["VoucherNumber"] as string,
                    ReferenceNo = reader["ReferenceNo"] as string,
                    Note = reader["Note"] as string,
                    CreatedAt = (DateTime)reader["CreatedAt"]
                });
            }

            return View(list);
        }


        [HttpPost]
        [Authorize(Roles = "Admin,Accountant")]
        public IActionResult Delete(int id)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            conn.Open();

            using var tran = conn.BeginTransaction();

            try
            {
                using (var cmd1 = new SqlCommand("DELETE FROM AccountVoucherDetails WHERE VoucherId = @VoucherId", conn, tran))
                {
                    cmd1.Parameters.AddWithValue("@VoucherId", id);
                    cmd1.ExecuteNonQuery();
                }

                using (var cmd2 = new SqlCommand("DELETE FROM AccountVoucher WHERE VoucherId = @VoucherId", conn, tran))
                {
                    cmd2.Parameters.AddWithValue("@VoucherId", id);
                    cmd2.ExecuteNonQuery();
                }

                tran.Commit();
            }
            catch (Exception ex)
            {
                tran.Rollback();
                throw;
            }

            return RedirectToAction("Details"); 
        }


        private List<SelectListItem> GetAccountHeadList()
        {
            var accountHeads = new List<SelectListItem>();

            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            //using var cmd = new SqlCommand("SELECT Id, AccountHead FROM ChartOfAccount WHERE IsActive = 1 ORDER BY AccountHead", conn);
            using var cmd = new SqlCommand("SELECT COA.Id, COA.Code +' - '+ COAP.AccountHead +' > ' + COA.AccountHead as AccountHead FROM ChartOfAccount COA left join ChartOfAccount COAP on COA.ParentId = COAP.Id  WHERE COA.IsActive = 1 and COA.IsLastLevel = 1 ORDER BY COA.AccountHead", conn);
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
