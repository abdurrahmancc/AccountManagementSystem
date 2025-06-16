using Microsoft.AspNetCore.Mvc.Rendering;

namespace AccountManagementSystem.Models
{
    public class ChartOfAccountModel
    {
        public int? ParentId { get; set; }
        public string AccountHead { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool IsLastLevel { get; set; }
        public bool IsParent { get; set; }
        public string? Description { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? ModifiedBy { get; set; }
        public int Level { get; set; }
    }

    public class ChartOfAccountCreateViewModel
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string AccountHead { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool IsLastLevel { get; set; }
        public bool IsParent { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? ModifiedBy { get; set; }
        public int Level { get; set; }
        public List<SelectListItem>? ParentList { get; set; }

    }

    public class ViewChartOfAccountModel
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string AccountHead { get; set; }

        public List<ViewChartOfAccountModel>? Children { get; set; }
    }


}
