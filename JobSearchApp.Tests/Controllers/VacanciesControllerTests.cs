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
    public class VacanciesControllerTests
    {
        private readonly Mock<IRepository<Vacancy>> _vacancyRepositoryMock;
        private readonly Mock<IRepository<Recruiter>> _recruiterRepositoryMock;
        private readonly Mock<IRepository<VacancyApplication>> _applicationRepositoryMock;
        private readonly Mock<IRepository<User>> _userRepositoryMock;
        private readonly VacanciesController _controller;

        public VacanciesControllerTests()
        {
            _vacancyRepositoryMock = new Mock<IRepository<Vacancy>>();
            _recruiterRepositoryMock = new Mock<IRepository<Recruiter>>();
            _applicationRepositoryMock = new Mock<IRepository<VacancyApplication>>();
            _userRepositoryMock = new Mock<IRepository<User>>();

            _controller = new VacanciesController(
                _vacancyRepositoryMock.Object,
                _recruiterRepositoryMock.Object,
                _applicationRepositoryMock.Object,
                _userRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateVacancy_RecruiterRole_ReturnsCreatedResult()
        {
            // Arrange
            var recruiterId = 1;
            var vacancy = new Vacancy { Id = 1, Title = "Test Vacancy" };
            SetupUserWithRole(UserRole.Recruiter, recruiterId);

            // Act
            var result = await _controller.CreateVacancy(vacancy);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(VacanciesController.GetVacancy), createdResult.ActionName);
            Assert.Equal(vacancy.Id, createdResult.RouteValues["id"]);
            Assert.Equal(VacancyStatus.Active, vacancy.Status);
            Assert.Equal(recruiterId, vacancy.RecruiterId);
            _vacancyRepositoryMock.Verify(r => r.AddAsync(vacancy), Times.Once);
        }

        [Fact]
        public async Task UpdateVacancy_OwnVacancy_ReturnsNoContent()
        {
            // Arrange
            var recruiterId = 1;
            var vacancy = new Vacancy { Id = 1, RecruiterId = recruiterId };
            SetupUserWithRole(UserRole.Recruiter, recruiterId);

            _vacancyRepositoryMock.Setup(r => r.GetByIdAsync(vacancy.Id))
                .ReturnsAsync(vacancy);

            // Act
            var result = await _controller.UpdateVacancy(vacancy.Id, vacancy);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _vacancyRepositoryMock.Verify(r => r.UpdateAsync(vacancy), Times.Once);
        }

        [Fact]
        public async Task UpdateVacancy_OtherVacancy_ReturnsForbid()
        {
            // Arrange
            var vacancy = new Vacancy { Id = 1, RecruiterId = 1 };
            SetupUserWithRole(UserRole.Recruiter, 2); // Different recruiter ID

            _vacancyRepositoryMock.Setup(r => r.GetByIdAsync(vacancy.Id))
                .ReturnsAsync(vacancy);

            // Act
            var result = await _controller.UpdateVacancy(vacancy.Id, vacancy);

            // Assert
            Assert.IsType<ForbidResult>(result);
            _vacancyRepositoryMock.Verify(r => r.UpdateAsync(vacancy), Times.Never);
        }

        [Fact]
        public async Task GetVacancy_ExistingVacancy_ReturnsVacancy()
        {
            // Arrange
            var vacancy = new Vacancy { Id = 1 };
            _vacancyRepositoryMock.Setup(r => r.GetByIdAsync(vacancy.Id))
                .ReturnsAsync(vacancy);

            // Act
            var result = await _controller.GetVacancy(vacancy.Id);

            // Assert
            var okResult = Assert.IsType<ActionResult<Vacancy>>(result);
            Assert.Equal(vacancy, okResult.Value);
        }

        [Fact]
        public async Task DeleteVacancy_OwnVacancy_ReturnsNoContent()
        {
            // Arrange
            var recruiterId = 1;
            var vacancy = new Vacancy { Id = 1, RecruiterId = recruiterId };
            SetupUserWithRole(UserRole.Recruiter, recruiterId);

            _vacancyRepositoryMock.Setup(r => r.GetByIdAsync(vacancy.Id))
                .ReturnsAsync(vacancy);

            // Act
            var result = await _controller.DeleteVacancy(vacancy.Id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Equal(VacancyStatus.Archived, vacancy.Status);
            _vacancyRepositoryMock.Verify(r => r.UpdateAsync(vacancy), Times.Once);
        }

        [Fact]
        public async Task SearchVacancies_ReturnsActiveVacancies()
        {
            // Arrange
            var query = "test";
            var vacancies = new List<Vacancy>
            {
                new Vacancy { Id = 1, Title = "Test Vacancy", Status = VacancyStatus.Active },
                new Vacancy { Id = 2, Title = "Another Test", Status = VacancyStatus.Active },
                new Vacancy { Id = 3, Title = "Inactive", Status = VacancyStatus.Archived }
            };

            _vacancyRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Func<Vacancy, bool>>()))
                .ReturnsAsync(vacancies.Where(v => v.Status == VacancyStatus.Active && 
                    (v.Title.Contains(query) || v.Description.Contains(query))));

            // Act
            var result = await _controller.SearchVacancies(query);

            // Assert
            var okResult = Assert.IsType<ActionResult<IEnumerable<Vacancy>>>(result);
            var returnedVacancies = Assert.IsAssignableFrom<IEnumerable<Vacancy>>(okResult.Value);
            Assert.Equal(2, returnedVacancies.Count());
        }

        [Fact]
        public async Task ApplyForVacancy_UserRole_ReturnsOk()
        {
            // Arrange
            var userId = 1;
            var vacancyId = 1;
            SetupUserWithRole(UserRole.User, userId);

            // Act
            var result = await _controller.ApplyForVacancy(vacancyId);

            // Assert
            Assert.IsType<OkResult>(result);
            _applicationRepositoryMock.Verify(r => r.AddAsync(It.Is<VacancyApplication>(a =>
                a.VacancyId == vacancyId &&
                a.UserId == userId &&
                a.Status == ApplicationStatus.Pending)), Times.Once);
        }

        [Fact]
        public async Task GetVacancyApplications_RecruiterRole_ReturnsApplications()
        {
            // Arrange
            var recruiterId = 1;
            var vacancyId = 1;
            SetupUserWithRole(UserRole.Recruiter, recruiterId);

            var vacancies = new List<Vacancy>
            {
                new Vacancy { Id = vacancyId, RecruiterId = recruiterId }
            };

            var applications = new List<VacancyApplication>
            {
                new VacancyApplication { Id = 1, VacancyId = vacancyId }
            };

            _vacancyRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Func<Vacancy, bool>>()))
                .ReturnsAsync(vacancies);

            _applicationRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Func<VacancyApplication, bool>>()))
                .ReturnsAsync(applications);

            // Act
            var result = await _controller.GetVacancyApplications();

            // Assert
            var okResult = Assert.IsType<ActionResult<IEnumerable<VacancyApplication>>>(result);
            var returnedApplications = Assert.IsAssignableFrom<IEnumerable<VacancyApplication>>(okResult.Value);
            Assert.Single(returnedApplications);
        }

        [Fact]
        public async Task UpdateApplicationStatus_OwnVacancy_ReturnsNoContent()
        {
            // Arrange
            var recruiterId = 1;
            var applicationId = 1;
            var vacancyId = 1;
            SetupUserWithRole(UserRole.Recruiter, recruiterId);

            var application = new VacancyApplication { Id = applicationId, VacancyId = vacancyId };
            var vacancy = new Vacancy { Id = vacancyId, RecruiterId = recruiterId };

            _applicationRepositoryMock.Setup(r => r.GetByIdAsync(applicationId))
                .ReturnsAsync(application);

            _vacancyRepositoryMock.Setup(r => r.GetByIdAsync(vacancyId))
                .ReturnsAsync(vacancy);

            // Act
            var result = await _controller.UpdateApplicationStatus(applicationId, ApplicationStatus.Accepted);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Equal(ApplicationStatus.Accepted, application.Status);
            _applicationRepositoryMock.Verify(r => r.UpdateAsync(application), Times.Once);
        }

        [Fact]
        public async Task GetManagedVacancies_RecruiterRole_ReturnsVacancies()
        {
            // Arrange
            var recruiterId = 1;
            SetupUserWithRole(UserRole.Recruiter, recruiterId);

            var vacancies = new List<Vacancy>
            {
                new Vacancy { Id = 1, RecruiterId = recruiterId },
                new Vacancy { Id = 2, RecruiterId = recruiterId }
            };

            _vacancyRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Func<Vacancy, bool>>()))
                .ReturnsAsync(vacancies);

            // Act
            var result = await _controller.GetManagedVacancies();

            // Assert
            var okResult = Assert.IsType<ActionResult<IEnumerable<Vacancy>>>(result);
            var returnedVacancies = Assert.IsAssignableFrom<IEnumerable<Vacancy>>(okResult.Value);
            Assert.Equal(2, returnedVacancies.Count());
        }

        [Fact]
        public async Task GetManagedUsers_RecruiterRole_ReturnsUsers()
        {
            // Arrange
            var recruiterId = 1;
            SetupUserWithRole(UserRole.Recruiter, recruiterId);

            var users = new List<User>
            {
                new User { Id = 1, FirstName = "Test", LastName = "User" },
                new User { Id = 2, FirstName = "Another", LastName = "User" }
            };

            _userRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Func<User, bool>>()))
                .ReturnsAsync(users);

            // Act
            var result = await _controller.GetManagedUsers();

            // Assert
            var okResult = Assert.IsType<ActionResult<IEnumerable<User>>>(result);
            var returnedUsers = Assert.IsAssignableFrom<IEnumerable<User>>(okResult.Value);
            Assert.Equal(2, returnedUsers.Count());
        }

        [Fact]
        public async Task SendResume_RecruiterRole_ReturnsOk()
        {
            // Arrange
            var recruiterId = 1;
            var userId = 2;
            SetupUserWithRole(UserRole.Recruiter, recruiterId);

            var recruiter = new Recruiter { Id = recruiterId };
            var user = new User { Id = userId };

            _recruiterRepositoryMock.Setup(r => r.GetByIdAsync(recruiterId))
                .ReturnsAsync(recruiter);

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(user);

            var request = new ResumeRequest { RecruiterId = recruiterId, UserId = userId };

            // Act
            var result = await _controller.SendResume(request);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task SendCourse_RecruiterRole_ReturnsOk()
        {
            // Arrange
            var recruiterId = 1;
            var userId = 2;
            SetupUserWithRole(UserRole.Recruiter, recruiterId);

            var user = new User { Id = userId };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(user);

            var request = new CourseRequest { CourseId = 1, UserId = userId };

            // Act
            var result = await _controller.SendCourse(request);

            // Assert
            Assert.IsType<OkResult>(result);
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