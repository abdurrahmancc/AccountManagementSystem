using System;
using System.ComponentModel.DataAnnotations;
using AccountManagementSystem.Models;


namespace AccountManagementSystem.Models
{
    public class RolePermissionModel
    {
        public int Id { get; set; }

        [Required]
        public Guid RoleId { get; set; }

        [Required]
        [StringLength(256)]
        public string PageName { get; set; }

        public bool IsAllowed { get; set; }

        public DateTime CreatedAt { get; set; }

        [StringLength(256)]
        public string UpdatedBy { get; set; }
    }
    public class RolePermissionsViewModel
    {
        public List<RolePermissionModel> RolePermissions { get; set; }
        public List<RoleModel> Roles { get; set; }
    }

}