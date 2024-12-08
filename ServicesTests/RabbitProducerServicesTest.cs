using NSubstitute;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using companyappbasic.Services.RabbitMQServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CompanyAppTestProject.ServicesTests
{
    public class RabbitProducerTests
    {
        private readonly IConfiguration _mockConfiguration;
        private readonly IModel _mockChannel;
        private readonly IConnection _mockConnection;
        private readonly ILogger<RabbitProducer> _mockLogger;

        private readonly RabbitProducer _rabbitProducer;

        public RabbitProducerTests()
        {
            // NSubstitute ile mock'ları başlat
            _mockConfiguration = Substitute.For<IConfiguration>();
            _mockChannel = Substitute.For<IModel>();
            _mockConnection = Substitute.For<IConnection>();
            _mockLogger=Substitute.For<ILogger<RabbitProducer>>();

            // RabbitMQ ile ilgili mock'ları yapılandır
            _mockConfiguration["RabbitMQ:HostName"].Returns("localhost");
            _mockConfiguration["RabbitMQ:UserName"].Returns("Melikenur");
            _mockConfiguration["RabbitMQ:Password"].Returns("Mk546865");

            _mockConnection.CreateModel().Returns(_mockChannel);

            // RabbitProducer sınıfını başlat
            _rabbitProducer = new RabbitProducer(_mockConfiguration);
        }

        [Fact]
        public async Task SendMessageAsync_ShouldPublishMessageToQueue()
        {
            // Arrange
            var email = "test@example.com";
            var subject = "Test Subject";
            var body = "Test Body";

            // StringWriter ile Console çıktısını yakala
            var successStringWriter = new System.IO.StringWriter();
            Console.SetOut(successStringWriter); // Console.WriteLine çıktısını StringWriter'a yönlendir

            // Act
            await _rabbitProducer.SendMessageAsync(email, subject, body);

            // Assert

            //_mockChannel.Received(1).BasicPublish(
            //Arg.Any<string>(),
            //Arg.Any<string>(),
            //Arg.Any<IBasicProperties>(),
            //Arg.Any<ReadOnlyMemory<byte>>());

            _mockChannel.When(x => x.BasicPublish(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<IBasicProperties>(),
                Arg.Any<ReadOnlyMemory<byte>>()))
              .Do(x => { /* Burada loglama veya test amaçlı bir işlem yapabilirsiniz. */ });


            // Ayrıca, console'a yazdırılan mesajı kontrol ediyoruz
            successStringWriter.ToString().Should().Contain("Mesaj kuyruğa gönderildi.");

        }

        [Fact]
        public async Task SendMessageAsync_ShouldHandleException()
        {
            // Arrange
            var email = "test@example.com";
            var subject = "Test Subject";
            var body = "Test Body";

            var errorStringWriter = new System.IO.StringWriter();
            Console.SetOut(errorStringWriter);

            // RabbitMQ bağlantı ve kanal hatası simüle et
            _mockChannel
           .When(x => x.BasicPublish(
               Arg.Any<string>(),
               Arg.Any<string>(),
               Arg.Any<IBasicProperties>(),
               Arg.Any<ReadOnlyMemory<byte>>()))
           .Throw(new Exception("None of the specified endpoints were reachable"));

            // Act
            await _rabbitProducer.SendMessageAsync(email, subject, body);
          

            // Assert
            // Hata mesajının "Mesaj gönderme hatası" içermesi gerekiyor
            string output = errorStringWriter.ToString();
            output.Should().Contain("Mesaj gönderme hatası");
            output.Should().Contain("None of the specified endpoints were reachable");
        }
            

    }
}

