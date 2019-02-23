using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BankService.Migrations
{
    public partial class AccountTransferAndReservation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    OwnerId = table.Column<string>(nullable: false),
                    OwnerName = table.Column<string>(maxLength: 100, nullable: true),
                    Balance = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.OwnerId);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    OwnerAccountOwnerId = table.Column<string>(nullable: true),
                    Amount = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_Accounts_OwnerAccountOwnerId",
                        column: x => x.OwnerAccountOwnerId,
                        principalTable: "Accounts",
                        principalColumn: "OwnerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transfers",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    FromOwnerId = table.Column<string>(nullable: true),
                    ToOwnerId = table.Column<string>(nullable: true),
                    Amount = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transfers_Accounts_FromOwnerId",
                        column: x => x.FromOwnerId,
                        principalTable: "Accounts",
                        principalColumn: "OwnerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transfers_Accounts_ToOwnerId",
                        column: x => x.ToOwnerId,
                        principalTable: "Accounts",
                        principalColumn: "OwnerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_OwnerAccountOwnerId",
                table: "Reservations",
                column: "OwnerAccountOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_FromOwnerId",
                table: "Transfers",
                column: "FromOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_ToOwnerId",
                table: "Transfers",
                column: "ToOwnerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Transfers");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
