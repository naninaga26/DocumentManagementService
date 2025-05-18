using Microsoft.AspNetCore.Mvc;
using Moq;
using Document.Services.AuthAPI.Controllers;
using Document.Services.AuthAPI.Models.DTOs;
using Document.Services.AuthAPI.Services.IServices;
using Document.Services.AuthAPI.Models;

namespace Document.Services.AuthAPI.Tests.Controllers;

// AuthController
public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _controller = new AuthController(_mockAuthService.Object);
    }

    // POST api/auth/login
    // should return OK result with token for valid credentials
    [Fact]
    public async Task ShouldReturnOkResultForValidCredentials()
    {
        // Arrange
        var request = new LoginDto ("testuser", "password" );
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            UserRole = Role.Admin
        };
        var token = new TokenDto("jwt-token", DateTime.UtcNow.AddHours(1), "testuser", Role.Admin.ToString());

        _mockAuthService.Setup(service => service.LoginAsync(request)).ReturnsAsync((user,token, "Login successful."));
        // Act
        var result = await _controller.Login(request);
        // Assert
       var okResult = Assert.IsType<OkObjectResult>(result);
       var returnResponse = Assert.IsType<TokenDto>(okResult.Value);

        Assert.Equal("jwt-token", returnResponse?.AccessToken);
    }
}