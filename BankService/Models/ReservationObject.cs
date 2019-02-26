using System;

namespace BankService.Models
{
    public class ReservationObject
    {
        public Guid AccountId { get; set; }
        public double Amount { get; set; }
    }
}