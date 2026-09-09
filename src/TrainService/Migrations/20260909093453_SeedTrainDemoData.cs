using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TrainService.Migrations
{
    /// <inheritdoc />
    public partial class SeedTrainDemoData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Stations",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 1, "ALP", "Alpha Junction" },
                    { 2, "BRV", "Bravo Central" },
                    { 3, "CRN", "Charlie Town" },
                    { 4, "DLT", "Delta City" },
                    { 5, "ECH", "Echo Terminal" }
                });

            migrationBuilder.InsertData(
                table: "Trains",
                columns: new[] { "Id", "Name", "TrainNumber" },
                values: new object[,]
                {
                    { 1, "Northern Express", "12001" },
                    { 2, "Southern Express", "12002" }
                });

            migrationBuilder.InsertData(
                table: "Coaches",
                columns: new[] { "Id", "CoachNumber", "CoachType", "TrainId" },
                values: new object[,]
                {
                    { 1, "G1", 0, 1 },
                    { 2, "S1", 1, 1 },
                    { 3, "A3-1", 2, 1 },
                    { 4, "S1", 1, 2 },
                    { 5, "A2-1", 3, 2 },
                    { 6, "A1-1", 4, 2 }
                });

            migrationBuilder.InsertData(
                table: "Fares",
                columns: new[] { "Id", "Amount", "CoachType", "FromStationId", "ToStationId", "TrainId" },
                values: new object[,]
                {
                    { 1, 250.00m, 0, 1, 3, 1 },
                    { 2, 450.00m, 1, 1, 3, 1 },
                    { 3, 750.00m, 2, 1, 3, 1 },
                    { 4, 250.00m, 0, 2, 4, 1 },
                    { 5, 450.00m, 1, 2, 4, 1 },
                    { 6, 750.00m, 2, 2, 4, 1 },
                    { 7, 250.00m, 0, 3, 5, 1 },
                    { 8, 450.00m, 1, 3, 5, 1 },
                    { 9, 750.00m, 2, 3, 5, 1 },
                    { 10, 600.00m, 0, 1, 5, 1 },
                    { 11, 1000.00m, 1, 1, 5, 1 },
                    { 12, 1600.00m, 2, 1, 5, 1 },
                    { 13, 450.00m, 1, 5, 3, 2 },
                    { 14, 1100.00m, 3, 5, 3, 2 },
                    { 15, 1800.00m, 4, 5, 3, 2 },
                    { 16, 450.00m, 1, 4, 2, 2 },
                    { 17, 1100.00m, 3, 4, 2, 2 },
                    { 18, 1800.00m, 4, 4, 2, 2 },
                    { 19, 450.00m, 1, 3, 1, 2 },
                    { 20, 1100.00m, 3, 3, 1, 2 },
                    { 21, 1800.00m, 4, 3, 1, 2 },
                    { 22, 1000.00m, 1, 5, 1, 2 },
                    { 23, 2100.00m, 3, 5, 1, 2 },
                    { 24, 3200.00m, 4, 5, 1, 2 }
                });

            migrationBuilder.InsertData(
                table: "RouteStops",
                columns: new[] { "Id", "ArrivalTime", "DepartureTime", "StationId", "StopOrder", "TrainId" },
                values: new object[,]
                {
                    { 1, new TimeSpan(0, 6, 0, 0, 0), new TimeSpan(0, 6, 0, 0, 0), 1, 1, 1 },
                    { 2, new TimeSpan(0, 7, 30, 0, 0), new TimeSpan(0, 7, 35, 0, 0), 2, 2, 1 },
                    { 3, new TimeSpan(0, 9, 0, 0, 0), new TimeSpan(0, 9, 5, 0, 0), 3, 3, 1 },
                    { 4, new TimeSpan(0, 10, 30, 0, 0), new TimeSpan(0, 10, 35, 0, 0), 4, 4, 1 },
                    { 5, new TimeSpan(0, 12, 0, 0, 0), new TimeSpan(0, 12, 0, 0, 0), 5, 5, 1 },
                    { 6, new TimeSpan(0, 14, 0, 0, 0), new TimeSpan(0, 14, 0, 0, 0), 5, 1, 2 },
                    { 7, new TimeSpan(0, 15, 25, 0, 0), new TimeSpan(0, 15, 30, 0, 0), 4, 2, 2 },
                    { 8, new TimeSpan(0, 17, 0, 0, 0), new TimeSpan(0, 17, 5, 0, 0), 3, 3, 2 },
                    { 9, new TimeSpan(0, 18, 30, 0, 0), new TimeSpan(0, 18, 35, 0, 0), 2, 4, 2 },
                    { 10, new TimeSpan(0, 20, 0, 0, 0), new TimeSpan(0, 20, 0, 0, 0), 1, 5, 2 }
                });

            migrationBuilder.InsertData(
                table: "Seats",
                columns: new[] { "Id", "CoachId", "SeatNumber" },
                values: new object[,]
                {
                    { 1, 1, "1" },
                    { 2, 1, "2" },
                    { 3, 1, "3" },
                    { 4, 1, "4" },
                    { 5, 2, "1" },
                    { 6, 2, "2" },
                    { 7, 2, "3" },
                    { 8, 2, "4" },
                    { 9, 3, "1" },
                    { 10, 3, "2" },
                    { 11, 3, "3" },
                    { 12, 3, "4" },
                    { 13, 4, "1" },
                    { 14, 4, "2" },
                    { 15, 4, "3" },
                    { 16, 4, "4" },
                    { 17, 5, "1" },
                    { 18, 5, "2" },
                    { 19, 5, "3" },
                    { 20, 5, "4" },
                    { 21, 6, "1" },
                    { 22, 6, "2" },
                    { 23, 6, "3" },
                    { 24, 6, "4" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Fares",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Fares",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Fares",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Fares",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Fares",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Fares",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Fares",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Fares",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Fares",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Fares",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Fares",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Fares",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Fares",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Fares",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Fares",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Fares",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Fares",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Fares",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Fares",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Fares",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Fares",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Fares",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Fares",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Fares",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "RouteStops",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "RouteStops",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "RouteStops",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "RouteStops",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "RouteStops",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "RouteStops",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "RouteStops",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "RouteStops",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "RouteStops",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "RouteStops",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Seats",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Seats",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Seats",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Seats",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Seats",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Seats",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Seats",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Seats",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Seats",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Seats",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Seats",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Seats",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Seats",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Seats",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Seats",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Seats",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Seats",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Seats",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Seats",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Seats",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Seats",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Seats",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Seats",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Seats",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Coaches",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Coaches",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Coaches",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Coaches",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Coaches",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Coaches",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Stations",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Stations",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Stations",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Stations",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Stations",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Trains",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Trains",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
