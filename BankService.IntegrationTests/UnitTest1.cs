using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace BankService.IntegrationTests
{
    public class UnitTest1 : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;
        public UnitTest1(CustomWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Test1()
        {
            // The endpoint or route of the controller action.
            var httpResponse = await _client.GetAsync("/api/Account");

            // Must be successful.
            httpResponse.EnsureSuccessStatusCode();
        }
    }
}
