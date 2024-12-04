using companyappbasic.Data.Context;
using companyappbasic.Data.Entity;
using companyappbasic.Data.Models;
using companyappbasic.Services.AdminServices;
using companyappbasic.Services.EmployeeServices;
using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyAppTestProject.ServicesTests
{
    public class EmployeeServicesTest
    {
       
            private readonly Mock<ApplicationDBContext> _mockContext;
            private readonly EmployeeServi _employeeService;

            public EmployeeServicesTest()
            {
                _mockContext = new Mock<ApplicationDBContext>();
                _employeeService = new EmployeeServi(_mockContext.Object);
            }

            [Fact]
            public async Task EmployeeServices_GetAllAsync_ReturnAllEmployees()
            {
                // Arrange
                var employees = new List<Employee>{
                    new Employee { Id = 1, FirstName = "John", LastName = "Doe" },
                    new Employee { Id = 2, FirstName = "Jane", LastName = "Doe" }
                };
                _mockContext.Setup(x => x.Employees).ReturnsDbSet(employees);

                // Act
                var result = await _employeeService.GetAllAsync();

                // Assert
                result.Should().NotBeNullOrEmpty();
                result.Count.Should().Be(2);
                result[0].FirstName.Should().Be("John");
                result[1].FirstName.Should().Be("Jane");
            }

            [Fact]
            public async Task EmployeeServices_GetAllAsync_ReturnNotAllEmployees()
            {
                // Arrange
                _mockContext.Setup(x => x.Employees).ReturnsDbSet(new List<Employee>());

                // Act
                var result = await _employeeService.GetAllAsync();

                // Assert
                result.Should().BeEmpty();
            }

        [Fact]
            public async Task EmployeeServices_GetByIdAsync_ShouldReturnCorrectEmployee_WhenEmployeeExists()
            {
                // Arrange
                var employee = new Employee { Id = 1, FirstName = "John", LastName = "Doe" };
                _mockContext.Setup(x => x.Employees).ReturnsDbSet(new List<Employee> { employee });

                // Act
                var result = await _employeeService.GetByIdAsync(1);

                // Assert
                result.Should().NotBeNull();
                result.FirstName.Should().Be("John");
                result.LastName.Should().Be("Doe");
            }
       

            [Fact]
            public async Task EmployeeServices_GetByIdAsync_ShouldReturnCorrectEmployee_DoesntWhenEmployeeExists()
            {
                // Arrange
                _mockContext.Setup(x => x.Employees).ReturnsDbSet(new List<Employee>());

                // Act
                var result = await _employeeService.GetByIdAsync(99);

                // Assert
                result.Should().BeNull();
            }

            [Fact]
            public async Task EmployeeServices_CreateAsync_ShouldAddNewEmployee()
            {
                // Arrange
                var newEmployee = new Employee { Id = 3, FirstName = "Alice", LastName = "Smith" };
                _mockContext.Setup(x => x.Employees).ReturnsDbSet(new List<Employee>());

                // Act
                var result = await _employeeService.CreateAsync(newEmployee);

                // Assert
                result.Should().NotBeNull();
                result.FirstName.Should().Be("Alice");
                result.LastName.Should().Be("Smith");

                //Addasync yöntemi'nin sadece 1 kez çağrıldığını doğrulamak için;
                _mockContext.Verify(x => x.Employees.AddAsync(newEmployee, It.IsAny<CancellationToken>()), Times.Once);
                //SaveChangesAsync yöntemi'nin sadece 1 kez çağrıldığını doğrulamak için;
                _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            }

            [Fact]
            public async Task EmployeeServices_UpdateAsync_ShouldUpdateExistingEmployee()
            {
                // Arrange
                var existingEmployee = new Employee { Id = 1, FirstName = "John", LastName = "Doe" };
                var EmployeeList = new List<Employee> { existingEmployee };

               
                _mockContext.Setup(x => x.Employees).ReturnsDbSet(EmployeeList);

                // `FindAsync` metodunun doğru çalışması için ayarlandı
                _mockContext.Setup(x => x.Employees.FindAsync(It.IsAny<object[]>()))
                            .ReturnsAsync((object[] ids) => EmployeeList.FirstOrDefault(a => a.Id == (int)ids[0]));

                _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

                var updateDto = new UpdateEmployeesRequestDto { FirstName = "Aley", LastName = "Dote" };

                // Act
                var result = await _employeeService.UpdateAsync(1, updateDto);

                // Assert
                result.Should().NotBeNull();
                result?.FirstName.Should().Be("Aley");
                result?.LastName.Should().Be("Dote");
         
                // SaveChangesAsync'in bir kez çağrıldığını doğrula
                _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            }


            [Fact]
            public async Task EmployeeServices_DeleteAsync_ShouldRemoveEmployee_WhenEmployeeExists()
            {
                // Arrange
                var existingEmployee = new Employee { Id = 1, FirstName = "John", LastName = "Doe" };
                var employeeList = new List<Employee> { existingEmployee };
                _mockContext.Setup(x => x.Employees).ReturnsDbSet(employeeList);

                // Act
                var result = await _employeeService.DeleteAsync(1);

                // Assert
                result.Should().NotBeNull();
                _mockContext.Verify(x => x.Employees.Remove(It.IsAny<Employee>()), Times.Once);
                _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            }
            [Fact]
            public async Task EmployeeServices_DeleteAsync_ShouldRemoveEmployee_WhenEmployeedoesntExists()
            {
                // Arrange
                _mockContext.Setup(x => x.Employees).ReturnsDbSet(new List<Employee>());

                 // Act
                var result = await _employeeService.DeleteAsync(99);

                // Assert
                result.Should().BeNull();
            }
    }
    }

