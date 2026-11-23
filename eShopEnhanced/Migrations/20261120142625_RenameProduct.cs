using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eShopEnhanced.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Rewritten by hand: EF scaffolded this as DropTable("Pies") + CreateTable("Products"),
    /// which would delete every product. These are true renames, so all rows are kept.
    /// </remarks>
    public partial class RenameProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Constraints are named after the table, so drop them first and re-add them with the new names
            migrationBuilder.DropForeignKey(
                name: "FK_Pies_SubCategories_SubCategoryId",
                table: "Pies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pies",
                table: "Pies");

            migrationBuilder.RenameTable(
                name: "Pies",
                newName: "Products");

            migrationBuilder.RenameColumn(
                name: "PieId",
                table: "Products",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "IsPieOfTheWeek",
                table: "Products",
                newName: "IsPickOfTheWeek");

            migrationBuilder.RenameIndex(
                name: "IX_Pies_SubCategoryId",
                table: "Products",
                newName: "IX_Products_SubCategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_SubCategories_SubCategoryId",
                table: "Products",
                column: "SubCategoryId",
                principalTable: "SubCategories",
                principalColumn: "SubCategoryId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Exactly the reverse of Up
            migrationBuilder.DropForeignKey(
                name: "FK_Products_SubCategories_SubCategoryId",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            migrationBuilder.RenameIndex(
                name: "IX_Products_SubCategoryId",
                table: "Products",
                newName: "IX_Pies_SubCategoryId");

            migrationBuilder.RenameColumn(
                name: "IsPickOfTheWeek",
                table: "Products",
                newName: "IsPieOfTheWeek");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "Products",
                newName: "PieId");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Pies");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pies",
                table: "Pies",
                column: "PieId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pies_SubCategories_SubCategoryId",
                table: "Pies",
                column: "SubCategoryId",
                principalTable: "SubCategories",
                principalColumn: "SubCategoryId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
