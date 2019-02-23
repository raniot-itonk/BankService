using System.Threading.Tasks;
using BankService.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly BankingContext _context;
        private ILogger<AccountController> _logger;

        public ReservationController(BankingContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPut]
        public async Task<IActionResult> PutReservation(ReservationObject reservationObject)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(x => x.OwnerId == reservationObject.AccountId);
            if (account == null) return BadRequest("AccountId does not exist");

            account.Balance = account.Balance - reservationObject.Amount;
            var reservation = new Reservation {Amount = reservationObject.Amount, OwnerAccount = account};
            _context.Reservations.Add(reservation);

            await _context.SaveChangesAsync();

            return Ok(reservation.Id);
        }
    }

    public class ReservationObject
    {
        public string AccountId { get; set; }
        public double Amount { get; set; }
    }
}