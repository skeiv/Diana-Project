using JobSearchApp.API.Controllers;
using JobSearchApp.Core.Authorization;
using JobSearchApp.Core.Entities;
using JobSearchApp.Core.Enums;
using JobSearchApp.Core.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace JobSearchApp.Tests.Controllers
{
    public class CoursesControllerTests
    {
        private readonly Mock<IRepository<Course>> _courseRepositoryMock;
        private readonly Mock<IRepository<UserCourse>> _userCourseRepositoryMock;
        private readonly Mock<IRepository<User>> _userRepositoryMock;
        private readonly CoursesController _controller;

        public CoursesControllerTests()
        {
            _courseRepositoryMock = new Mock<IRepository<Course>>();
            _userCourseRepositoryMock = new Mock<IRepository<UserCourse>>();
            _userRepositoryMock = new Mock<IRepository<User>>();

            _controller = new CoursesController(
                _courseRepositoryMock.Object,
                _userCourseRepositoryMock.Object,
                _userRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateCourse_AdminRole_ReturnsCreatedResult()
        {
            // Arrange
            var course = new Course { Id = 1, Title = "Test Course" };
            SetupUserWithRole(UserRole.Admin);

            // Act
            var result = await _controller.CreateCourse(course);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(CoursesController.GetCourse), createdResult.ActionName);
            Assert.Equal(course.Id, createdResult.RouteValues["id"]);
            _courseRepositoryMock.Verify(r => r.AddAsync(course), Times.Once);
        }

        [Fact]
        public async Task CreateCourse_TeacherRole_ReturnsCreatedResult()
        {
            // Arrange
            var teacherId = 1;
            var course = new Course { Id = 1, Title = "Test Course" };
            SetupUserWithRole(UserRole.Teacher, teacherId);

            // Act
            var result = await _controller.CreateCourse(course);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(CoursesController.GetCourse), createdResult.ActionName);
            Assert.Equal(course.Id, createdResult.RouteValues["id"]);
            _courseRepositoryMock.Verify(r => r.AddAsync(course), Times.Once);
        }

        [Fact]
        public async Task UpdateCourse_OwnCourse_ReturnsNoContent()
        {
            // Arrange
            var teacherId = 1;
            var course = new Course { Id = 1, TeacherId = teacherId };
            SetupUserWithRole(UserRole.Teacher, teacherId);

            _courseRepositoryMock.Setup(r => r.GetByIdAsync(course.Id))
                .ReturnsAsync(course);

            // Act
            var result = await _controller.UpdateCourse(course.Id, course);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _courseRepositoryMock.Verify(r => r.UpdateAsync(course), Times.Once);
        }

        [Fact]
        public async Task UpdateCourse_OtherCourse_ReturnsForbid()
        {
            // Arrange
            var course = new Course { Id = 1, TeacherId = 1 };
            SetupUserWithRole(UserRole.Teacher, 2); // Different teacher ID

            _courseRepositoryMock.Setup(r => r.GetByIdAsync(course.Id))
                .ReturnsAsync(course);

            // Act
            var result = await _controller.UpdateCourse(course.Id, course);

            // Assert
            Assert.IsType<ForbidResult>(result);
            _courseRepositoryMock.Verify(r => r.UpdateAsync(course), Times.Never);
        }

        [Fact]
        public async Task GetCourse_ExistingCourse_ReturnsCourse()
        {
            // Arrange
            var course = new Course { Id = 1 };
            _courseRepositoryMock.Setup(r => r.GetByIdAsync(course.Id))
                .ReturnsAsync(course);

            // Act
            var result = await _controller.GetCourse(course.Id);

            // Assert
            var okResult = Assert.IsType<ActionResult<Course>>(result);
            Assert.Equal(course, okResult.Value);
        }

        [Fact]
        public async Task DeleteCourse_AdminRole_ReturnsNoContent()
        {
            // Arrange
            var course = new Course { Id = 1 };
            SetupUserWithRole(UserRole.Admin);

            _courseRepositoryMock.Setup(r => r.GetByIdAsync(course.Id))
                .ReturnsAsync(course);

            // Act
            var result = await _controller.DeleteCourse(course.Id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.True(course.IsDeleted);
            _courseRepositoryMock.Verify(r => r.UpdateAsync(course), Times.Once);
        }

        [Fact]
        public async Task SearchCourses_ReturnsActiveCourses()
        {
            // Arrange
            var query = "test";
            var courses = new List<Course>
            {
                new Course { Id = 1, Title = "Test Course", IsDeleted = false },
                new Course { Id = 2, Title = "Another Test", IsDeleted = false },
                new Course { Id = 3, Title = "Deleted", IsDeleted = true }
            };

            _courseRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Func<Course, bool>>()))
                .ReturnsAsync(courses.Where(c => !c.IsDeleted && 
                    (c.Title.Contains(query) || c.Description.Contains(query))));

            // Act
            var result = await _controller.SearchCourses(query);

            // Assert
            var okResult = Assert.IsType<ActionResult<IEnumerable<Course>>>(result);
            var returnedCourses = Assert.IsAssignableFrom<IEnumerable<Course>>(okResult.Value);
            Assert.Equal(2, returnedCourses.Count());
        }

        [Fact]
        public async Task EnrollInCourse_ReturnsOk()
        {
            // Arrange
            var userId = 1;
            var courseId = 1;
            SetupUserWithRole(UserRole.User, userId);

            // Act
            var result = await _controller.EnrollInCourse(courseId);

            // Assert
            Assert.IsType<OkResult>(result);
            _userCourseRepositoryMock.Verify(r => r.AddAsync(It.Is<UserCourse>(uc =>
                uc.UserId == userId &&
                uc.CourseId == courseId &&
                uc.IsEnrolled)), Times.Once);
        }

        [Fact]
        public async Task GetEnrolledCourses_ReturnsEnrolledCourses()
        {
            // Arrange
            var userId = 1;
            SetupUserWithRole(UserRole.User, userId);

            var enrolledCourses = new List<UserCourse>
            {
                new UserCourse { UserId = userId, CourseId = 1, IsEnrolled = true },
                new UserCourse { UserId = userId, CourseId = 2, IsEnrolled = true }
            };

            _userCourseRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Func<UserCourse, bool>>()))
                .ReturnsAsync(enrolledCourses);

            // Act
            var result = await _controller.GetEnrolledCourses();

            // Assert
            var okResult = Assert.IsType<ActionResult<IEnumerable<Course>>>(result);
            var returnedCourses = Assert.IsAssignableFrom<IEnumerable<Course>>(okResult.Value);
            Assert.Equal(2, returnedCourses.Count());
        }

        [Fact]
        public async Task GetTeachingCourses_TeacherRole_ReturnsTeachingCourses()
        {
            // Arrange
            var teacherId = 1;
            SetupUserWithRole(UserRole.Teacher, teacherId);

            var courses = new List<Course>
            {
                new Course { Id = 1, TeacherId = teacherId },
                new Course { Id = 2, TeacherId = teacherId }
            };

            _courseRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Func<Course, bool>>()))
                .ReturnsAsync(courses);

            // Act
            var result = await _controller.GetTeachingCourses();

            // Assert
            var okResult = Assert.IsType<ActionResult<IEnumerable<Course>>>(result);
            var returnedCourses = Assert.IsAssignableFrom<IEnumerable<Course>>(okResult.Value);
            Assert.Equal(2, returnedCourses.Count());
        }

        [Fact]
        public async Task SendCourse_RecruiterRole_ReturnsOk()
        {
            // Arrange
            var recruiterId = 1;
            var userId = 2;
            var courseId = 1;
            SetupUserWithRole(UserRole.Recruiter, recruiterId);

            var user = new User { Id = userId };
            var course = new Course { Id = courseId };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(user);

            _courseRepositoryMock.Setup(r => r.GetByIdAsync(courseId))
                .ReturnsAsync(course);

            var request = new CourseRequest { CourseId = courseId, UserId = userId };

            // Act
            var result = await _controller.SendCourse(request);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task GetCourseStatistics_TeacherRole_ReturnsStatistics()
        {
            // Arrange
            var teacherId = 1;
            var courseId = 1;
            SetupUserWithRole(UserRole.Teacher, teacherId);

            var course = new Course { Id = courseId, TeacherId = teacherId };
            var enrolledUsers = new List<UserCourse>
            {
                new UserCourse { CourseId = courseId, UserId = 1, IsEnrolled = true },
                new UserCourse { CourseId = courseId, UserId = 2, IsEnrolled = true }
            };

            _courseRepositoryMock.Setup(r => r.GetByIdAsync(courseId))
                .ReturnsAsync(course);

            _userCourseRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Func<UserCourse, bool>>()))
                .ReturnsAsync(enrolledUsers);

            // Act
            var result = await _controller.GetCourseStatistics(courseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var statistics = Assert.IsType<CourseStatistics>(okResult.Value);
            Assert.Equal(2, statistics.EnrolledUsersCount);
        }

        [Fact]
        public async Task GetCourseStatistics_OtherTeacher_ReturnsForbid()
        {
            // Arrange
            var courseId = 1;
            SetupUserWithRole(UserRole.Teacher, 2); // Different teacher ID

            var course = new Course { Id = courseId, TeacherId = 1 };

            _courseRepositoryMock.Setup(r => r.GetByIdAsync(courseId))
                .ReturnsAsync(course);

            // Act
            var result = await _controller.GetCourseStatistics(courseId);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        private void SetupUserWithRole(UserRole role, int? userId = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, role.ToString())
            };

            if (userId.HasValue)
            {
                claims.Add(new Claim(ClaimTypes.Name, userId.Value.ToString()));
            }

            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }
    }
} 