using LMS.API.Controllers;
using LMS.Application.DTOs;
using LMS.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace LMS.Tests
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly UsersController _controller;
        private readonly Mock<ILogger<UsersController>> _loggerMock;

        public UsersControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _loggerMock = new Mock<ILogger<UsersController>>();
            _controller = new UsersController(_userServiceMock.Object, _loggerMock.Object);
        }

        private void SetUserClaims(string userId, string role)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            }, "mock"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetUsers_ReturnsOkResult_WithListOfUsers()
        {
            // Arrange
            var users = new List<UserDTO> { new UserDTO { UserId = 1, Username = "user1" } };
            _userServiceMock.Setup(service => service.GetAllUsersAsync()).ReturnsAsync(users);

            // Act
            var result = await _controller.GetUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result); // Use `result.Result` to access the underlying ActionResult
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(users, okResult.Value);
        }

        [Fact]
        public async Task GetUsers_Returns500_WhenExceptionThrown()
        {
            // Arrange
            _userServiceMock.Setup(service => service.GetAllUsersAsync()).ThrowsAsync(new Exception());

            // Act
            var result = await _controller.GetUsers();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetUser_ReturnsOkResult_WhenUserExists()
        {
            // Arrange
            var userId = 1;
            var user = new UserDTO { UserId = userId, Username = "user1" };
            _userServiceMock.Setup(service => service.GetUserByIdAsync(userId)).ReturnsAsync(user);
            SetUserClaims(userId.ToString(), "Admin");

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result); // Use `result.Result` to access the underlying ActionResult
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(user, okResult.Value);
        }

        [Fact]
        public async Task GetUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = 1;
            _userServiceMock.Setup(service => service.GetUserByIdAsync(userId)).ReturnsAsync((UserDTO)null);
            SetUserClaims(userId.ToString(), "Role1");

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetUser_ReturnsForbid_WhenUserTriesToAccessOtherUserDetails()
        {
            // Arrange
            var userId = 2;
            SetUserClaims("1", "Admin");

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Login_ReturnsOkResult_WithToken_WhenCredentialsAreValid()
        {
            // Arrange
            var loginDto = new LoginDTO { Username = "instructor", Password = "AQAAAAIAAYagAAAAEEEQeDbdNV5MgtGE9xRiBmHn6LHimppoTNMc5e+kfJyWOZulKNJ7QFrSr7KxgFsMoA==" };
            var user = new UserDTO { UserId = 2, Username = "instructor" };
            var token = "fake-jwt-token";
            _userServiceMock.Setup(service => service.AuthenticateUserAsync(loginDto.Username, loginDto.Password)).ReturnsAsync(user);
            _userServiceMock.Setup(service => service.GenerateJwtToken(user)).Returns(token);

            // Act
            var result = await _controller.Login(loginDto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(new { Token = token }, result.Value);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenCredentialsAreInvalid()
        {
            // Arrange
            var loginDto = new LoginDTO { Username = "user1", Password = "wrongpassword" };
            _userServiceMock.Setup(service => service.AuthenticateUserAsync(loginDto.Username, loginDto.Password)).ReturnsAsync((UserDTO)null);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Register_ReturnsOkResult_WithUserId()
        {
            // Arrange
            var createUserDto = new CreateUserDTO { Username = "newuser", 
                                                    Password = "AQAAAAIAAYagAAAAEEEQeDbdNV5MgtGE9xRiBmHn6LHimppoTNMc5e+kfJyWOZulKNJ7QFrSr7KxgFsMoA==", 
                                                    Email = "user1@example.com", 
                                                    FullName = "Test User", 
                                                    DateOfBirth = new DateTime(1980, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)};

            var userId = 1;
            var user = new UserDTO { UserId = userId, Username = "user1", Email = "user1@example.com" };
            _userServiceMock.Setup(service => service.CreateUserAsync(createUserDto)).ReturnsAsync(user);

            // Act
            var result = await _controller.Register(createUserDto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(new { UserId = userId }, result.Value);
        }

        [Fact]
        public async Task Register_Returns500_WhenExceptionThrown()
        {
            // Arrange
            var createUserDto = new CreateUserDTO { Username = "newuser", Password = "password" };
            _userServiceMock.Setup(service => service.CreateUserAsync(createUserDto)).ThrowsAsync(new Exception());

            // Act
            var result = await _controller.Register(createUserDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task UpdateUser_ReturnsNoContent_WhenUpdateIsSuccessful()
        {
            // Arrange
            var updateUserDto = new UpdateUserDTO { Username = "updateduser" };
            var userId = 1;

            // Act
            var result = await _controller.UpdateUser(userId, updateUserDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateUser_Returns500_WhenExceptionThrown()
        {
            // Arrange
            var updateUserDto = new UpdateUserDTO { Username = "updateduser" };
            var userId = 1;
            _userServiceMock.Setup(service => service.UpdateUserAsync(userId, updateUserDto)).ThrowsAsync(new Exception());

            // Act
            var result = await _controller.UpdateUser(userId, updateUserDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task DeleteUser_ReturnsNoContent_WhenDeletionIsSuccessful()
        {
            // Arrange
            var userId = 1;

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteUser_Returns500_WhenExceptionThrown()
        {
            // Arrange
            var userId = 1;
            _userServiceMock.Setup(service => service.DeleteUserAsync(userId)).ThrowsAsync(new Exception());

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task AssignRoles_ReturnsOkResult_WhenRolesAssignedSuccessfully()
        {
            // Arrange
            var userId = 1;
            var assignRolesDto = new AssignRolesDTO { RoleIds = new List<int> { 1, 2 } };

            // Act
            var result = await _controller.AssignRoles(userId, assignRolesDto);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task AssignRoles_ReturnsBadRequest_WhenArgumentExceptionThrown()
        {
            // Arrange
            var userId = 1;
            var assignRolesDto = new AssignRolesDTO { RoleIds = new List<int> { 1, 2 } };
            _userServiceMock.Setup(service => service.AssignRolesAsync(userId, assignRolesDto.RoleIds)).ThrowsAsync(new ArgumentException("Invalid roles"));

            // Act
            var result = await _controller.AssignRoles(userId, assignRolesDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task AssignRoles_Returns500_WhenExceptionThrown()
        {
            // Arrange
            var userId = 1;
            var assignRolesDto = new AssignRolesDTO { RoleIds = new List<int> { 1, 2 } };
            _userServiceMock.Setup(service => service.AssignRolesAsync(userId, assignRolesDto.RoleIds)).ThrowsAsync(new Exception());

            // Act
            var result = await _controller.AssignRoles(userId, assignRolesDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }
    }
}
