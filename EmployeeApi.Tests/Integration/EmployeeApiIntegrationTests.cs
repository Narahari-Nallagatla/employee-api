using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EmployeeApi.Tests.Integration
{
    public class EmployeeApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public EmployeeApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetEmployees_ReturnsUnauthorized()
        {
            // Act

            var response = await _client.GetAsync("/api/v1/employees");

            // Assert

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
