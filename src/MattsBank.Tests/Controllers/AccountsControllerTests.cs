using System.Net;
using System.Text;

using MattsBank.Api.Contracts;

using Newtonsoft.Json;

namespace MattsBank.Tests.Controllers
{
    [Collection(nameof(ApiWebApplicationFactoryCollection))]
    public class AccountsControllerTests
    {
        private readonly ApiWebApplicationFactory _fixture;
        private readonly HttpClient _client;
        private readonly string _baseUrl;

        public AccountsControllerTests(ApiWebApplicationFactory fixture)
        {
            _fixture = fixture;
            _client = _fixture.CreateClient();
            _baseUrl = $"{_client.BaseAddress}api/v1/accounts/";
        }

        [Fact]
        public async Task CreateAccount_ShouldReturn201()
        {
            // Arrange
            var request = GenerateCreateAccountRequest();

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            var content = await response.Content.ReadAsStringAsync();
            var account = JsonConvert.DeserializeObject<AccountResponse>(content)?.Account;

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(account);
        }

        [Fact]
        public async Task GetAccount_WhenAccountExists_ShouldReturn200()
        {
            // Arrange
            var account = await GivenAccountExists();

            var requestUri = new UriBuilder(_baseUrl);
            requestUri.Path += account?.AccountNumber.ToString();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = requestUri.Uri
            };

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("12345678", 404)]
        [InlineData("1234", 400)]
        [InlineData("NotValid", 400)]
        public async Task GetAccount_WhenRequestIsNotValid_ShouldReturnNotSuccess(string accountNumber, int expectedStatusCode)
        {
            // Arrange
            var requestUri = new UriBuilder(_baseUrl);
            requestUri.Path += accountNumber;

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = requestUri.Uri
            };

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal((HttpStatusCode)expectedStatusCode, response.StatusCode);
        }

        private HttpRequestMessage GenerateCreateAccountRequest()
        {
            var accountRequest = new CreateAccountRequest
            {
                FirstName = "Matt",
                LastName = "Jones"
            };

            return new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_baseUrl),
                Content = new StringContent(JsonConvert.SerializeObject(accountRequest), Encoding.UTF8, "application/json")
            };
        }

        private async Task<Account?> GivenAccountExists()
        {
            var request = GenerateCreateAccountRequest();
            var response = await _client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            var account = JsonConvert.DeserializeObject<AccountResponse>(content)?.Account;

            return account;
        }
    }
}
