using companyappbasic.Controller;
using companyappbasic.Data.Context;
using companyappbasic.Data.Entity;
using companyappbasic.Common.Extensions;
using companyappbasic.Data.Models;
using companyappbasic.Services.EmployeeServices;
using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;


namespace CompanyAppTestProject.ControllerTests
{
    public class EmployeeControllerTest
    {
        private readonly EmployeeController _controller;
        private readonly ApplicationDBContext _dbContext;
        private readonly Mock<IEmployee> _mockemployeesServices;


        public EmployeeControllerTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDB")
                .Options;

            _dbContext = new ApplicationDBContext(options);
            _mockemployeesServices = new Mock<IEmployee>();
            _controller = new EmployeeController(_dbContext, _mockemployeesServices.Object);
        }


        [Fact]
        public async Task EmployeeController_GetAll_ResultOk()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new Employee {FirstName = "John", LastName = "Doe" },
                new Employee {FirstName = "Jane", LastName = "Smith" }
            };
            _mockemployeesServices.Setup(x => x.GetAllAsync()).ReturnsAsync(employees);
           
            // Act
            var result = await _controller.GetALL();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Which;
            var returnedEmployees = okResult.Value.Should().BeOfType<List<Employee>>().Which;

            returnedEmployees.Should().HaveCount(2);
            returnedEmployees[0].FirstName.Should().Be("John");
            returnedEmployees[1].FirstName.Should().Be("Jane");
        }

        [Fact]
        public async Task EmployeeController_GetAll_ResultBadRequest()
        {
            // Arrange

           _mockemployeesServices.Setup(x => x.GetAllAsync())!.ReturnsAsync((List<Employee>)null);            
          
            // Act
            var result = await _controller.GetALL();

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Which;
            notFoundResult.Value.Should().Be("Employees bulunamadı.");

        }

        [Fact]
        public async Task EmployeeController_GetById_ResultOk()
        {
            // Arrange
            var employee = new Employee { Id = 1, FirstName = "John", LastName = "Doe" };
            _mockemployeesServices.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(employee);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Which;
            var returnedEmployee = okResult.Value.Should().BeOfType<EmployeeDto>().Which;

            returnedEmployee.FirstName.Should().Be("John");
            returnedEmployee.LastName.Should().Be("Doe");
        }

        [Fact]
        public async Task EmployeeController_GetById_NotFoundResult()
        {
            // Arrange
            _mockemployeesServices.Setup(x => x.GetByIdAsync(1))!.ReturnsAsync((Employee)null);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task EmployeeController_Create_ResultOk()
        {
            // Arrange
            var createDto = new CreateEmployeesRequestDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com"
            };

            var employeeModel = new Employee { FirstName = "John", LastName = "Doe" };

            _mockemployeesServices.Setup(x => x.CreateAsync(It.IsAny<Employee>()))
                                .ReturnsAsync(employeeModel);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Which;
            var returnedEmployee = createdResult.Value.Should().BeOfType<EmployeeDto>().Which;

            returnedEmployee.FirstName.Should().Be("John");
            returnedEmployee.LastName.Should().Be("Doe");

        }

        [Fact]
        public async Task EmployeeController_Create_ResultBadRequest()
        {

            // Arrange
            var createDto = new CreateEmployeesRequestDto
            {
                FirstName = "",
                LastName = "",
                Email = "melike.kaya@gmail.com"
            };
            //İstediğim satırı boş bırakarak deneyebilirim.

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();

        }

        [Fact]
        public async Task EmployeeController_Update_ResultOk()
        {
            // Arrange
                int id = 1;
                var updateDto = new UpdateEmployeesRequestDto {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com"
                };
                var updatedEmployee = new Employee
                {
                    FirstName = "Melike",
                    LastName = "Doe",
                    Email = "john.doe@gmail.com"
                };

            _mockemployeesServices.Setup(s => s.UpdateAsync(id, updateDto)).ReturnsAsync(updatedEmployee);

                // Act
                var result = await _controller.Update(id, updateDto);

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                Assert.Equal(200, okResult.StatusCode);
                Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task EmployeeController_Update_ResultIdBadRequest()
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
        public async Task EmployeeController_Update_ResultIdNotFound()
        {
            // Arrange
            int id = 1;
            UpdateEmployeesRequestDto updateDto = null!;

            _mockemployeesServices.Setup(s => s.UpdateAsync(id, updateDto!)).ReturnsAsync((Employee)null);

            // Act
            var result = await _controller.Update(id, updateDto!);

            // Assert
            var NotfoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, NotfoundResult.StatusCode);
        }

        [Fact]
        public async Task EmployeeController_Delete_ResultOk()
        {
            // Arrange
            int id = 1;
            var employee = new Employee 
            {
                FirstName = "Melike",
                LastName = "Doe",
                Email = "john.doe@gmail.com"
            };

            _mockemployeesServices.Setup(s => s.DeleteAsync(id)).ReturnsAsync(employee);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
        [Fact]
        public async Task EmployeeController_Delete_ResultIdBadRequest()
        {
            // Arrange
            int id = 0;
            // Act
            var result = await _controller.Delete(id);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }
        public async Task EmployeeController_Delete_ResultIdNotFound()
        {
            // Arrange
            int id = 1;
            _mockemployeesServices.Setup(s => s.DeleteAsync(id)).ReturnsAsync((Employee)null);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var NotfoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, NotfoundResult.StatusCode);

        }



    }
}
