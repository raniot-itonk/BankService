using System;

namespace BankService.Models
{
    public class ReservationResult
    {
        public bool Valid { get; set; }
        public Guid ReservationId { get; set; }
        public string ErrorMessage { get; set; }
    }
}