using companyappbasic.Controller;
using companyappbasic.Data.Context;
using companyappbasic.Data.Entity;
using companyappbasic.Data.Models;
using companyappbasic.Services.AppUserServices;
using companyappbasic.Services.BackgroundServices;
using companyappbasic.Services.RabbitMQServices;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;


namespace CompanyAppTestProject.ControllerTests
{
    public class AccountControllerTest
    {
        private readonly AccountController _controller;
        private readonly Mock<UserManager<AppUser>> _userManagerMock;
        private readonly Mock<SignInManager<AppUser>> _signInManagerMock;
        private readonly Mock<IToken> _tokenServiceMock;
        private readonly Mock<BackgroundServi> _backgroundServiMock;
        private readonly Mock<IRabbitProducer> _rabbitProducerMock;
        private readonly ApplicationDBContext _dbContext;


        public AccountControllerTest()
        {
            // DbContextOptions oluştur
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase") // In-memory database kullanımı
                .Options;

            // ApplicationDBContext için sahte veritabanı oluştur
            _dbContext = new ApplicationDBContext(options);



            // UserManager ve SignInManager için Moq nesneleri oluşturuyoruz
            var userStoreMock = new Mock<IUserStore<AppUser>>();
            var identityOptions = new Mock<IOptions<IdentityOptions>>();
            identityOptions.Setup(o => o.Value).Returns(new IdentityOptions());

            _userManagerMock = new Mock<UserManager<AppUser>>(userStoreMock.Object,
            identityOptions.Object,
            null, // IPasswordHasher<AppUser>
            null, // IUserValidator<AppUser>[] 
            null, // IPasswordValidator<AppUser>[]
            null, // ILookupNormalizer
            null, // IdentityErrorDescriber
            null, // IServiceProvider
            null  // ILogger<UserManager<AppUser>>
            );

            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<AppUser>>();
            var logger = new Mock<ILogger<SignInManager<AppUser>>>();

            _signInManagerMock = new Mock<SignInManager<AppUser>>(_userManagerMock.Object,
                contextAccessor.Object,
                claimsFactory.Object,
                identityOptions.Object,
                logger.Object,
                null,  // IAuthenticationSchemeProvider
                null); // IUserConfirmation<AppUser>

            _tokenServiceMock = new Mock<IToken>();
            _backgroundServiMock = new Mock<BackgroundServi>(_dbContext);
            _rabbitProducerMock = new Mock<IRabbitProducer>();

            // Controller'ı başlatıyoruz
            _controller = new AccountController(_userManagerMock.Object, _tokenServiceMock.Object, _signInManagerMock.Object, _backgroundServiMock.Object, _rabbitProducerMock.Object);
        }

        [Fact]
        public async Task AccountController_LoginPost_ResultOk()
        {
            // Arrange
            var loginDto = new LoginDto { UserName = "testuser", Password = "password" };
            var user = new AppUser { UserName = "testuser", Email = "test@gmail.com", Id = "1" };

            // Kullanıcıyı veritabanına ekle
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync(); //VERİTABANINA KAYDET

            _userManagerMock.Setup(um => um.Users).Returns(_dbContext.Users);

            var signInResult = Microsoft.AspNetCore.Identity.SignInResult.Success;
            _signInManagerMock.Setup(sm => sm.CheckPasswordSignInAsync(It.IsAny<AppUser>(), loginDto.Password, false))
                .ReturnsAsync(signInResult);

            _tokenServiceMock.Setup(ts => ts.CreateToken(user)).Returns(new ResponseModel<string> { Data = "test-token" }); // TOKEN OLUSTUR


            // Act
            var result = await _controller.Login(loginDto); //TESTİ ÇALIŞTIRMAK 

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Which; //ÇIKAN SONUÇ İSTEDİĞİMLE UYUŞUYOR MU DOĞRULAMA
            var newUserDto = okResult.Value.Should().BeOfType<NewUserDto>().Which;
            newUserDto.Token.Should().Be("test-token");
        }

        [Fact]
        public async Task AccountController_RegisterPost_ResultOk()     
        {
            // Arrange
            var registerDto = new RegisterDto { UserName = "newuser", Email = "new@gmail.com", Password = "password", Role = "Admin" };
            var appUser = new AppUser { UserName = registerDto.UserName, Email = registerDto.Email };

            _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<AppUser>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(um => um.AddToRoleAsync(It.IsAny<AppUser>(), registerDto.Role))
                .ReturnsAsync(IdentityResult.Success);
            _tokenServiceMock.Setup(ts => ts.CreateToken(It.IsAny<AppUser>())).Returns(new ResponseModel<string> { Data = "new-user-token" });


            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Which;
            var newUserDto = okResult.Value.Should().BeOfType<NewUserDto>().Which;
            newUserDto.Token.Should().Be("new-user-token");
        }

        [Fact]
        public async Task AccountController_LoginPost_ResultBadRequest()
        {
            // Arrange
            var loginDto = new LoginDto { UserName = "User", Password = "password" };

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task AccountController_RegisterPost_ResultBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                UserName = null,
                Email = null,
                Password = "Password123!",
                Role = "User"
            };


            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
           
        }
   
    }
}
