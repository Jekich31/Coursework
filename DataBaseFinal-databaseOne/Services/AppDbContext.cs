using Microsoft.EntityFrameworkCore;

namespace ProjectFIN.models;

public class AppDbContext : DbContext
{
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<ElectricCar> ElectricCars { get; set; }
    public DbSet<GasolineCar> GasolineCars { get; set; }
    public DbSet<Coordinate> Coordinates { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Data Source=.\SQLEXPRESS;Database=CarTelematicsDb;Integrated Security=True;TrustServerCertificate=True;");
        //optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Vehicle>()
            .HasDiscriminator<string>("Discriminator")
            .HasValue<GasolineCar>("GasolineCar")
            .HasValue<ElectricCar>("ElectricCar");

    }
}