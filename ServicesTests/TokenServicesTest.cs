using companyappbasic.Data.Entity;
using companyappbasic.Services.AppUserServices;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CompanyAppTestProject.ServicesTests
{
    public class TokenServicesTest
    {
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<UserManager<AppUser>> _userManagerMock;
        private readonly ResponseModel<string> _response;
        private readonly TokenServi _tokenServi;

        public TokenServicesTest()
        {
            _configMock = new Mock<IConfiguration>();
            _userManagerMock = new Mock<UserManager<AppUser>>(
                new Mock<IUserStore<AppUser>>().Object, null, null, null, null, null, null, null, null);

            _configMock.Setup(c => c["JWT:SigningKey"]).Returns("vb9lşfkekrbeşvwkvşbeljwbterı4893985uıerfırherfwjkuruhfıevwrhvjevhoeıheşk8y5t9845turhefeladal40uhghjjhjwke");
            _configMock.Setup(c => c["JWT:Issuer"]).Returns("your_issuer");
            _configMock.Setup(c => c["JWT:Audience"]).Returns("your_audience");

            _response = new ResponseModel<string>();
            _tokenServi = new TokenServi(_configMock.Object, _userManagerMock.Object, _response);
        }



        [Fact]
        public void TokenServices_CreateToken_ShouldThrowArgumentNullException()
        {
            // Arrange
            var user = new AppUser { Email = null, UserName = null };

            // Act

            var result = _tokenServi.CreateToken(user);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("User email or username is null.");
            result.Data.Should().BeNull();
        }


        [Fact]
        public void TokenServices_CreateToken_ShouldReturnToken()
        {
            // Arrange
            var user = new AppUser { Email = "test@gmail.com", UserName = "TestUser" };
            var roles = new List<string> { "Admin", "Employee" };

            _userManagerMock.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(roles);
            _userManagerMock.Setup(um => um.GetRolesAsync(It.IsAny<AppUser>())).ReturnsAsync(roles);

            // Act
            var token = _tokenServi.CreateToken(user);

            // Assert
            token.Success.Should().BeTrue();
            token.Message.Should().Be("Token oluşturuldu.");
            token.Data.Should().NotBeNull(); // Tokenin oluşturulduğunu kontrol eder
            token.Data.Should().BeOfType<string>();
            token.Data.Should().NotBeNullOrEmpty();
            token.Data.Split('.').Length.Should().Be(3); // JWT format kontrolü: Header, Payload, Signature

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token.Data) as JwtSecurityToken;

            jwtToken.Should().NotBeNull();
            jwtToken.Claims.Should().Contain(c => c.Value == "Admin");
            jwtToken.Claims.Should().Contain(c => c.Value == "Employee");
            jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
            jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.GivenName && c.Value == user.UserName);




        }
    }
}