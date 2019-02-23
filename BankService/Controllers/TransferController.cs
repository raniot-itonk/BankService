using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BankService.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransferController : ControllerBase
    {
        private readonly BankingContext _context;
        private ILogger<AccountController> _logger;

        public TransferController(BankingContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPut]
        public async Task<IActionResult> PutTransfer(TransferObject transferObject)
        {
            var fromAccount = await _context.Accounts.FirstOrDefaultAsync(x => x.OwnerId == transferObject.FromAccountId);
            if (fromAccount == null) return BadRequest("FromAccountId does not exist");
            var toAccount = await _context.Accounts.FirstOrDefaultAsync(x => x.OwnerId == transferObject.ToAccountId);
            if (toAccount == null) return BadRequest("ToAccountId does not exist");
            var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.OwnerAccount == fromAccount && r.Id == transferObject.ReservationId);
            if (reservation == null) return BadRequest("Reservation does not exist");

            reservation.Amount = reservation.Amount - transferObject.Amount;
            toAccount.Balance = toAccount.Balance + transferObject.Amount;
            _context.Transfers.Add(new Transfer {Amount = transferObject.Amount, From = fromAccount, To = toAccount});
            if (transferObject.ReleaseReservation)
            {
                fromAccount.Balance += reservation.Amount;
                _context.Reservations.Remove(reservation);
            }
                
            await _context.SaveChangesAsync();

            return Ok();
        }
    }

    public class TransferObject
    {
        [Required]
        public string FromAccountId { get; set; }
        [Required]
        public Guid ReservationId { get; set; }
        [Required]
        public bool ReleaseReservation { get; set; }
        [Required]
        public string ToAccountId { get; set; }
        [Required]
        [RegularExpression(@"^\d+\.?\d{0,2}$", ErrorMessage = "Please only use two decimals")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Please enter a value greater than {1}")]
        public double Amount { get; set; }
    }
}