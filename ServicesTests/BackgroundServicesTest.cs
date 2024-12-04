using companyappbasic.Data.Context;
using companyappbasic.Data.Entity;
using companyappbasic.Services.BackgroundServices;
using companyappbasic.Services.EmailServices;
using FluentAssertions;
using FluentAssertions.Common;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyAppTestProject.ServicesTests
{
    public class BackgroundServicesTest
    {
        private readonly Mock<ApplicationDBContext> _mockcontext;
        private readonly BackgroundServi _backgroundservi;

        public BackgroundServicesTest()
        {
            _mockcontext = new Mock<ApplicationDBContext>();
            _backgroundservi = new BackgroundServi(_mockcontext.Object);
        }

        [Fact]
        public void BackgroundService_CheckAndUpdateRecords_ShouldAddOrUpdateRecord()
        {
            // Arrange
            var loginLogs = new List<LoginLog>
            {
                new LoginLog { UserId = "1", UserName = "User1", Email = "user1@gmail.com", NumberOfLogin = 1 },
                new LoginLog { UserId = "2", UserName = "User2", Email = "user2@gmail.com", NumberOfLogin = 2 }
            };

            var recordOfBackgroundJobs = new List<RecordOfBackgroungJobs>
            {
                new RecordOfBackgroungJobs { UserId = "1", UserName = "User1", Email = "user1@gmail.com", NumberOfBackjob = 0 },
                new RecordOfBackgroungJobs { UserId = "2", UserName = "User2", Email = "user2@gmail.com", NumberOfBackjob = 0 }

            };

            // DbSet mock'larını ReturnDbSet ile oluşturma
            _mockcontext.Setup(c => c.LoginLogss).ReturnsDbSet(loginLogs);
            _mockcontext.Setup(c => c.RecordOfBackgroungJobss).ReturnsDbSet(recordOfBackgroundJobs);

            //Act
            _backgroundservi.CheckAndUpdateRecords();

            // Assert
            _mockcontext.Verify(c => c.SaveChanges(), Times.Once);

            recordOfBackgroundJobs.Should().ContainSingle(r => r.UserId == "1" && r.NumberOfBackjob == 1);
            recordOfBackgroundJobs.Should().ContainSingle(r => r.UserId == "2" && r.NumberOfBackjob == 2);
        }

        [Fact]
        public void BackgroundService_CheckAndUpdateRecords_ShouldNotAddOrUpdateRecord()
        {
            //Arrange
            var loginLogs = new List<LoginLog>(); // Boş bir liste veriyoruz
            var recordOfBackgroundJobs = new List<RecordOfBackgroungJobs>(); // Boş bir liste veriyoruz

            _mockcontext.Setup(c => c.LoginLogss).ReturnsDbSet(loginLogs);
            _mockcontext.Setup(c => c.RecordOfBackgroungJobss).ReturnsDbSet(recordOfBackgroundJobs);
            
            //Act
            _backgroundservi.CheckAndUpdateRecords();

            //Assert
            _mockcontext.Verify(c => c.SaveChanges(), Times.Never); // SaveChanges'in çağrılmadığını doğruluyoruz
            recordOfBackgroundJobs.Should().BeEmpty();
        }


        [Fact]
        public async Task BackgroundService_AddLoginLogAsync_ShouldAddOrUpdateLoginLog()
        {
            // Arrange
            var users = new List<AppUser> { new AppUser { Id = "1", UserName = "User1", Email = "user1@gmail.com" } };
            var loginLogs = new List<LoginLog>
            {
                new LoginLog { UserId = "1", UserName = "User1", Email = "user1@gmail.com", NumberOfLogin = 1 }
            };

            // DbSet mock'larını ReturnDbSet ile oluşturma
            _mockcontext.Setup(c => c.Users).ReturnsDbSet(users);
            _mockcontext.Setup(c => c.LoginLogss).ReturnsDbSet(loginLogs);

            //Act
            await _backgroundservi.AddLoginLogAsync("User1", "user1@gmail.com", "1");

            // Assert

            _mockcontext.Verify(c => c.SaveChangesAsync(default), Times.Once);
            loginLogs.Should().ContainSingle(l => l.UserId == "1" && l.NumberOfLogin == 2);
        }

        [Fact]
        public async Task BackgroundService_AddLoginLogAsync_ShouldNotAddOrUpdateLoginLog()
        {
            //Arrange
            var users = new List<AppUser>(); // Boş bir kullanıcı listesi
            var loginLogs = new List<LoginLog>(); // Boş bir login log listesi

            // DbSet mock'larını ReturnDbSet ile oluşturma
            _mockcontext.Setup(c => c.Users).ReturnsDbSet(users);
            _mockcontext.Setup(c => c.LoginLogss).ReturnsDbSet(loginLogs);

            //Act
            await _backgroundservi.AddLoginLogAsync("NonExistentUser", "nonexistent@gmail.com", "999");

            //Assert
            _mockcontext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never); // SaveChangesAsync'in çağrılmadığını doğruluyoruz
            loginLogs.Should().BeEmpty(); // Login loglarının boş olduğunu doğruluyoruz
        }
    }
}