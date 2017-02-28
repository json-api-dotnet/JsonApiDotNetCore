using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JsonApiDotNetCoreExample.Migrations
{
    public partial class AddTodoItemCollection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CollectionId",
                table: "TodoItems",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TodoItemCollection",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    OwnerId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoItemCollection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TodoItemCollection_People_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_CollectionId",
                table: "TodoItems",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItemCollection_OwnerId",
                table: "TodoItemCollection",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoItems_TodoItemCollection_CollectionId",
                table: "TodoItems",
                column: "CollectionId",
                principalTable: "TodoItemCollection",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoItems_TodoItemCollection_CollectionId",
                table: "TodoItems");

            migrationBuilder.DropTable(
                name: "TodoItemCollection");

            migrationBuilder.DropIndex(
                name: "IX_TodoItems_CollectionId",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "CollectionId",
                table: "TodoItems");
        }
    }
}
