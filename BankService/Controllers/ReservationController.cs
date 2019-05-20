using System;
using System.Threading.Tasks;
using BankService.Clients;
using BankService.DB;
using BankService.Helpers;
using BankService.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace BankService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly BankingContext _context;
        private readonly ILogger<ReservationController> _logger;
        private readonly IHostingEnvironment _env;
        private readonly IRabbitMqClient _rabbitMqClient;

        private static readonly Counter TotalMoneyReserved = Metrics.CreateCounter("TotalMoneyReserved", "Total amount of money reserved");

        public ReservationController(BankingContext context, ILogger<ReservationController> logger,
            IHostingEnvironment env, IRabbitMqClient rabbitMqClient)
        {
            _context = context;
            _logger = logger;
            _env = env;
            _rabbitMqClient = rabbitMqClient;
        }

        [HttpPost]
        public async Task<ActionResult<ReservationResult>> PostReservation(ReservationObject reservationObject)
        {
            if (!RequestHelper.ValidateId(reservationObject.AccountId, Request, _env))
                return BadRequest("HeaderId and Id are not equal");
            try
            {
                var account = await _context.Accounts.FirstOrDefaultAsync(x => x.OwnerId == reservationObject.AccountId);
                if (account == null)
                {
                    _logger.LogWarning(@"AccountId {AccountId} does not exist", reservationObject.AccountId);
                    return new ReservationResult { Valid = false, ErrorMessage = $"AccountId {reservationObject.AccountId} does not exist" };
                }
                if (account.Balance < reservationObject.Amount)
                {
                    _logger.LogInformation("The account {AccountId} only got a balance of {balance}, but the request is for {Amount}", reservationObject.AccountId, account.Balance, reservationObject.Amount);
                    return new ReservationResult { Valid = false, ErrorMessage = $"The balance is {account.Balance}, but the request is for {reservationObject.Amount}" };
                }

                account.Balance = account.Balance - reservationObject.Amount;
                var reservation = new Reservation { Amount = reservationObject.Amount, OwnerAccount = account };
                _context.Reservations.Add(reservation);

                await _context.SaveChangesAsync();
                _rabbitMqClient.SendMessage(new HistoryMessage { Event = "CreatedReservation", EventMessage = $"Reserved ${reservationObject.Amount} for buying shares with reservation id {reservationObject.AccountId}", User = reservationObject.AccountId, Timestamp = DateTime.UtcNow });
                _logger.LogInformation("Successfully reserved {Amount} from {@Account}", reservationObject.Amount, account);
                TotalMoneyReserved.Inc(reservationObject.Amount);
                return new ReservationResult{Valid = true, ReservationId = reservation.Id, ErrorMessage = string.Empty};
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
            try
            {
                var reservation = await _context.Reservations.Include(r => r.OwnerAccount).FirstAsync(r => r.Id == id);
                reservation.OwnerAccount.Balance += reservation.Amount;
                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
                _rabbitMqClient.SendMessage(new HistoryMessage { Event = "DeleteReservation", EventMessage = $"Removed reservation with id ${id}", User = reservation.OwnerAccount.OwnerId, Timestamp = DateTime.UtcNow });
                _logger.LogInformation("Successfully Removed reservation and transferred {Amount} back to {@Owner}", reservation.Amount, reservation.OwnerAccount);
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