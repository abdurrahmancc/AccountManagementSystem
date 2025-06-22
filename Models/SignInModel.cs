using System.ComponentModel.DataAnnotations;

namespace AccountManagementSystem.Models
{
    public class SignInModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "admin@gmail.com";

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "admin@";
    }
}
