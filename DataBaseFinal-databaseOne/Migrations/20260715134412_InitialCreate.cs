using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectFIN.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Vin = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Engine = table.Column<int>(type: "int", nullable: false),
                    Doors = table.Column<int>(type: "int", nullable: false),
                    VehicleType = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    BatteryCapacity = table.Column<double>(type: "float", nullable: true),
                    CurrentCharge = table.Column<double>(type: "float", nullable: true),
                    FuelTankCapacity = table.Column<double>(type: "float", nullable: true),
                    CurrentFuel = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Vin);
                });

            migrationBuilder.CreateTable(
                name: "Coordinates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    HomeZoneLatitude = table.Column<double>(type: "float", nullable: true),
                    HomeZoneLongitude = table.Column<double>(type: "float", nullable: true),
                    AllowedRadius = table.Column<double>(type: "float", nullable: false),
                    VehicleVin = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coordinates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Coordinates_Vehicles_VehicleVin",
                        column: x => x.VehicleVin,
                        principalTable: "Vehicles",
                        principalColumn: "Vin",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Coordinates_VehicleVin",
                table: "Coordinates",
                column: "VehicleVin",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Coordinates");

            migrationBuilder.DropTable(
                name: "Vehicles");
        }
    }
}
