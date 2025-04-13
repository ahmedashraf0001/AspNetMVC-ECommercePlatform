using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Commerce_Platform.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedUserAddedIsExternalLoginBool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsExternalLogin",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsExternalLogin",
                table: "AspNetUsers");
        }
    }
}
