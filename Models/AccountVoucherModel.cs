using AccountManagementSystem.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AccountManagementSystem.Models
{
    public class AccountVoucherModel
    {

        public int VoucherId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime VoucherDate { get; set; }

        [Required]
        public VoucherType VoucherType { get; set; }

        public string VoucherNumber { get; set; }

        public string ReferenceNo { get; set; }

        public string Note { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class VoucherDetailModel
    {
        public TransactionType TransactionType { get; set; }
        public int AccountHeadId { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public string Description { get; set; }
    }

    public class AccountVoucherWithDetailsModel
    {
        public int VoucherId { get; set; }
        public DateTime VoucherDate { get; set; }
        public VoucherType VoucherType { get; set; }
        public string ReferenceNo { get; set; }
        public string Note { get; set; }
        public string VoucherNumber { get; set; }

        public List<VoucherDetailModel> Details { get; set; }
    }



}
