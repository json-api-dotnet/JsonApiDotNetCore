using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JsonApiDotNetCoreExample.Migrations
{
    public partial class AddPeople : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGeneratedOnAdd", true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                });

            migrationBuilder.AddColumn<int>(
                name: "PersonId",
                table: "TodoItems",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_PersonId",
                table: "TodoItems",
                column: "PersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoItems_People_PersonId",
                table: "TodoItems",
                column: "PersonId",
                principalTable: "People",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoItems_People_PersonId",
                table: "TodoItems");

            migrationBuilder.DropIndex(
                name: "IX_TodoItems_PersonId",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "PersonId",
                table: "TodoItems");

            migrationBuilder.DropTable(
                name: "People");
        }
    }
}
