using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CattleFarm.Models
{
    /// <summary>
    /// Design-time factory used by EF Core CLI tools (migrations, scaffolding).
    /// Reads connection string from appsettings.json at the project root.
    /// </summary>
    public class CattleFarmDbContextFactory : IDesignTimeDbContextFactory<CattleFarmDbContext>
    {
        public CattleFarmDbContext CreateDbContext(string[] args)
        {
            var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<CattleFarmDbContext>();
            optionsBuilder.UseSqlServer(config.GetConnectionString("DefaultConnection"))
                .ConfigureWarnings(warnings =>
                    warnings.Ignore(RelationalEventId.PendingModelChangesWarning));

            return new CattleFarmDbContext(optionsBuilder.Options);
        }
    }
}
