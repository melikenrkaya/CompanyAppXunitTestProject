using companyappbasic.Controller;
using companyappbasic.Data.Context;
using companyappbasic.Data.Entity;
using companyappbasic.Data.Models;
using companyappbasic.Services.AdminServices;
using companyappbasic.Services.EmployeeServices;
using companyappbasic.Services.TaskServices;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyAppTestProject.ControllerTests
{
    public class AdminControllerTest
    {
        private readonly AdminController _controller;
        private readonly ApplicationDBContext _dbContext;
        private readonly Mock<IAdmin> _mockAdminServices;

        public AdminControllerTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                 .UseInMemoryDatabase(databaseName: "TestDB")
                 .Options;

            _dbContext = new ApplicationDBContext(options);
            _mockAdminServices = new Mock<IAdmin>();
            _controller = new AdminController(_dbContext, _mockAdminServices.Object);
        }

        [Fact]
        public async Task AdminController_GetAll_ResultOk()
        {
            // Arrange
            var admin = new List<Admin>
            {
                new Admin {UserName = "John", Password = "ret5y6u76r" ,Role="Yönetici"},
                new Admin {UserName = "Jane", Password = "ety6u5uye",Role="Yapımcı" }
            };
            _mockAdminServices.Setup(x => x.GetAllAsync()).ReturnsAsync(admin);

            // Act
            var result = await _controller.GetALL();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AdminController_GetAll_ResultBadRequest()
        {
            //Arrange
            _mockAdminServices.Setup(x => x.GetAllAsync()).ReturnsAsync((List<Admin>)null);

            //Act
            var result = await _controller.GetALL();

            //Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Which;
            notFoundResult.Value.Should().Be("Admin bulunamadı.");
        }

        [Fact]
        public async Task AdminController_GetById_ResultOk()
        {
            // Arrange
            var admin = new Admin { Id = 1, UserName = "John", Password = "ret5y6u76r", Role = "Yönetici" };
            _mockAdminServices.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(admin);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();

        }
        [Fact]
        public async Task AdminController_GetById_NotFoundResult()
        {
            // Arrange
            _mockAdminServices.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Admin)null);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }
        [Fact]
        public async Task AdminController_Create_ResultOk()
        {
            // Arrange
            var createDto = new CreateAdminRequestDto
            {
                UserName = "John",
                Password = "frgdhtjykul34",
                Role = "Yönetici"
            };

            var adminModel = new Admin {
                UserName = "John",
                Password = "frgdhtjykul34",
                Role = "Yönetici"
            };

            _mockAdminServices.Setup(x => x.CreateAsync(It.IsAny<Admin>()))
                                .ReturnsAsync(adminModel);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Which;
            createdResult.Value.Should().BeEquivalentTo(adminModel);

        }
        [Fact]
        public async Task AdminController_Create_ResultBadRequest()
        {
            // Arrange
            var createDto = new CreateAdminRequestDto
            {
                UserName = "John",
                Password = "",
                Role = "Yönetici"
            };

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }
        [Fact]
        public async Task AdminController_Update_ResultOk()
        {
            // Arrange
            int id = 1;
            var updateDto = new UpdateAdminRequestDto
            {
                UserName = "John",
                Password = "dfretadb",
                Role = "Yönetici"
            };
            var updatedAdmin = new Admin
            {
                UserName = "John",
                Password = "dfretadb",
                Role = "Yardımcı"
            };

            _mockAdminServices.Setup(s => s.UpdateAsync(id, updateDto)).ReturnsAsync(updatedAdmin);

            // Act
            var result = await _controller.Update(id, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(okResult.Value);
        }
 
        [Fact]
        public async Task AdminController_Update_ResultIdBadRequest()
        {
            // Arrange
            int id = 0;
            // Act
            var result = await _controller.Update(id, null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }
        [Fact]
        public async Task AdminController_Update_ResultIdNotFound()
        {
            // Arrange
            int id = 1;
            UpdateAdminRequestDto updateDto = new UpdateAdminRequestDto
            {
                UserName = "",
                Password = "password",
                Role = "Yönetici"
            };

            _mockAdminServices.Setup(s => s.UpdateAsync(id, updateDto)).ReturnsAsync((Admin)null);

            // Act
            var result = await _controller.Update(id, updateDto);

            // Assert
            var NotfoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, NotfoundResult.StatusCode);
        }
        [Fact]
        public async Task AdminController_Delete_ResultOk()
        {
            // Arrange
            int id = 1;
            var admin = new Admin
            {
                UserName = "John",
                Password = "dfretadb",
                Role = "Yardımcı"
            };

            _mockAdminServices.Setup(s => s.DeleteAsync(id)).ReturnsAsync(admin);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
        [Fact]
        public async Task AdminController_Delete_ResultIdBadRequest()
        {
            // Arrange
            int id = 0;
            // Act
            var result = await _controller.Delete(id);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task AdminController_Delete_ResultIdNotFound()
        { 
            // Arrange
            int id = 1;
            _mockAdminServices.Setup(s => s.DeleteAsync(id)).ReturnsAsync((Admin)null);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var NotfoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, NotfoundResult.StatusCode);
        }
       
    } 
}
