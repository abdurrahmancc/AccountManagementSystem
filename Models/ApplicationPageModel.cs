using Microsoft.EntityFrameworkCore;

namespace AccountManagementSystem.Models
{
    [Keyless]
    public class ApplicationPageModel
    {
        public int PageId { get; set; }
        public string PageName { get; set; }
        public string PageUrl { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class RolePermissionWithPageInfo
    {
        public int Id { get; set; }
        public Guid RoleId { get; set; }
        public int PageId { get; set; }
        public string PageName { get; set; }
        public string PageUrl { get; set; }
        public string Description { get; set; }
        public bool IsAllowed { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
    }


}
