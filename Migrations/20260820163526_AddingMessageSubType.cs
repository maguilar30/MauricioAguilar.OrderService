using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MauricioAguilar.OrderService.Migrations
{
    /// <inheritdoc />
    public partial class AddingMessageSubType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MessageSubType",
                schema: "ord",
                table: "OutboxMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MessageSubType",
                schema: "ord",
                table: "OutboxMessages");
        }
    }
}
