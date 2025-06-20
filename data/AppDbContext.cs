using AccountManagementSystem.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;

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
        public DbSet<RolePermissionModel> RolePermissions { get; set; }
        public DbSet<RolePermissionPageDto> RolePermissionPageDto { get; set; }



        public async Task<List<ApplicationPageModel>> GetAllowedPagesByUserIdAsync(string userId)
        {
            if (!Guid.TryParse(userId, out Guid parsedUserId))
                throw new ArgumentException("Invalid UserId");

            var userIdParam = new SqlParameter("@UserId", parsedUserId);

            var allowedPages = await ApplicationPages
                .FromSqlRaw("EXEC dbo.sp_GetAllowedPagesByUserId @UserId", userIdParam)
                .ToListAsync();

            return allowedPages;
        }



        public async Task<List<ApplicationPageModel>> GetApplicationPagesAsync(string userId)
        {
            if (!Guid.TryParse(userId, out Guid parsedUserId))
                throw new ArgumentException("Invalid UserId");

            Guid? roleId = null;
            var connString = this.Database.GetDbConnection().ConnectionString;

            using (var conn = new SqlConnection(connString))
            {
                await conn.OpenAsync();

                using (var cmd = new SqlCommand("sp_GetRoleIdByUserId", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", parsedUserId));

                    var result = await cmd.ExecuteScalarAsync();
                    if (result != null && Guid.TryParse(result.ToString(), out Guid parsedRoleId))
                    {
                        roleId = parsedRoleId;
                    }
                }
            }

            if (roleId == null) return new List<ApplicationPageModel>();
            var allPermissions = await RolePermissions.FromSqlRaw("EXEC dbo.sp_GetAllRolePermissions").ToListAsync();


            List<int> allowedPageIds = new List<int>();
            foreach (var rp in allPermissions)
            {
                if (rp.RoleId == roleId && rp.IsAllowed)
                {
                    if (!allowedPageIds.Contains(rp.PageId))
                    {
                        allowedPageIds.Add(rp.PageId);
                    }
                }
            }


            var allPages = await ApplicationPages.FromSqlRaw("EXEC sp_ManageApplicationPage @Action = 'SELECT'").ToListAsync();


            var allowedPages = new List<ApplicationPageModel>();
            foreach (var page in allPages)
            {
                if (allowedPageIds.Contains(page.PageId))
                {
                    allowedPages.Add(page);
                }
            }


            return allowedPages;
        }



    }
}
