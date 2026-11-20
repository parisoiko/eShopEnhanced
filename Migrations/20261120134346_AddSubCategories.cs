using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eShopEnhanced.Migrations
{
    /// <inheritdoc />
    public partial class AddSubCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Added by hand: the old rows only contain seed data, and the old categories
            // ("Fruit pies"...) are now subcategories. Clear them so the new foreign key
            // can be created; DbInitializer reseeds everything in the new structure.
            migrationBuilder.Sql("DELETE FROM Pies; DELETE FROM Categories;");

            migrationBuilder.DropForeignKey(
                name: "FK_Pies_Categories_CategoryId",
                table: "Pies");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Pies",
                newName: "SubCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Pies_CategoryId",
                table: "Pies",
                newName: "IX_Pies_SubCategoryId");

            migrationBuilder.CreateTable(
                name: "SubCategories",
                columns: table => new
                {
                    SubCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubCategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategories", x => x.SubCategoryId);
                    table.ForeignKey(
                        name: "FK_SubCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubCategories_CategoryId",
                table: "SubCategories",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pies_SubCategories_SubCategoryId",
                table: "Pies",
                column: "SubCategoryId",
                principalTable: "SubCategories",
                principalColumn: "SubCategoryId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pies_SubCategories_SubCategoryId",
                table: "Pies");

            migrationBuilder.DropTable(
                name: "SubCategories");

            migrationBuilder.RenameColumn(
                name: "SubCategoryId",
                table: "Pies",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Pies_SubCategoryId",
                table: "Pies",
                newName: "IX_Pies_CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pies_Categories_CategoryId",
                table: "Pies",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
