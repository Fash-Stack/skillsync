using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillSync.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    internal partial class AddRefreshTokenFamilyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FamilyId",
                schema: "identity",
                table: "RefreshTokens",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_FamilyId",
                schema: "identity",
                table: "RefreshTokens",
                column: "FamilyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_FamilyId",
                schema: "identity",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "FamilyId",
                schema: "identity",
                table: "RefreshTokens");
        }
    }
}
