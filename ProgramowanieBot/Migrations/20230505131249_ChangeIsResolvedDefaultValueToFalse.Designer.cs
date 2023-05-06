﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using ProgramowanieBot.Data;

#nullable disable

namespace ProgramowanieBot.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20230505131249_ChangeIsResolvedDefaultValueToFalse")]
    partial class ChangeIsResolvedDefaultValueToFalse
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ProgramowanieBot.Data.GuildProfile", b =>
                {
                    b.Property<decimal>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<long>("Reputation")
                        .HasColumnType("bigint");

                    b.Property<long>("ReputationToday")
                        .HasColumnType("bigint");

                    b.HasKey("UserId");

                    b.ToTable("Profiles");
                });

            modelBuilder.Entity("ProgramowanieBot.Data.Post", b =>
                {
                    b.Property<decimal>("PostId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("IsResolved")
                        .HasColumnType("boolean");

                    b.Property<int>("PostResolveReminderCounter")
                        .HasColumnType("integer");

                    b.HasKey("PostId");

                    b.ToTable("Posts");
                });
#pragma warning restore 612, 618
        }
    }
}