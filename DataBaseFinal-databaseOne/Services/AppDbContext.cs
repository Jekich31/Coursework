using Microsoft.EntityFrameworkCore;

namespace ProjectFIN.models;

public class AppDbContext : DbContext
{
    public DbSet<Vehicle> Vehicles { get; set; } = null!;
    public DbSet<ElectricCar> ElectricCars { get; set; } = null!;
    public DbSet<GasolineCar> GasolineCars { get; set; } = null!;
    public DbSet<Coordinate> Coordinates { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Data Source=.\SQLEXPRESS;Database=CarTelematicsDb;Integrated Security=True;TrustServerCertificate=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Vehicle>()
            .HasDiscriminator<string>("Discriminator")
            .HasValue<GasolineCar>("GasolineCar")
            .HasValue<ElectricCar>("ElectricCar");
        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.LocationData)
            .WithOne(c => c.Vehicle)
            .HasForeignKey<Coordinate>(c => c.VehicleVin)
            .OnDelete(DeleteBehavior.Cascade);
    }
}