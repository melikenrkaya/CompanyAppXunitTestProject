using companyappbasic.Data.Context;
using companyappbasic.Data.Entity;
using companyappbasic.Data.Models;
using companyappbasic.Services.EmployeeServices;
using companyappbasic.Services.TaskServices;
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
    public class TaskServicesTest
    {


        private readonly Mock<ApplicationDBContext> _mockContext;
        private readonly TaskServi _taskService;

        public TaskServicesTest()
        {
            _mockContext = new Mock<ApplicationDBContext>();
            _taskService = new TaskServi(_mockContext.Object);
        }

        [Fact]
        public async Task TaskServices_GetAllAsync_ReturnAllTask()
        {
            // Arrange
            var task = new List<Tasks>{
            new Tasks { Id = 1, Title = "John", Description = "Doe",AssignedToEmployeeId=1 },
            new Tasks { Id = 1, Title = "Ayşe", Description = "önemli mesaj",AssignedToEmployeeId=5 },
        };

            _mockContext.Setup(x => x.Taskss).ReturnsDbSet(task);

            // Act
            var result = await _taskService.GetAllAsync();

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Count.Should().Be(2);
            result[0].Title.Should().Be("John");
            result[1].Title.Should().Be("Ayşe");
        }

        [Fact]
        public async Task TaskServices_GetAllAsync_ReturnNotAllTask()
        {
            // Arrange
            _mockContext.Setup(x => x.Taskss).ReturnsDbSet(new List<Tasks>());

            // Act
            var result = await _taskService.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task TaskServices_GetByIdAsync_ShouldReturnCorrectTask()
        {
            // Arrange
            var task = new Tasks { Id = 1, Title = "John", Description = "Doe", AssignedToEmployeeId = 1 };
            _mockContext.Setup(x => x.Taskss).ReturnsDbSet(new List<Tasks> {task});

            // Act
            var result = await _taskService.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("John");
            result.Description.Should().Be("Doe");
        }


        [Fact]
        public async Task TaskServices_GetByIdAsync_DoenstShouldReturnCorrectTask()
        {
            // Arrange
            _mockContext.Setup(x => x.Taskss).ReturnsDbSet(new List<Tasks>());

            // Act
            var result = await _taskService.GetByIdAsync(99);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task TaskServices_CreateAsync_ShouldAddNewTask()
        {
            // Arrange
            var task = new Tasks { Id = 1, Title = "Alice", Description = "Smith", AssignedToEmployeeId = 1 };
            _mockContext.Setup(x => x.Taskss).ReturnsDbSet(new List<Tasks>());

            // Act
            var result = await _taskService.CreateAsync(task);

            // Assert
            result.Should().NotBeNull();
            result?.Title.Should().Be("Alice");
            result?.Description.Should().Be("Smith");

            //Addasync yöntemi'nin sadece 1 kez çağrıldığını doğrulamak için;
            _mockContext.Verify(x => x.Taskss.AddAsync(task, It.IsAny<CancellationToken>()), Times.Once);
            //SaveChangesAsync yöntemi'nin sadece 1 kez çağrıldığını doğrulamak için;
            _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TaskServices_UpdateAsync_ShouldUpdateExistingTask()
        {
            // Arrange
            var existingTask = new Tasks { Id = 1, Title = "Alice", Description = "Smith", AssignedToEmployeeId = 1 };
            var TaskList = new List<Tasks> { existingTask };

            // `Admins` DbSet'i için `ReturnsDbSet` ile mock ayarlandı
            _mockContext.Setup(x => x.Taskss).ReturnsDbSet(TaskList);

            // `FindAsync` metodunun doğru çalışması için ayarlandı
            _mockContext.Setup(x => x.Taskss.FindAsync(It.IsAny<object[]>()))
                        .ReturnsAsync((object[] ids) => TaskList.FirstOrDefault(a => a.Id == (int)ids[0]));

            _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var updateDto = new UpdateTaskRequestDto { Title = "Aley", Description = "Dote", AssignedToEmployeeId = 4 };

            // Act
            var result = await _taskService.UpdateAsync(1, updateDto);

            // Assert
            result.Should().NotBeNull();
            result?.Title.Should().Be("Aley");
            result?.Description.Should().Be("Dote");
            result?.AssignedToEmployeeId.Should().Be(4);

            // SaveChangesAsync'in bir kez çağrıldığını doğrula
            _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public async Task TaskServices_DeleteAsync_ShouldRemoveTask()
        {
            // Arrange
            var existingTask = new Tasks { Id = 1, Title = "Alice", Description = "Smith", AssignedToEmployeeId = 1 };
            var TaskList = new List<Tasks> { existingTask };
            _mockContext.Setup(x => x.Taskss).ReturnsDbSet(TaskList);

            // Act
            var result = await _taskService.DeleteAsync(1);

            // Assert
            result.Should().NotBeNull();
            _mockContext.Verify(x => x.Taskss.Remove(It.IsAny<Tasks>()), Times.Once);
            _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        }
        [Fact]
        public async Task TaskServices_DeleteAsync_NotShouldRemoveTask()
        {
            // Arrange
            _mockContext.Setup(x => x.Taskss).ReturnsDbSet(new List<Tasks>());

            // Act
            var result = await _taskService.DeleteAsync(99);

            // Assert
            result.Should().BeNull();
        }
    
}
}
