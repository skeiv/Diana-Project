using JobSearchApp.API.Controllers;
using JobSearchApp.API.DTOs;
using JobSearchApp.API.Services;
using JobSearchApp.Core.Entities;
using JobSearchApp.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace JobSearchApp.Tests.Controllers
{
    public class ResumesControllerTests
    {
        private readonly Mock<ApplicationDbContext> _contextMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly ResumesController _controller;
        private readonly User _currentUser;

        public ResumesControllerTests()
        {
            // Mock DbContext
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique name for each test run
                .Options;
            // We don't mock ApplicationDbContext directly, but use a real in-memory version for simplicity
            // Alternatively, mock DbSet<Resume> and setup methods like Add, Remove, FirstOrDefaultAsync, etc.
            // For this example, we'll mock services and assume DbContext works as expected via services if possible,
            // but ResumesController uses DbContext directly, so we need to mock it or its DbSets.

            var dbContextMock = new Mock<ApplicationDbContext>(options);
            var resumes = new List<Resume>().AsQueryable();
            var mockSet = new Mock<DbSet<Resume>>();
            mockSet.As<IQueryable<Resume>>().Setup(m => m.Provider).Returns(resumes.Provider);
            mockSet.As<IQueryable<Resume>>().Setup(m => m.Expression).Returns(resumes.Expression);
            mockSet.As<IQueryable<Resume>>().Setup(m => m.ElementType).Returns(resumes.ElementType);
            mockSet.As<IQueryable<Resume>>().Setup(m => m.GetEnumerator()).Returns(resumes.GetEnumerator());
            // Setup other DbSet methods as needed (Add, Remove, FindAsync, FirstOrDefaultAsync, etc.)
            
            dbContextMock.Setup(c => c.Resumes).Returns(mockSet.Object);
            dbContextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1); // Mock SaveChangesAsync

            _contextMock = dbContextMock; // Assign the mocked DbContext

            _userServiceMock = new Mock<IUserService>();
            _fileServiceMock = new Mock<IFileService>();

            // Setup current user
            _currentUser = new User { Id = 1, Email = "test@test.com" };
            _userServiceMock.Setup(s => s.GetCurrentUser()).ReturnsAsync(_currentUser);

            _controller = new ResumesController(
                _contextMock.Object,
                _userServiceMock.Object,
                _fileServiceMock.Object);

            // Setup ControllerContext for authorization
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, _currentUser.Id.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        // Helper to mock DbSet<T> with data
        private Mock<DbSet<T>> GetMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();
            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            dbSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>(sourceList.Add);
             // Mock FirstOrDefaultAsync
            // Note: EF Core extension methods like FirstOrDefaultAsync are hard to mock directly.
            // It's often easier to test the IQueryable part or use an in-memory provider.
            // This setup might not cover all scenarios perfectly.
            return dbSet;
        }


        [Fact]
        public async Task GetResumes_ReturnsUserResumes()
        {
            // Arrange
            var resumesData = new List<Resume>
            {
                new Resume { Id = 1, UserId = _currentUser.Id, Title = "Resume 1" },
                new Resume { Id = 2, UserId = _currentUser.Id, Title = "Resume 2" },
                new Resume { Id = 3, UserId = 99, Title = "Other User Resume" }
            };
            var mockSet = GetMockDbSet(resumesData);
             _contextMock.Setup(c => c.Resumes).Returns(mockSet.Object);

            // Act
            var result = await _controller.GetResumes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedDtos = Assert.IsAssignableFrom<IEnumerable<ResumeDto>>(okResult.Value);
            Assert.Equal(2, returnedDtos.Count());
            Assert.True(returnedDtos.All(r => r.UserId == _currentUser.Id));
        }
        
        [Fact]
        public async Task CreateResume_ValidDto_ReturnsCreatedResume()
        {
            // Arrange
            var createDto = new CreateResumeDto { Title = "New Resume" };
            var createdResume = new Resume(); // Capture the added resume

            var mockSet = GetMockDbSet(new List<Resume>()); // Start with empty list
            mockSet.Setup(d => d.Add(It.IsAny<Resume>()))
                   .Callback<Resume>(r => {
                       r.Id = 5; // Simulate DB assigning an ID
                       r.CreatedAt = DateTime.UtcNow; // Simulate DB setting date
                       createdResume = r; // Capture
                   }); 
            _contextMock.Setup(c => c.Resumes).Returns(mockSet.Object);
            
            // Act
            var result = await _controller.CreateResume(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedDto = Assert.IsType<ResumeDto>(createdResult.Value);
            Assert.Equal(createDto.Title, returnedDto.Title);
            Assert.Equal(_currentUser.Id, returnedDto.UserId);
            Assert.Equal(5, returnedDto.Id); // Check simulated ID
            _contextMock.Verify(c => c.SaveChangesAsync(default), Times.Once); // Verify SaveChanges called
        }

        // TODO: Add tests for GetResume(id), UpdateResume, DeleteResume, UploadResumeFile, DownloadResumeFile
        // These tests will require more setup for FirstOrDefaultAsync, FindAsync, FileService methods etc.

    }
} 