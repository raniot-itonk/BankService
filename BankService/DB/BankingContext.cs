using System.ComponentModel.DataAnnotations;
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
    }

    public class Account
    {
        [Key]
        public string OwnerId { get; set; }
        [MaxLength(100)]
        public string OwnerName { get; set; }
        public double Balance { get; set; }
    }
}
