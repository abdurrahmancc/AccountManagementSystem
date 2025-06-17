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

        public DbSet<ApplicationPageModel> ApplicationPages { get; set; }

        public async Task<List<ApplicationPageModel>> GetApplicationPagesAsync()
        {
            return await ApplicationPages
                .FromSqlRaw("EXEC sp_ManageApplicationPage @Action = 'SELECT'")
                .ToListAsync();
        }

    }
}
