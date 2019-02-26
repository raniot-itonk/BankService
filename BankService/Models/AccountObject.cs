using System;
using System.ComponentModel.DataAnnotations;

namespace BankService.Models
{
    public class AccountObject
    {
        public Guid OwnerId { get; set; }
        [MaxLength(100)] public string OwnerName { get; set; }
        public double Balance { get; set; }
    }
}