﻿// <auto-generated />
using System;
using BankService.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BankService.Migrations
{
    [DbContext(typeof(BankingContext))]
    partial class BankingContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.0-rtm-35687")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("BankService.DB.Account", b =>
                {
                    b.Property<Guid>("OwnerId")
                        .ValueGeneratedOnAdd();

                    b.Property<double>("Balance");

                    b.Property<string>("OwnerName")
                        .HasMaxLength(100);

                    b.HasKey("OwnerId");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("BankService.DB.Reservation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<double>("Amount");

                    b.Property<Guid?>("OwnerAccountOwnerId");

                    b.HasKey("Id");

                    b.HasIndex("OwnerAccountOwnerId");

                    b.ToTable("Reservations");
                });

            modelBuilder.Entity("BankService.DB.Transfer", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<double>("Amount");

                    b.Property<Guid?>("FromOwnerId");

                    b.Property<Guid?>("ToOwnerId");

                    b.HasKey("Id");

                    b.HasIndex("FromOwnerId");

                    b.HasIndex("ToOwnerId");

                    b.ToTable("Transfers");
                });

            modelBuilder.Entity("BankService.DB.Reservation", b =>
                {
                    b.HasOne("BankService.DB.Account", "OwnerAccount")
                        .WithMany()
                        .HasForeignKey("OwnerAccountOwnerId");
                });

            modelBuilder.Entity("BankService.DB.Transfer", b =>
                {
                    b.HasOne("BankService.DB.Account", "From")
                        .WithMany()
                        .HasForeignKey("FromOwnerId");

                    b.HasOne("BankService.DB.Account", "To")
                        .WithMany()
                        .HasForeignKey("ToOwnerId");
                });
#pragma warning restore 612, 618
        }
    }
}
