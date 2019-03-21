using System;
using System.Threading.Tasks;
using BankService.DB;
using BankService.Models;
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
        private readonly ILogger<TransferController> _logger;

        public TransferController(BankingContext context, ILogger<TransferController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPut]
        public async Task<IActionResult> PutTransfer(TransferObject transferObject)
        {
            try
            {
                var fromAccount = await _context.Accounts.FirstOrDefaultAsync(x => x.OwnerId == transferObject.FromAccountId);
                if (fromAccount == null) return BadRequest($"FromAccountId {transferObject.FromAccountId} does not exist");
                var toAccount = await _context.Accounts.FirstOrDefaultAsync(x => x.OwnerId == transferObject.ToAccountId);
                if (toAccount == null) return BadRequest($"ToAccountId {transferObject.ToAccountId} does not exist");
                var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.OwnerAccount == fromAccount && r.Id == transferObject.ReservationId);
                if (reservation == null) return BadRequest($"Reservation {transferObject.ReservationId} does not exist");

                reservation.Amount -= transferObject.Amount;
                toAccount.Balance += transferObject.Amount;
                _context.Transfers.Add(new Transfer { Amount = transferObject.Amount, From = fromAccount, To = toAccount });

                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully transferred {Amount} from {@Sender} to {@Receiver}", transferObject.Amount, fromAccount, toAccount);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to transfer");
                throw;
            }

            return Ok();
        }
    }
}