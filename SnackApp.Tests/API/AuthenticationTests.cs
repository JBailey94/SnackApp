namespace SnackApp.Tests.API 
{
    public class AuthenticationTests : IClassFixture<CustomWebApplicationFactory<SnackApp.API.Program>>
    {
        private readonly CustomWebApplicationFactory<SnackApp.API.Program> _factory;

        public AuthenticationTests(CustomWebApplicationFactory<SnackApp.API.Program> factory) 
        {
            _factory = factory;
        }

        [Fact]
        public async Task Register_Should_Create_User()
        {
            // Arrange
            var client = _factory.CreateAuthenticatedClient();
            var newUser = new
            {
                Username = "testuser",
                Password = "password",
                Role = "User"
            };

            // Act
            var response = await client.PostAsJsonAsync("/register", newUser);

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            content.Should().BeEmpty();
        }

        [Fact]
        public async Task Login_Should_Return_JwtToken()
        {
            // Arrange
            var client = _factory.CreateAuthenticatedClient();
            var newUser = new
            {
                Username = "testuser",
                Password = "password",
                Role = "User"
            };
            await client.PostAsJsonAsync("/register", newUser);

            // Act
            var response = await client.PostAsJsonAsync("/login", newUser);

            // Assert
            response.EnsureSuccessStatusCode();
            var token = await response.Content.ReadAsStringAsync();
            token.Should().NotBeEmpty();
        }
    }
}

