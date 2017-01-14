using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JsonApiDotNetCoreExample.Migrations
{
    public partial class AddAttributes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "TodoItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "People",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "People",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "People");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "People");
        }
    }
}
