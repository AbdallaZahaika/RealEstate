using RealEstate.Models;
using Microsoft.EntityFrameworkCore;

namespace RealEstate.DBContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<City> Cities { get; set; }
        public DbSet<ContactUs> ContactUs { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<MaintenanceContract> MaintenanceContracts { get; set; }
        public DbSet<RealEstateFile> RealEstateFiles { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<User> Users { get; set; }

    }
}
