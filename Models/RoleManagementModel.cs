namespace AccountManagementSystem.Models
{
    public class RoleManagementModel
    {

            public Guid RoleId { get; set; }
            public string RoleName { get; set; }
            public string Description { get; set; }
            public string Discriminator { get; set; }
        

    }
}
