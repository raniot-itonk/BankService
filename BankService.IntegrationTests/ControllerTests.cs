using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using BankService.DB;
using BankService.Models;
using Newtonsoft.Json;
using Xunit;

namespace BankService.IntegrationTests
{
    public class ControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;
        public ControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateAccount()
        {
            await CreateAccountHelper();
        }

        [Fact]
        public async Task GetSingleAccount()
        {
            var id = await CreateAccountHelper();
            await GetSingleAccountHelper(id);
        }

        [Fact]
        public async Task MakeReservation()
        {
            var receiver = await CreateAccountHelper();

            await MakeReservationHelper(receiver);
        }

        [Fact]
        public async Task MakeTransfer()
        {
            var senderId = await CreateAccountHelper();
            var receiverId = await CreateAccountHelper();

            var reservationId = await MakeReservationHelper(senderId);
            await MakeTransferHelper(senderId, receiverId, reservationId);

            var receiverAfterTransfer = await GetSingleAccountHelper(receiverId);
            var senderAfterTransfer = await GetSingleAccountHelper(senderId);


            Assert.Equal(15, receiverAfterTransfer.Balance);
            Assert.Equal(5, senderAfterTransfer.Balance);
        }

        private async Task MakeTransferHelper(Guid senderId, Guid receiverId, Guid reservationId)
        {
            var transferObject = new TransferObject() { FromAccountId = senderId, ToAccountId = receiverId, Amount = 5.00, ReservationId = reservationId};
            var transferContent = new StringContent(JsonConvert.SerializeObject(transferObject), Encoding.UTF8, "application/json");
            var httpResponse = await _client.PutAsync("/api/transfer", transferContent);
            httpResponse.EnsureSuccessStatusCode();
        }

        private async Task<Account> GetSingleAccountHelper(Guid id)
        {
            var httpResponse = await _client.GetAsync($"/api/Account/{id}");
            httpResponse.EnsureSuccessStatusCode();
            return await httpResponse.Content.ReadAsAsync<Account>(new[] { new JsonMediaTypeFormatter() });
        }

        private async Task<Guid> MakeReservationHelper(Guid id)
        {
            var reservation = new ReservationObject { AccountId = id, Amount = 5 };
            var reservationContent = new StringContent(JsonConvert.SerializeObject(reservation), Encoding.UTF8, "application/json");
            var httpResponse = await _client.PutAsync("/api/Reservation", reservationContent);
            httpResponse.EnsureSuccessStatusCode();
            var reservationId = await httpResponse.Content.ReadAsAsync<Guid>(new[] {new JsonMediaTypeFormatter()});
            return reservationId;
        }

        private async Task<Guid> CreateAccountHelper()
        {
            var id = Guid.NewGuid();
            var account = new Account { OwnerId = id, Balance = 10, OwnerName = id.ToString() };
            var content = new StringContent(JsonConvert.SerializeObject(account), Encoding.UTF8, "application/json");
            var httpResponse = await _client.PostAsync("/api/Account", content);
            httpResponse.EnsureSuccessStatusCode();
            return id;
        }
    }
}
