using companyappbasic.Data.Context;
using companyappbasic.Data.Entity;
using companyappbasic.Data.Models;
using companyappbasic.Services.AdminServices;
using FluentAssertions;
using FluentAssertions.Common;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyAppTestProject.ServicesTests
{
    public class AdminServicesTest
    {
        private readonly ApplicationDBContext _dbContext;
        private readonly Adminservi _AdminServices;

        public AdminServicesTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                 .UseInMemoryDatabase(databaseName: "TestDB")
                 .Options;

            _dbContext = new ApplicationDBContext(options);
            _AdminServices = new Adminservi(_dbContext);
        }

        [Fact]
        public async Task AdminServices_GetAll_ToList()
        {
            var admins = new List<Admin> { new Admin { Id = 1, UserName = "admin1" } };
            await _dbContext.Admins.AddRangeAsync(admins);
            await _dbContext.SaveChangesAsync();
            // Act
            var result = await _AdminServices.GetAllAsync();

            // Assert
            result.Should().HaveCount(1);
            result[0].UserName.Should().Be("admin1");
        }
        [Fact]
        public async Task AdminServices_GetById_FirstOrDefaultAsync()
        {
            // Arrange
            var admin = new Admin { Id = 1, UserName = "admin1" };
            // _AdminServices.Setup(c => c.Admins.FindAsync(1)).ReturnsAsync(admin);

            // Act
            var result = await _AdminServices.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.UserName.Should().Be("admin1");
        }
        public async Task AdminServices_Create_AddAsync()
        {

        }
        public async Task AdminServices_Update_FindAsync()
        {

        }
        public async Task  AsminServices_Delete_FirstOrDefaultAsync()
        {

        }

    }
}
