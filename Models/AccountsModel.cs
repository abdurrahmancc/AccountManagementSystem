using System.ComponentModel.DataAnnotations;

namespace AccountManagementSystem.Models
{
    public class AccountsModel
    {

        [Key]
        public Guid Id { get; set; }

        [Required]
        public string AccountName { get; set; }

        public Guid? ParentId { get; set; }

        public AccountsModel Parent { get; set; }
    }

}
