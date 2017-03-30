using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JsonApiDotNetCoreExample.Migrations
{
    public partial class AddAssignedTodoItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssigneeId",
                table: "TodoItems",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_AssigneeId",
                table: "TodoItems",
                column: "AssigneeId");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoItems_People_AssigneeId",
                table: "TodoItems",
                column: "AssigneeId",
                principalTable: "People",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoItems_People_AssigneeId",
                table: "TodoItems");

            migrationBuilder.DropIndex(
                name: "IX_TodoItems_AssigneeId",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "AssigneeId",
                table: "TodoItems");
        }
    }
}
