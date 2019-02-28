using System;
using System.Threading.Tasks;
using BankService.DB;
using BankService.Helpers;
using BankService.Models;
using Microsoft.AspNetCore.Hosting;
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
        private readonly ILogger<AccountController> _logger;
        private readonly IHostingEnvironment _env;

        public ReservationController(BankingContext context, ILogger<AccountController> logger,
            IHostingEnvironment env)
        {
            _context = context;
            _logger = logger;
            _env = env;
        }

        [HttpPost]
        public async Task<ActionResult<Reservation>> PostReservation(ReservationObject reservationObject)
        {
            if (!RequestHelper.ValidateId(reservationObject.AccountId, Request, _env))
                return BadRequest("HeaderId and Id are not equal");
            try
            {
                var account = await _context.Accounts.FirstOrDefaultAsync(x => x.OwnerId == reservationObject.AccountId);
                if (account == null) return BadRequest("AccountId does not exist");

                account.Balance = account.Balance - reservationObject.Amount;
                var reservation = new Reservation { Amount = reservationObject.Amount, OwnerAccount = account };
                _context.Reservations.Add(reservation);

                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully reserved {Amount} from {Account}", reservationObject.Amount, account);
                return Ok(reservation.Id);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to Reserve money");
                throw;
            }

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(Guid id)
        {
            if (!RequestHelper.ValidateId(id, Request, _env))
                return BadRequest("HeaderId and Id are not equal");
            try
            {
                var reservation = await _context.Reservations.Include(r => r.OwnerAccount).FirstAsync(r => r.Id == id);
                reservation.OwnerAccount.Balance += reservation.Amount;
                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully Removed reservation and transferred {Amount} back to {Owner}", reservation.Amount, reservation.OwnerAccount);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to Remove Reservation");
                throw;
            }

            return Ok();
        }

    }
}