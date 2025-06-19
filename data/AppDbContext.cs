using AccountManagementSystem.Models;
using Microsoft.Data.SqlClient;
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
        public DbSet<RolePermissionModel> RolePermissions { get; set; }

        public async Task<List<ApplicationPageModel>> GetApplicationPagesAsync()
        {
            var items = await ApplicationPages
                .FromSqlRaw("EXEC sp_ManageApplicationPage @Action = 'SELECT'")
                .ToListAsync();

            //foreach(var item in items)
            //{
            //    item
            //}
            //return items;
            return await ApplicationPages
                  .FromSqlRaw("EXEC sp_ManageApplicationPage @Action = 'SELECT'")
                  .ToListAsync();
        }





        public async Task<List<RolePermissionModel>> GetRolePermissionsByRoleAsync(Guid roleId)
        {
            var actionParam = new SqlParameter("@Action", "SELECT");
            var roleIdParam = new SqlParameter("@RoleId", roleId);

            var sql = "EXEC sp_ManageRolePermission @Action, @RoleId";

            var permissions = await RolePermissions
                .FromSqlRaw(sql, actionParam, roleIdParam)
                .ToListAsync();

            return permissions;
        }

        public async Task<List<RolePermissionModel>> GetAllowedRolePermissionsByRoleAsync(Guid roleId)
        {
            var allPermissions = await GetRolePermissionsByRoleAsync(roleId);
            return allPermissions.Where(p => p.IsAllowed).ToList();
        }

    }
}
