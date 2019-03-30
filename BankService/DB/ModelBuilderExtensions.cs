using System;
using Microsoft.EntityFrameworkCore;

namespace BankService.DB
{
    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>().HasData(
                new Account
                {
                    OwnerId = Guid.Parse("7bedb953-4e7e-45f9-91de-ffc0175be744"),
                    Balance = 0,
                    OwnerName = "State Tax Account"
                }
            );
        }
    }
}
