using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using JsonApiDotNetCoreExample.Data;

namespace JsonApiDotNetCoreExample.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20170228175630_MakeOwnerOptionalOnTodoItems")]
    partial class MakeOwnerOptionalOnTodoItems
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752");

            modelBuilder.Entity("JsonApiDotNetCoreExample.Models.Person", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.HasKey("Id");

                    b.ToTable("People");
                });

            modelBuilder.Entity("JsonApiDotNetCoreExample.Models.TodoItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<long>("Ordinal");

                    b.Property<int?>("OwnerId");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("TodoItems");
                });

            modelBuilder.Entity("JsonApiDotNetCoreExample.Models.TodoItem", b =>
                {
                    b.HasOne("JsonApiDotNetCoreExample.Models.Person", "Owner")
                        .WithMany("TodoItems")
                        .HasForeignKey("OwnerId");
                });
        }
    }
}
