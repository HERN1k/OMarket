using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using OMarket.Infrastructure.Data.Contexts.ApplicationContext;

namespace OMarket.Infrastructure.Data.Contexts
{
    public class AppDBContextFactory : IDesignTimeDbContextFactory<AppDBContext>
    {
        public AppDBContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("POSTGRESQL");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("POSTGRESQL", "The connection string environment variable is not set.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<AppDBContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new AppDBContext(optionsBuilder.Options);
        }
    }
}
