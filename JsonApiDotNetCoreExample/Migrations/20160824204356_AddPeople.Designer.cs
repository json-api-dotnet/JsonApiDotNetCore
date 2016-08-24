using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using JsonApiDotNetCoreExample.Data;

namespace JsonApiDotNetCoreExample.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20160824204356_AddPeople")]
    partial class AddPeople
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431");

            modelBuilder.Entity("JsonApiDotNetCoreExample.Models.Person", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("People");
                });

            modelBuilder.Entity("JsonApiDotNetCoreExample.Models.TodoItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<int>("PersonId");

                    b.HasKey("Id");

                    b.HasIndex("PersonId");

                    b.ToTable("TodoItems");
                });

            modelBuilder.Entity("JsonApiDotNetCoreExample.Models.TodoItem", b =>
                {
                    b.HasOne("JsonApiDotNetCoreExample.Models.Person", "Owner")
                        .WithMany("TodoItems")
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
