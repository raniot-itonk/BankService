using System;
using System.ComponentModel.DataAnnotations;

namespace BankService.Models
{
    public class TransferObject
    {
        [Required]
        public string FromAccountId { get; set; }
        [Required]
        public Guid ReservationId { get; set; }
        [Required]
        public string ToAccountId { get; set; }
        [Required]
        [RegularExpression(@"^\d+\.?\d{0,2}$", ErrorMessage = "Please only use two decimals")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Please enter a value greater than {1}")]
        public double Amount { get; set; }
    }
}