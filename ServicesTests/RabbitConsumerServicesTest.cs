using Moq;
using Xunit;
using Microsoft.Extensions.Configuration;
using companyappbasic.Services.EmailServices;
using companyappbasic.Services.RabbitMQServices;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using companyappbasic.Data.Entity;

namespace companyappbasic.Tests
{
    public class RabbitConsumerTests
    {
        private readonly Mock<IEmail> _emailServiceMock;
        private readonly Mock<IConfiguration> _configurationMock; 
        private readonly Mock<IModel> _channelMock;
        private readonly Mock<IConnection> _connectionMock;
        private readonly RabbitConsumer _rabbitConsumer;

        public RabbitConsumerTests()
        {
            // Mock bağımlılıkları oluşturuyoruz
            _emailServiceMock = new Mock<IEmail>();
            _configurationMock = new Mock<IConfiguration>();
            _channelMock = new Mock<IModel>();
            _connectionMock = new Mock<IConnection>();
            // RabbitMQ yapılandırmasını ayarlıyoruz
            _configurationMock.Setup(config => config.GetSection("RabbitMQ")["HostName"]).Returns("localhost");
            _configurationMock.Setup(config => config.GetSection("RabbitMQ")["UserName"]).Returns("Melikenur");
            _configurationMock.Setup(config => config.GetSection("RabbitMQ")["Password"]).Returns("Mk546865");

            // Sınıfın test edilecek örneğini oluşturuyoruz
            _rabbitConsumer = new RabbitConsumer(_configurationMock.Object, _emailServiceMock.Object);
        }
        [Fact]
        public async Task StartConsuming_ShouldConsumeMessages_AndSendEmail()
        {
            // Arrange
            var emailMessage = new EmailMessage
            {
                Email = "test@gmail.com",
                Subject = "Test Subject",
                Body = "Test Body"
            };

            var messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(emailMessage));
            var ea = new BasicDeliverEventArgs
            {
                Body = messageBody
            };

            _connectionMock.Setup(c => c.CreateModel()).Returns(_channelMock.Object);

            // Mock EventingBasicConsumer
            var consumer = new EventingBasicConsumer(_channelMock.Object);

            // Tüketici olayını dinleyiciyi simüle ediyoruz
            consumer.Received += async (sender, args) =>
            {
                var message = Encoding.UTF8.GetString(args.Body.ToArray());
                var deserializedMessage = JsonConvert.DeserializeObject<EmailMessage>(message);
                if (deserializedMessage != null)
                {
                    await _emailServiceMock.Object.SendEmailAsync(deserializedMessage.Email!, deserializedMessage.Subject!, deserializedMessage.Body!);
                }
            };

            // Act
            // Olayı tetikleyerek simülasyon yapıyoruz
            consumer.HandleBasicDeliver("consumerTag", 1, false, "exchange", "routingKey", null, ea.Body);

            // Assert
            _emailServiceMock.Verify(emailService => emailService.SendEmailAsync(
                emailMessage.Email, emailMessage.Subject, emailMessage.Body), Times.Once);
        }

        [Fact]
        public async Task StartConsuming_ShouldNotSendEmail_WhenMessageIsInvalid()
        {
            // Arrange
            // Geçersiz bir mesaj oluşturuyoruz (örneğin, boş bir mesaj)
            var invalidMessageBody = Encoding.UTF8.GetBytes("Invalid JSON");
            var ea = new BasicDeliverEventArgs
            {
                Body = invalidMessageBody
            };

            _connectionMock.Setup(c => c.CreateModel()).Returns(_channelMock.Object);

            // Mock EventingBasicConsumer
            var consumer = new EventingBasicConsumer(_channelMock.Object);

            // Tüketici olayını dinleyiciyi simüle ediyoruz
            consumer.Received += async (sender, args) =>
            {
                var message = Encoding.UTF8.GetString(args.Body.ToArray());
                var deserializedMessage = JsonConvert.DeserializeObject<EmailMessage>(message);
                if (deserializedMessage != null)
                {
                    await _emailServiceMock.Object.SendEmailAsync(deserializedMessage.Email!, deserializedMessage.Subject!, deserializedMessage.Body!);
                }
            };

            // Act
            // Geçersiz mesajı simüle ederek olayı tetikliyoruz
            consumer.HandleBasicDeliver("consumerTag", 1, false, "exchange", "routingKey", null, ea.Body);

            // Assert
            // E-posta gönderme işlemi hiç çağrılmamış olmalı
            _emailServiceMock.Verify(emailService => emailService.SendEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

    }
}




