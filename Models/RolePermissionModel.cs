using System;
using System.ComponentModel.DataAnnotations;
using AccountManagementSystem.Models;
using Microsoft.EntityFrameworkCore;


namespace AccountManagementSystem.Models
{
    public class RolePermissionModel
    {
        public int Id { get; set; }

        [Required]
        public Guid RoleId { get; set; }

        [Required]
        public int PageId { get; set; }


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

    public class NewRolePermissionViewModel
    {
        public int Id { get; set; }
        public Guid RoleId { get; set; }
        public int PageId { get; set; }
        public string PageName { get; set; }
        public bool IsAllowed { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
    }

    [Keyless]
    public class RolePermissionPageDto
    {
        public Guid? Id { get; set; }
        public Guid? RoleId { get; set; }
        public Guid? PageId { get; set; }
        public string PageName { get; set; }
        public string PageUrl { get; set; }
        public string Description { get; set; }
        public bool? IsAllowed { get; set; }
        public DateTime? CreatedAt { get; set; }
        //public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
    }



}