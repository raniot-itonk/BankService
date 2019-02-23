using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BankService.DB
{
    public class BankingContext : DbContext
    {
        public BankingContext(DbContextOptions<BankingContext> options)
            : base(options)
        {
        }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transfer> Transfers { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
    }

    public class Transfer
    {
        public long Id { get; set; }
        public Account From { get; set; }
        public Account To { get; set; }
        public double Amount { get; set; }
    }

    public class Account
    {
        [Key]
        public string OwnerId { get; set; }
        [MaxLength(100)]
        public string OwnerName { get; set; }
        public double Balance { get; set; }
    }

    public class Reservation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public Account OwnerAccount { get; set; }
        public double Amount { get; set; }
    }
}
