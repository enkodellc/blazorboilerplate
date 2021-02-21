﻿// <auto-generated />
using BlazorBoilerplate.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BlazorBoilerplate.Storage.Migrations.ApiContentDb
{
    [DbContext(typeof(ApiContentDbContext))]
    partial class ApiContentDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("BlazorBoilerplate.Infrastructure.Storage.DataModels.WikiPage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Extract")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ns")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("pageid")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("pageid")
                        .IsUnique();

                    b.ToTable("WikiPages");
                });
#pragma warning restore 612, 618
        }
    }
}
