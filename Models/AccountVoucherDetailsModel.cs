using AccountManagementSystem.Enums;
using System.ComponentModel.DataAnnotations;

namespace AccountManagementSystem.Models
{
    public class AccountVoucherDetailsModels
    {
            public int Id { get; set; }

            [Required]
            public TransactionType TransactionType { get; set; }

            [Required]
            public int AccountHeadId { get; set; }

            [Required]
            [Range(0.00, double.MaxValue, ErrorMessage = "Debit amount must be zero or more")]
            public decimal DebitAmount { get; set; }

            [Required]
            [Range(0.00, double.MaxValue, ErrorMessage = "Credit amount must be zero or more")]
            public decimal CreditAmount { get; set; }

            public string Description { get; set; }

            public int VoucherId { get; set; }
    }
}
