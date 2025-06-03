using AccountManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountManagementSystem.data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public DbSet<AccountsModel> Accounts { get; set; }

    }
}
