using System.ComponentModel.DataAnnotations;

namespace AccountManagementSystem.Models
{
    public class AccountsModel
    {

        [Key]
        public Guid UserId { get; set; }

        [Required]
        public string FullName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public Guid Role { get; set; }
    }
    public class AccountViewModel
    {

        [Key]
        public Guid UserId { get; set; }

        [Required]
        public string FullName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required(ErrorMessage = "Please select a role.")]
        public string SelectedRoleId { get; set; }
        public List<RoleModel> Roles { get; set; }
    }

    public class RoleModel
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
    }

    public class RoleViewModel
    {
        public string SelectedRoleId { get; set; }
        public List<RoleModel> Roles { get; set; }
    }

}
