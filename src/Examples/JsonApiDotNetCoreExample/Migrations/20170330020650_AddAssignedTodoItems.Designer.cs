using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using JsonApiDotNetCoreExample.Data;

namespace JsonApiDotNetCoreExample.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20170330020650_AddAssignedTodoItems")]
    partial class AddAssignedTodoItems
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "1.1.1");

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

                    b.Property<int?>("AssigneeId");

                    b.Property<Guid?>("CollectionId");

                    b.Property<string>("Description");

                    b.Property<long>("Ordinal");

                    b.Property<int?>("OwnerId");

                    b.HasKey("Id");

                    b.HasIndex("AssigneeId");

                    b.HasIndex("CollectionId");

                    b.HasIndex("OwnerId");

                    b.ToTable("TodoItems");
                });

            modelBuilder.Entity("JsonApiDotNetCoreExample.Models.TodoItemCollection", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<int>("OwnerId");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("TodoItemCollections");
                });

            modelBuilder.Entity("JsonApiDotNetCoreExample.Models.TodoItem", b =>
                {
                    b.HasOne("JsonApiDotNetCoreExample.Models.Person", "Assignee")
                        .WithMany("AssignedTodoItems")
                        .HasForeignKey("AssigneeId");

                    b.HasOne("JsonApiDotNetCoreExample.Models.TodoItemCollection", "Collection")
                        .WithMany("TodoItems")
                        .HasForeignKey("CollectionId");

                    b.HasOne("JsonApiDotNetCoreExample.Models.Person", "Owner")
                        .WithMany("TodoItems")
                        .HasForeignKey("OwnerId");
                });

            modelBuilder.Entity("JsonApiDotNetCoreExample.Models.TodoItemCollection", b =>
                {
                    b.HasOne("JsonApiDotNetCoreExample.Models.Person", "Owner")
                        .WithMany("TodoItemCollections")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
