using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JsonApiDotNetCoreExample.Migrations
{
    public partial class AssociatedTodoItemsToAPerson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "TodoItems",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_OwnerId",
                table: "TodoItems",
                column: "OwnerId");

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

            migrationBuilder.DropIndex(
                name: "IX_TodoItems_OwnerId",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "TodoItems");
        }
    }
}
