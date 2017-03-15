using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JsonApiDotNetCoreExample.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TodoItemCollection",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
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

            migrationBuilder.CreateTable(
                name: "TodoItems",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    CollectionId = table.Column<Guid>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Ordinal = table.Column<long>(nullable: false),
                    OwnerId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TodoItems_TodoItemCollection_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "TodoItemCollection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TodoItems_People_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_CollectionId",
                table: "TodoItems",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_OwnerId",
                table: "TodoItems",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItemCollection_OwnerId",
                table: "TodoItemCollection",
                column: "OwnerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TodoItems");

            migrationBuilder.DropTable(
                name: "TodoItemCollection");

            migrationBuilder.DropTable(
                name: "People");
        }
    }
}
