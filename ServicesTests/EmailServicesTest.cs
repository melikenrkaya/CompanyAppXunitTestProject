using companyappbasic.Data.Entity;
using companyappbasic.Services.AdminServices;
using companyappbasic.Services.EmailServices;
using companyappbasic.Services.SmtpServices;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;


namespace CompanyAppTestProject.ServicesTests
{
    public class EmailServicesTest
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ISmtpClientWrapper> _mockSmtpClientWrapper;
        private readonly EmailServi _emailServi;
        private readonly ResponseModel<string> _response;
        public EmailServicesTest()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockSmtpClientWrapper = new Mock<ISmtpClientWrapper>();
            _response = new ResponseModel<string>();

            // `Smtp:FromEmail` anahtarını yapılandırın
            _mockConfiguration.Setup(config => config["Smtp:FromEmail"]).Returns("melike.kaya603@gmail.com");

            _emailServi = new EmailServi(_mockConfiguration.Object, _response, _mockSmtpClientWrapper.Object);
        }

        [Fact]
        public async Task EmailServi_SendEmailAsync_ShouldSendEmail()
        {
            // Arrange
            string email = "melike.kaya603@gmail.com";
            string subject = "Test Subject";
            string body = "<h1>Test Body</h1>";


            _mockSmtpClientWrapper.Setup(smtp => smtp.SendMailAsync(It.IsAny<MailMessage>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _emailServi.SendEmailAsync(email, subject, body);

            // Assert
            _mockSmtpClientWrapper.Verify(smtp => smtp.SendMailAsync(It.IsAny<MailMessage>()), Times.Once);
            _response.Success.Should().BeTrue();
            _response.Message.Should().Be("Email gönderimi başarılı.");
        }

        [Fact]
        public async Task EmailServi_SendEmailAsync_ShouldnotSendEmail()
        {
            // Arrange
            string email = "melike.kaya603@gmail.com";          
            string subject = "Test Subject";
            string body = "<h1>Test Body</h1>";

            _mockSmtpClientWrapper.Setup(smtp => smtp.SendMailAsync(It.IsAny<MailMessage>()))
                           .ThrowsAsync(new SmtpException("SMTP error"));

            // Act
            await _emailServi.SendEmailAsync(email, subject, body);

            // Assert
            _response.Success.Should().BeFalse();
            _response.Message.Should().Contain("SMTP error");
        }
    
    }
}
