using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BankService.DB;
using BankService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace BankService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly BankingContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(BankingContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Get Account information
        //[Authorize("BankingService.UserActions")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Account>> GetAccount(string id)
        {
            
            var account = await _context.Accounts.FindAsync(id);

            if (account == null)
            {
                return NotFound();
            }
            _logger.LogInformation("Got {Account}", account);
            return account;
        }

        // Deposit money
        //[Authorize("BankingService.UserActions")]
        [HttpPut("{id}/deposit/{amount}")]
        public async Task<IActionResult> PutAccount(string id, double amount)
        {
            try
            {
                var account = await _context.Accounts.FirstOrDefaultAsync(x => x.OwnerId == id);
                account.Balance += amount;

                _context.Entry(account).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully deposited {Amount} to {Account}", amount, account);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to deposit money");
                throw;
            }

            return NoContent();
        }

        // Add new accounts
        [HttpPost]
        public async Task<ActionResult<Account>> PostAccount(AccountObject accountObject)
        {
            try
            {
                var account = new Account
                {
                    Balance = accountObject.Balance,
                    OwnerId = accountObject.OwnerId,
                    OwnerName = accountObject.OwnerName
                };
                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully created {Account}", account);
                return CreatedAtAction("GetAccount", new { id = account.OwnerId }, account);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create account");
                throw;
            }
        }
    }
}
