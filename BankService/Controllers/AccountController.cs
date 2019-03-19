using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BankService.DB;
using BankService.Helpers;
using BankService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace BankService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class AccountController : ControllerBase
    {
        private readonly BankingContext _context;
        private readonly ILogger<AccountController> _logger;
        private readonly IHostingEnvironment _env;

        public AccountController(BankingContext context, ILogger<AccountController> logger,
            IHostingEnvironment env)
        {
            _context = context;
            _logger = logger;
            _env = env;
        }

        // Get Account information
        [Authorize("BankingService.UserActions")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Account>> GetAccount(Guid id)
        {
            if (!RequestHelper.ValidateId(id, Request, _env))
                return BadRequest("HeaderId and Id are not equal");

            var account = await _context.Accounts.FindAsync(id);

            if (account == null)
            {
                return NotFound();
            }
            _logger.LogInformation("Got {@Account}", account);
            return account;
        }



        // Deposit money
        [Authorize("BankingService.UserActions")]
        [HttpPut("{id}/balance")]
        public async Task<IActionResult> PutAccount(Guid id, [FromBody]DepositRequest depositRequest)
        {
            try
            {
                if (!RequestHelper.ValidateId(id, Request, _env))
                    return BadRequest("HeaderId and Id are not equal");
                var account = await _context.Accounts.FirstOrDefaultAsync(x => x.OwnerId == id);
                account.Balance += depositRequest.Amount;

                _context.Entry(account).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully deposited {Amount} to {Account}", depositRequest.Amount, account);
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
            if (!RequestHelper.ValidateId(accountObject.OwnerId, Request, _env))
                return BadRequest("HeaderId and Id are not equal");
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
