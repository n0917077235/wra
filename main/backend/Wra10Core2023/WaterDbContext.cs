using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Wra10Core2023
{
    public class WaterDbContext: DbContext
    {
        public WaterDbContext(DbContextOptions<WaterDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

                var connectionString = configuration.GetConnectionString("Water2022");

                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }
}
