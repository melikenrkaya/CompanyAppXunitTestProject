using companyappbasic.Controller;
using companyappbasic.Data.Context;
using companyappbasic.Data.Entity;
using companyappbasic.Data.Models;
using companyappbasic.Services.EmployeeServices;
using companyappbasic.Services.TaskServices;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyAppTestProject.ControllerTests
{
    public class TaskControllerTest
    {
        private readonly TaskController _controller;
        private readonly ApplicationDBContext _dbContext;
        private readonly Mock<ITask> _mocktaskServices;

        public TaskControllerTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
    .UseInMemoryDatabase(databaseName: "TestDB")
    .Options;

            _dbContext = new ApplicationDBContext(options);
            _mocktaskServices = new Mock<ITask>();
            _controller = new TaskController(_dbContext, _mocktaskServices.Object);
        }

        [Fact]
        public async Task TaskController_GetAll_ResultOk()
        {
            // Arrange
            var task = new List<Tasks>
            {
                new Tasks {Title = "Temızlık", Description = "temızlık onemlı" ,AssignedToEmployeeId=5},
                new Tasks {Title = "Mesai", Description = "Çift Mesai" ,AssignedToEmployeeId=3},
            };
            _mocktaskServices.Setup(x => x.GetAllAsync()).ReturnsAsync(task);

            // Act
            var result = await _controller.GetALL();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }
       

        [Fact]
        public async Task TaskController_GetAll_ResultBadRequest()
        {
            //Arrange
            _mocktaskServices.Setup(x => x.GetAllAsync()).ReturnsAsync((List<Tasks>)null);

            //Act
            var result = await _controller.GetALL();

            //Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Which;
            notFoundResult.Value.Should().Be("Tasks bulunamadı.");
        }

        [Fact]
        public async Task TaskController_GetById_ResultOk()
        {
            // Arrange
            var task = new Tasks { Id = 1,Title = "Temızlık", Description = "temızlık onemlı" ,AssignedToEmployeeId=5};
            _mocktaskServices.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(task);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }
        [Fact]
        public async Task TaskController_GetById_NotFoundResult()
        {   // Arrange
            _mocktaskServices.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Tasks)null);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }
        [Fact]
        public async Task TaskController_Create_ResultOk()
        {   // Arrange
            var createDto = new CreateTaskRequestDto
            {
               Title = "Temızlık", 
               Description = "temızlık onemlı",
               AssignedToEmployeeId=5
            };

            var taskModel = new Tasks
            {
                Title = "Temızlık",
                Description = "temızlık onemlı",
                AssignedToEmployeeId = 5
            };

            _mocktaskServices.Setup(x => x.CreateAsync(It.IsAny<Tasks>()))
                                .ReturnsAsync(taskModel);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
        }

        [Fact]
        public async Task TaskController_Create_ResultBadRequest()
        {
            // Arrange
            var createDto = new CreateTaskRequestDto
            {
                Title = "Temızlık",
                Description = "",
                AssignedToEmployeeId = 5
            };

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }
        [Fact]
        public async Task TaskController_Update_ResultOk()
        {
            // Arrange
            int id = 1;
            var updateDto = new UpdateTaskRequestDto
            {
                Title = "Temızlık",
                Description = "temızlık onemlı",
                AssignedToEmployeeId = 5
            };
            var updatedTask = new Tasks
            {
                Title = "Temızlık",
                Description = "temızlık onemlı",
                AssignedToEmployeeId = 4
            };

            _mocktaskServices.Setup(s => s.UpdateAsync(id, updateDto)).ReturnsAsync(updatedTask);

            // Act
            var result = await _controller.Update(id, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(okResult.Value);
        }
        [Fact]
        public async Task TaskController_Update_ResultIdBadRequest()
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
        public async Task TaskController_Update_ResultIdNotFound()
        {

            // Arrange
            int id = 1;
            UpdateTaskRequestDto updateDto = null;

            _mocktaskServices.Setup(s => s.UpdateAsync(id, updateDto)).ReturnsAsync((Tasks)null);

            // Act
            var result = await _controller.Update(id, updateDto);

            // Assert
            var NotfoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, NotfoundResult.StatusCode);
        }
        [Fact]
        public async Task TaskController_Delete_ResultOk()
        {
            // Arrange
            int id = 1;
            var task = new Tasks
            {
                Title = "Temızlık",
                Description = "temızlık onemlı",
                AssignedToEmployeeId = 4
            };

            _mocktaskServices.Setup(s => s.DeleteAsync(id)).ReturnsAsync(task);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
        [Fact]
        public async Task TaskController_Delete_ResultIdBadRequest()
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
        public async Task TaskController_Delete_ResultIdNotFound()
        {
            // Arrange
            int id = 1;
            _mocktaskServices.Setup(s => s.DeleteAsync(id)).ReturnsAsync((Tasks)null);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var NotfoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, NotfoundResult.StatusCode);
        }
    }
}
