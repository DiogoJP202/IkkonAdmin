using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IkkonAdmin.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBlogModuleBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlogCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(140)", maxLength: 140, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BlogTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogTags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BlogPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContentHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AuthorUserId = table.Column<int>(type: "int", nullable: true),
                    AuthorDisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    IsWeeklyHighlight = table.Column<bool>(type: "bit", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScheduledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArchivedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SeoTitle = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true),
                    SeoDescription = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    ReadingTimeMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogPosts_BlogCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "BlogCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BlogPosts_UsuariosSistema_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalTable: "UsuariosSistema",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BlogPostTags",
                columns: table => new
                {
                    BlogPostId = table.Column<int>(type: "int", nullable: false),
                    BlogTagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogPostTags", x => new { x.BlogPostId, x.BlogTagId });
                    table.ForeignKey(
                        name: "FK_BlogPostTags_BlogPosts_BlogPostId",
                        column: x => x.BlogPostId,
                        principalTable: "BlogPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlogPostTags_BlogTags_BlogTagId",
                        column: x => x.BlogTagId,
                        principalTable: "BlogTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlogCategories_IsActive",
                table: "BlogCategories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_BlogCategories_Name",
                table: "BlogCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlogCategories_Slug",
                table: "BlogCategories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_AuthorUserId",
                table: "BlogPosts",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_CategoryId",
                table: "BlogPosts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_IsFeatured_PublishedAtUtc",
                table: "BlogPosts",
                columns: new[] { "IsFeatured", "PublishedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_IsWeeklyHighlight_PublishedAtUtc",
                table: "BlogPosts",
                columns: new[] { "IsWeeklyHighlight", "PublishedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_Slug",
                table: "BlogPosts",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_Status",
                table: "BlogPosts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_Status_PublishedAtUtc_DeletedAtUtc",
                table: "BlogPosts",
                columns: new[] { "Status", "PublishedAtUtc", "DeletedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_BlogPostTags_BlogTagId",
                table: "BlogPostTags",
                column: "BlogTagId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogTags_IsActive",
                table: "BlogTags",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_BlogTags_Name",
                table: "BlogTags",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlogTags_Slug",
                table: "BlogTags",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlogPostTags");

            migrationBuilder.DropTable(
                name: "BlogPosts");

            migrationBuilder.DropTable(
                name: "BlogTags");

            migrationBuilder.DropTable(
                name: "BlogCategories");
        }
    }
}
