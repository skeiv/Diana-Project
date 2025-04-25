using JobSearchApp.API.Controllers;
using JobSearchApp.API.Services;
using JobSearchApp.Core.Authorization;
using JobSearchApp.Core.Entities;
using JobSearchApp.Core.Enums;
using JobSearchApp.Core.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using JobSearchApp.API.DTOs;

namespace JobSearchApp.Tests.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IRepository<User>> _userRepositoryMock;
        private readonly Mock<IRepository<Resume>> _resumeRepositoryMock;
        private readonly Mock<IRepository<VacancyApplication>> _applicationRepositoryMock;
        private readonly Mock<IWebHostEnvironment> _environmentMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _userRepositoryMock = new Mock<IRepository<User>>();
            _resumeRepositoryMock = new Mock<IRepository<Resume>>();
            _applicationRepositoryMock = new Mock<IRepository<VacancyApplication>>();
            _environmentMock = new Mock<IWebHostEnvironment>();
            _userServiceMock = new Mock<IUserService>();
            _fileServiceMock = new Mock<IFileService>();

            _environmentMock.Setup(e => e.WebRootPath).Returns("wwwroot");

            _controller = new UsersController(
                _userRepositoryMock.Object,
                _resumeRepositoryMock.Object,
                _applicationRepositoryMock.Object,
                _environmentMock.Object,
                _userServiceMock.Object,
                _fileServiceMock.Object);
        }

        [Fact]
        public async Task CreateUser_AdminRole_ReturnsCreatedResult()
        {
            // Arrange
            var user = new User { Id = 1, Role = UserRole.User };
            SetupUserWithRole(UserRole.Admin);

            // Act
            var result = await _controller.CreateUser(user);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(UsersController.GetUser), createdResult.ActionName);
            Assert.Equal(user.Id, createdResult.RouteValues["id"]);
            _userRepositoryMock.Verify(r => r.AddAsync(user), Times.Once);
        }

        [Fact]
        public async Task UpdateUser_OwnProfile_ReturnsNoContent()
        {
            // Arrange
            var userId = 1;
            var user = new User { Id = userId };
            SetupUserWithRole(UserRole.User, userId);

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.UpdateUser(userId, user);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task UpdateUser_OtherProfile_ReturnsForbid()
        {
            // Arrange
            var userId = 1;
            var user = new User { Id = userId };
            SetupUserWithRole(UserRole.User, 2); // Different user ID

            // Act
            var result = await _controller.UpdateUser(userId, user);

            // Assert
            Assert.IsType<ForbidResult>(result);
            _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Never);
        }

        [Fact]
        public async Task UploadAvatar_ValidFile_ReturnsOkWithUrl()
        {
            // Arrange
            var userId = 1;
            var user = new User { Id = userId };
            SetupUserWithRole(UserRole.User, userId);
            var expectedUrl = "/uploads/avatars/avatar.jpg";
            var fileMock = new Mock<IFormFile>();

            _fileServiceMock.Setup(s => s.UploadResumeFileAsync(fileMock.Object, userId))
                .ReturnsAsync(expectedUrl);

            // Act
            var result = await _controller.UploadAvatar(fileMock.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            dynamic value = okResult.Value;
            Assert.Equal(expectedUrl, value.avatarUrl);
            _fileServiceMock.Verify(s => s.UploadResumeFileAsync(fileMock.Object, userId), Times.Once);
        }

        [Fact]
        public async Task GetAvatar_ExistingAvatar_ReturnsFileResult()
        {
            // Arrange
            var userId = 1;
            var avatarUrl = "/uploads/avatars/test.jpg";
            var user = new User { Id = userId, AvatarUrl = avatarUrl };
            SetupUserWithRole(UserRole.User, userId);
            var expectedBytes = new byte[] { 1, 2, 3 };
            var fileName = Path.GetFileName(avatarUrl);
            var filePath = Path.Combine(_environmentMock.Object.WebRootPath, avatarUrl.TrimStart('/'));

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            
            var dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            await File.WriteAllBytesAsync(filePath, expectedBytes);

            // Act
            var result = await _controller.GetAvatar();

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("image/jpeg", fileResult.ContentType);
            Assert.Equal(expectedBytes, fileResult.FileContents);

            // Cleanup
            if (File.Exists(filePath)) File.Delete(filePath);
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }

        [Fact]
        public async Task GetAvatar_NoAvatarUrl_ReturnsNotFound()
        {
            // Arrange
            var userId = 1;
            var user = new User { Id = userId, AvatarUrl = null };
            SetupUserWithRole(UserRole.User, userId);
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _controller.GetAvatar();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No avatar found", notFoundResult.Value);
        }

        [Fact]
        public async Task GetAvatar_FileNotFound_ReturnsNotFound()
        {
             // Arrange
            var userId = 1;
            var avatarUrl = "/uploads/avatars/nonexistent.jpg";
            var user = new User { Id = userId, AvatarUrl = avatarUrl };
            SetupUserWithRole(UserRole.User, userId);
             _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _controller.GetAvatar();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Avatar file not found", notFoundResult.Value);
        }

        [Fact]
        public async Task DeleteAvatar_ReturnsNoContent()
        {
            // Arrange
            var userId = 1;
            SetupUserWithRole(UserRole.User, userId);
            _fileServiceMock.Setup(s => s.DeleteResumeFileAsync(userId.ToString()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteAvatar();

            // Assert
            Assert.IsType<NoContentResult>(result.Result);
            _fileServiceMock.Verify(s => s.DeleteResumeFileAsync(userId.ToString()), Times.Once);
        }

        [Fact]
        public async Task GetUser_ReturnsUserDto()
        {
            // Arrange
            var userId = 1;
            var userDto = new UserDto { Id = userId, Email = "test@test.com" };
            SetupUserWithRole(UserRole.User, userId);

            _userServiceMock.Setup(s => s.GetUserByIdAsync(userId))
                .ReturnsAsync(userDto);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedDto = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal(userId, returnedDto.Id);
            _userServiceMock.Verify(s => s.GetUserByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetUser_NotFound_ReturnsNotFound()
        {
            // Arrange
            var userId = 1;
            SetupUserWithRole(UserRole.User, userId);

            _userServiceMock.Setup(s => s.GetUserByIdAsync(userId))
                .ReturnsAsync((UserDto)null);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }
        
        [Fact]
        public async Task SearchUsers_ReturnsUserDtoList()
        {
            // Arrange
            var query = "test";
            var users = new List<UserDto> { new UserDto { Id = 1 }, new UserDto { Id = 2 } };
            SetupUserWithRole(UserRole.Admin);

            _userServiceMock.Setup(s => s.SearchUsersAsync(query))
                .ReturnsAsync(users);

            // Act
            var result = await _controller.SearchUsers(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedUsers = Assert.IsAssignableFrom<IEnumerable<UserDto>>(okResult.Value);
            Assert.Equal(2, returnedUsers.Count());
            _userServiceMock.Verify(s => s.SearchUsersAsync(query), Times.Once);
        }

        [Fact]
        public async Task GetUserApplications_ReturnsApplications()
        {
            // Arrange
            var userId = 1;
            var applications = new List<VacancyApplication> { new VacancyApplication { Id = 1 }, new VacancyApplication { Id = 2 } };
            SetupUserWithRole(UserRole.User, userId);

            _applicationRepositoryMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<VacancyApplication, bool>>>()))
                .ReturnsAsync(applications);

            // Act
            var result = await _controller.GetUserApplications();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedApps = Assert.IsAssignableFrom<IEnumerable<VacancyApplication>>(okResult.Value);
            Assert.Equal(2, returnedApps.Count());
            _applicationRepositoryMock.Verify(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<VacancyApplication, bool>>>()), Times.Once);
        }

        [Theory]
        [InlineData(UserRole.Employer, "GetEmployers")]
        [InlineData(UserRole.Teacher, "GetTeachers")]
        [InlineData(UserRole.User, "GetJobSeekers")]
        public async Task GetUsersByRole_ReturnsUserDtoList(UserRole role, string methodName)
        {
            // Arrange
            var users = new List<UserDto> { new UserDto { Id = 1 }, new UserDto { Id = 2 } };
            SetupUserWithRole(UserRole.Admin); // Assuming Admin can call these

            _userServiceMock.Setup(s => s.GetUsersByRoleAsync(role))
                .ReturnsAsync(users);

            // Act
            ActionResult<IEnumerable<UserDto>> result = methodName switch
            {
                "GetEmployers" => await _controller.GetEmployers(),
                "GetTeachers" => await _controller.GetTeachers(),
                "GetJobSeekers" => await _controller.GetJobSeekers(),
                _ => throw new ArgumentException("Invalid method name", nameof(methodName))
            };

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedUsers = Assert.IsAssignableFrom<IEnumerable<UserDto>>(okResult.Value);
            Assert.Equal(2, returnedUsers.Count());
            _userServiceMock.Verify(s => s.GetUsersByRoleAsync(role), Times.Once);
        }

        private void SetupUserWithRole(UserRole role, int? userId = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, role.ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId?.ToString() ?? "1")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }
    }
} 