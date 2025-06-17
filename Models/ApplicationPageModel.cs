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

}
