using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restorant.Data.Migrations
{
    /// <inheritdoc />
    public partial class CustomerReviewsAndOrderProductUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomerReviewCount",
                table: "OrderProducts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsReviewed",
                table: "OrderProducts",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerReviewCount",
                table: "OrderProducts");

            migrationBuilder.DropColumn(
                name: "IsReviewed",
                table: "OrderProducts");
        }
    }
}
