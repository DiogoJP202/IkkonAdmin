using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IkkonAdmin.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBlogPostLanguageVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "BlogPosts",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "pt-BR");

            migrationBuilder.AddColumn<int>(
                name: "TranslationGroupId",
                table: "BlogPosts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_LanguageCode",
                table: "BlogPosts",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_TranslationGroupId_LanguageCode",
                table: "BlogPosts",
                columns: new[] { "TranslationGroupId", "LanguageCode" });

            migrationBuilder.AddForeignKey(
                name: "FK_BlogPosts_BlogPosts_TranslationGroupId",
                table: "BlogPosts",
                column: "TranslationGroupId",
                principalTable: "BlogPosts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlogPosts_BlogPosts_TranslationGroupId",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_LanguageCode",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_TranslationGroupId_LanguageCode",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "TranslationGroupId",
                table: "BlogPosts");
        }
    }
}
