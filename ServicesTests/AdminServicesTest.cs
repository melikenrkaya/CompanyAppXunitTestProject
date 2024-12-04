using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using companyappbasic.Data.Context;
using companyappbasic.Data.Entity;
using companyappbasic.Data.Models;
using companyappbasic.Services.AdminServices;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace CompanyAppTestProject.ServicesTests
{
    public class AdminServicesTest
    {
        private readonly Mock<ApplicationDBContext> _mockContext;
        private readonly Adminservi _adminService;

        public AdminServicesTest()
        {
            _mockContext = new Mock<ApplicationDBContext>();
            _adminService = new Adminservi(_mockContext.Object);
        }

        [Fact]
        public async Task AdminServices_GetAllAsync_ReturnAllAdmins()
        {
            // Arrange
            var adminList = new List<Admin>
            {
                new Admin { Id = 1, UserName = "Melike", Password = "password1", Role = "Admin" },
                new Admin { Id = 2, UserName = "Eyup", Password = "password2", Role = "User" }
            };
            _mockContext.Setup(x => x.Admins).ReturnsDbSet(adminList);

            // Act
            var result = await _adminService.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(admin => admin.UserName == "Melike");
            result.Should().Contain(admin => admin.UserName == "Eyup");
        }

        [Fact]
        public async Task AdminServices_GetByIdAsync_ShouldReturnCorrectAdmin_WhenAdminExists()
        {
            // Arrange
            var admin = new Admin { Id = 1, UserName = "Melike", Password = "password1", Role = "Admin" };
            _mockContext.Setup(x => x.Admins).ReturnsDbSet(new List<Admin> { admin });

            // Act
            var result = await _adminService.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result?.UserName.Should().Be("Melike");
        }

        [Fact]
        public async Task AdminServices_CreateAsync_ShouldAddNewAdmin()
        {
            // Arrange
            var newAdmin = new Admin { Id = 1, UserName = "NewAdmin", Password = "newpassword", Role = "Admin" };
            _mockContext.Setup(x => x.Admins).ReturnsDbSet(new List<Admin>());

            // Act
            var result = await _adminService.CreateAsync(newAdmin);

            // Assert
            result.Should().NotBeNull();
            result.UserName.Should().Be("NewAdmin");

            //Addasync yöntemi'nin sadece 1 kez çağrıldığını doğrulamak için;
            _mockContext.Verify(x => x.Admins.AddAsync(It.IsAny<Admin>(), default), Times.Once);

            //SaveChangesAsync yöntemi'nin sadece 1 kez çağrıldığını doğrulamak için;
            _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        }

        [Fact]
        public async Task AdminServices_UpdateAsync_ShouldUpdateExistingAdmin()
        {
            // Arrange
            var existingAdmin = new Admin { Id = 1, UserName = "Melike", Password = "oldpassword", Role = "Admin" };
            var adminList = new List<Admin> { existingAdmin };

            // `Admins` DbSet'i için `ReturnsDbSet` ile mock ayarlandı
            _mockContext.Setup(x => x.Admins).ReturnsDbSet(adminList);

            // `FindAsync` metodunun doğru çalışması için ayarlandı
            _mockContext.Setup(x => x.Admins.FindAsync(It.IsAny<object[]>()))
                        .ReturnsAsync((object[] ids) => adminList.FirstOrDefault(a => a.Id == (int)ids[0]));


            // SaveChangesAsync'in çağrıldığını doğrulamak için yapılandırma eklendi
            _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            
            var updateDto = new UpdateAdminRequestDto { UserName = "UpdatedAdmin", Password = "newpassword", Role = "SuperAdmin" };

            // Act
            var result = await _adminService.UpdateAsync(1, updateDto);

            // Assert
            result.Should().NotBeNull();
            result?.UserName.Should().Be("UpdatedAdmin");
            result?.Password.Should().Be("newpassword");
            result?.Role.Should().Be("SuperAdmin");

            // SaveChangesAsync'in bir kez çağrıldığını doğrula
            _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public async Task AdminServices_DeleteAsync_ShouldRemoveAdmin_WhenAdminExists()
        {
            // Arrange
            var adminToDelete = new Admin { Id = 1, UserName = "Melike", Password = "password1", Role = "Admin" };
            _mockContext.Setup(x => x.Admins).ReturnsDbSet(new List<Admin> { adminToDelete });

            // Act
            var result = await _adminService.DeleteAsync(1);

            // Assert
            result.Should().NotBeNull();
            result?.Id.Should().Be(1);
            _mockContext.Verify(x => x.Admins.Remove(It.IsAny<Admin>()), Times.Once);
            _mockContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }
    }
}

