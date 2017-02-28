using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JsonApiDotNetCoreExample.Migrations
{
    public partial class MakeOwnerOptionalOnTodoItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoItems_People_OwnerId",
                table: "TodoItems");

            migrationBuilder.AlterColumn<int>(
                name: "OwnerId",
                table: "TodoItems",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TodoItems_People_OwnerId",
                table: "TodoItems",
                column: "OwnerId",
                principalTable: "People",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoItems_People_OwnerId",
                table: "TodoItems");

            migrationBuilder.AlterColumn<int>(
                name: "OwnerId",
                table: "TodoItems",
                nullable: false);

            migrationBuilder.AddForeignKey(
                name: "FK_TodoItems_People_OwnerId",
                table: "TodoItems",
                column: "OwnerId",
                principalTable: "People",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
