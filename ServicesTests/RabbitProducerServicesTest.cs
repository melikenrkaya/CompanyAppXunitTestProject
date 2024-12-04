//using Moq;
//using Newtonsoft.Json;
//using RabbitMQ.Client;
//using System;
//using System.Text;
//using System.Threading.Tasks;
//using Xunit;
//using Microsoft.Extensions.Configuration;
//using companyappbasic.Services.RabbitMQServices;

//namespace companyappbasic.Tests.RabbitProducerTests
//{
//    public class RabbitProducerTests
//    {
//        [Fact]
//        public async Task SendMessageAsync_ShouldSendMessageToRabbitMQ_WhenValidDataIsProvided()
//        {
//            // Arrange
//            var mockConfiguration = new Mock<IConfiguration>();
//            mockConfiguration.Setup(config => config["RabbitMQ:HostName"]).Returns("localhost");
//            mockConfiguration.Setup(config => config["RabbitMQ:UserName"]).Returns("user");
//            mockConfiguration.Setup(config => config["RabbitMQ:Password"]).Returns("password");

//            var mockConnection = new Mock<IConnection>();
//            var mockChannel = new Mock<IModel>();

//            // Mock the factory to return the mocked connection
//            var mockConnectionFactory = new Mock<ConnectionFactory>();
//            mockConnectionFactory.Setup(factory => factory.CreateConnection()).Returns(mockConnection.Object);
//            mockConnection.Setup(conn => conn.CreateModel()).Returns(mockChannel.Object);

//            var producer = new RabbitProducer(mockConfiguration.Object)
//            {
//                // Override the factory to use the mocked version
//                Factory = mockConnectionFactory.Object
//            };

//            string email = "test@example.com";
//            string subject = "Test Subject";
//            string body = "Test Body";

//            // Act
//            await producer.SendMessageAsync(email, subject, body);

//            // Assert
//            // Verify that BasicPublish was called on the channel with the correct parameters
//            mockChannel.Verify(channel => channel.BasicPublish(
//                It.IsAny<string>(),
//                "mailatma", // routingKey
//                It.IsAny<IBasicProperties>(),
//                It.Is<byte[]>(bodyBytes => Encoding.UTF8.GetString(bodyBytes) == JsonConvert.SerializeObject(new { email, subject, body }))),
//                Times.Once);

//            // Verify that QueueDeclare was called with the correct parameters
//            mockChannel.Verify(channel => channel.QueueDeclare(
//                "mailatma",
//                It.IsAny<bool>(),
//                It.IsAny<bool>(),
//                It.IsAny<bool>(),
//                It.IsAny<object>()),
//                Times.Once);
//        }

//        [Fact]
//        public async Task SendMessageAsync_ShouldHandleException_WhenRabbitMQFails()
//        {
//            // Arrange
//            var mockConfiguration = new Mock<IConfiguration>();
//            mockConfiguration.Setup(config => config["RabbitMQ:HostName"]).Returns("localhost");
//            mockConfiguration.Setup(config => config["RabbitMQ:UserName"]).Returns("user");
//            mockConfiguration.Setup(config => config["RabbitMQ:Password"]).Returns("password");

//            var mockConnectionFactory = new Mock<ConnectionFactory>();
//            mockConnectionFactory.Setup(factory => factory.CreateConnection()).Throws(new Exception("RabbitMQ connection failed"));

//            var producer = new RabbitProducer(mockConfiguration.Object)
//            {
//                Factory = mockConnectionFactory.Object
//            };

//            // Act
//            await producer.SendMessageAsync("test@example.com", "Subject", "Body");

//            // Assert
//            // We can verify that no calls were made to the channel because the connection failed
//            mockConnectionFactory.Verify(factory => factory.CreateConnection(), Times.Once);
//        }
//    }
//}


//using Moq;
//using Xunit;
//using Microsoft.Extensions.Configuration;
//using companyappbasic.Services.RabbitMQServices;
//using RabbitMQ.Client;
//using System.Text;
//using Newtonsoft.Json;

//namespace companyappbasic.Tests
//{
//    public class RabbitProducerTests
//    {
//        private readonly Mock<IConfiguration> _configurationMock;
//        private readonly Mock<IConnection> _connectionMock;
//        private readonly Mock<IModel> _channelMock;
//        private readonly RabbitProducer _rabbitProducer;

//        public RabbitProducerTests()
//        {
//            // Mock bağımlılıklarını oluşturuyoruz
//            _configurationMock = new Mock<IConfiguration>();
//            _connectionMock = new Mock<IConnection>();
//            _channelMock = new Mock<IModel>();

//            // IConfiguration mock'ını ayarlıyoruz
//            _configurationMock.Setup(config => config["RabbitMQ:HostName"]).Returns("localhost");
//            _configurationMock.Setup(config => config["RabbitMQ:UserName"]).Returns("Melikenur");
//            _configurationMock.Setup(config => config["RabbitMQ:Password"]).Returns("Mk546865");

//            // IConnection ve IModel mock'larını ayarlıyoruz
//            _connectionMock.Setup(c => c.CreateModel()).Returns(_channelMock.Object);

//            // RabbitProducer sınıfının örneğini oluşturuyoruz
//            _rabbitProducer = new RabbitProducer(_configurationMock.Object);
//        }

//        [Fact]
//        public async Task SendMessageAsync_ShouldSendMessageToQueue()
//        {
//            // Arrange
//            string email = "test@example.com";
//            string subject = "Test Subject";
//            string body = "Test Body";

//            var messageObject = new
//            {
//                email,
//                subject,
//                body
//            };

//            var expectedMessage = JsonConvert.SerializeObject(messageObject);
//            var expectedBodyBytes = Encoding.UTF8.GetBytes(expectedMessage);

//            // Act
//            await _rabbitProducer.SendMessageAsync(email, subject, body);

//            // Assert
//            // BasicPublish metodunun doğru parametrelerle çağrıldığını doğruluyoruz
//            _channelMock.Verify(channel =>
//                channel.BasicPublish(
//                     "",
//                     "mailatma",
//                     null,
//                     expectedBodyBytes), Times.Once);
//        }


//        [Fact]
//        public async Task SendMessageAsync_ShouldHandleException_WhenRabbitMQFails()
//        {
//            // Arrange
//            string email = "test@example.com";
//            string subject = "Test Subject";
//            string body = "Test Body";

//            // RabbitMQ bağlantısının başarısız olmasını simüle ediyoruz
//            _connectionMock.Setup(c => c.CreateModel()).Throws(new System.Exception("RabbitMQ Connection failed"));

//            // Act
//            await _rabbitProducer.SendMessageAsync(email, subject, body);

//            // Assert
//            // Exception'ı yakalayıp uygun hata mesajını verecek şekilde doğrulama yapıyoruz
//            _connectionMock.Verify(conn => conn.CreateModel(), Times.Once, "RabbitMQ bağlantısı oluşturulurken bir hata oluştu.");
//        }
//        [Fact]
//        public async Task SendMessageAsync_ShouldSendMessageToQueue_WhenMessageIsValid()
//        {
//            // Arrange
//            string email = "test@example.com";
//            string subject = "Test Subject";
//            string body = "Test Body";

//            // Mock RabbitMQ connection and channel (IConnection ve IModel nesnelerini mock'lıyoruz)
//            var mockConnection = new Mock<IConnection>();
//            var mockChannel = new Mock<IModel>();

//            // Create a mock for the connection's CreateModel method
//            mockConnection.Setup(conn => conn.CreateModel()).Returns(mockChannel.Object);

//            // Setup the mock channel to ensure BasicPublish is called
//            mockChannel.Setup(channel => channel.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IBasicProperties>(), It.IsAny<byte[]>()))
//                       .Verifiable(); // Verifiable, çağrıldığını doğrulamak için

//            // Instantiate RabbitProducer with the mock connection
//            var rabbitProducer = new RabbitProducer(new Mock<IConfiguration>().Object); // Configuration mock'lanabilir veya geçici olarak kullanılabilir

//            // Act
//            // Send the message (RabbitProducer içerisindeki SendMessageAsync metodu çalıştırılıyor)
//            await rabbitProducer.SendMessageAsync(email, subject, body);

//            // Assert
//            // Ensure the BasicPublish method was called exactly once (BasicPublish metodunun bir kez çağrıldığını doğruluyoruz)
//            mockChannel.Verify(channel => channel.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IBasicProperties>(), It.IsAny<byte[]>()), Times.Once);
//        }
//        [Fact]
//        public async Task SendMessageAsync2_ShouldSendMessageToQueue_WhenMessageIsValid()
//        {
//            // Arrange
//            string email = "test@example.com";
//            string subject = "Test Subject";
//            string body = "Test Body";

//            // Mock RabbitMQ connection and channel (IConnection ve IModel nesnelerini mock'lıyoruz)
//            var mockConnection = new Mock<IConnection>();
//            var mockChannel = new Mock<IModel>();

//            // Create a mock for the connection's CreateModel method
//            mockConnection.Setup(conn => conn.CreateModel()).Returns(mockChannel.Object);

//            // Instantiate RabbitProducer with the mock connection
//            var rabbitProducer = new RabbitProducer(new Mock<IConfiguration>().Object); // Configuration mock'lanabilir veya geçici olarak kullanılabilir

//            // Act
//            // Send the message (RabbitProducer içerisindeki SendMessageAsync metodu çalıştırılıyor)
//            await rabbitProducer.SendMessageAsync(email, subject, body);

//            // Assert
//            // Ensure the BasicPublish method was called exactly once (BasicPublish metodunun bir kez çağrıldığını doğruluyoruz)
//            mockChannel.Verify(channel => channel.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IBasicProperties>(), It.IsAny<byte[]>()), Times.Once);
//        }



//    }
//}


//using Moq;
//using Xunit;
//using RabbitMQ.Client;
//using Microsoft.Extensions.Configuration;
//using companyappbasic.Services.RabbitMQServices;
//using System.Text;

//namespace companyappbasic.Tests
//{
//    public class RabbitProducerTests
//    {
//        private readonly Mock<IConfiguration> _configurationMock;
//        private readonly Mock<IConnection> _connectionMock;
//        private readonly Mock<IModel> _channelMock;
//        private readonly RabbitProducer _rabbitProducer;

//        public RabbitProducerTests()
//        {
//            // Mock bağımlılıkları oluşturuyoruz
//            _configurationMock = new Mock<IConfiguration>();
//            _connectionMock = new Mock<IConnection>();
//            _channelMock = new Mock<IModel>();

//            // RabbitMQ yapılandırmasını ayarlıyoruZ
//            _configurationMock.Setup(config => config.GetSection("RabbitMQ")["HostName"]).Returns("localhost");
//            _configurationMock.Setup(config => config.GetSection("RabbitMQ")["UserName"]).Returns("Melikenur");
//            _configurationMock.Setup(config => config.GetSection("RabbitMQ")["Password"]).Returns("Mk546865");

//            // RabbitMQ bağlantısını ve kanalını ayarlıyoruz
//            _connectionMock.Setup(conn => conn.CreateModel()).Returns(_channelMock.Object);

//            // RabbitProducer sınıfını oluşturuyoruz
//            _rabbitProducer = new RabbitProducer(_configurationMock.Object);
//        }

//        [Fact]
//        public async Task SendMessageAsync_ShouldSendMessageToQueue()
//        {
//            // Arrange
//            var email = "test@example.com";
//            var subject = "Test Subject";
//            var body = "Test Body";

//            // `BasicPublish`'i doğrudan mocklamak yerine Callback kullanarak işleme yapıyoruz
//            _channelMock.Setup(channel => channel.BasicPublish(
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<IBasicProperties>(),
//                It.IsAny<byte[]>()
//            )).Callback<string, string, IBasicProperties, byte[]>((exchange, routingKey, properties, body) =>
//            {
//                // Burada BasicPublish çağrıldığında yapılacak işlemleri kontrol edebilirsiniz
//                Assert.Equal("mailatma", routingKey);
//                Assert.NotNull(body); // Mesajın boş olmadığını kontrol et
//            });

//            // Act
//            await _rabbitProducer.SendMessageAsync(email, subject, body);

//            // Assert
//            _channelMock.Verify(channel => channel.BasicPublish(
//                "", "mailatma", null, body), Times.Once);


















// Arrange
//var email = "test@example.com";
// var subject = "Test Subject";
// var body = "Test Body";

// Action mock
// _channelMock.Setup(channel => channel.BasicPublish(
//     It.IsAny<string>(),
//     It.IsAny<string>(),
//     It.IsAny<IBasicProperties>(),
//     It.IsAny<byte[]>()
// )).Callback<string, string, IBasicProperties, byte[]>((exchange, routingKey, properties, body) =>
// {
//     Burada BasicPublish çağrıldığında yapılacak işlemleri kontrol edebilirsiniz
//     Assert.Equal("mailatma", routingKey);
// });

// Act
//await _rabbitProducer.SendMessageAsync(email, subject, body);

// Assert
// _channelMock.Verify(channel => channel.BasicPublish(
//     "", "mailatma", null, It.IsAny<byte[]>()), Times.Once);











//// QueueDeclare metodunun çağrıldığını kontrol etmek için ayarlıyoruz
//_channelMock.Setup(channel => channel.QueueDeclare("mailatma", false, false, false, null));

//// BasicPublish metodunun çağrıldığını kontrol etmek için ayarlıyoruz
//_channelMock.Setup(channel => channel.BasicPublish(
//    "",
//    "mailatma",
//    null,
//    It.IsAny<byte[]>()
//));

//// Act
//await _rabbitProducer.SendMessageAsync(email, subject, body);

//// Assert
//_channelMock.Verify(channel => channel.QueueDeclare("mailatma", false, false, false, null), Times.Once);
//_channelMock.Verify(channel => channel.BasicPublish(
//     "",
//   "mailatma",
//    null,
//    It.Is<byte[]>(b => Encoding.UTF8.GetString(b).Contains(email) &&
//                 Encoding.UTF8.GetString(b).Contains(subject) &&
//                 Encoding.UTF8.GetString(b).Contains(body))
//), Times.Once);
//        }
//    }
//}


//using Moq;
//using Newtonsoft.Json;
//using RabbitMQ.Client;
//using System.Text;
//using System.Threading.Tasks;
//using Xunit;
//using Microsoft.Extensions.Configuration;
//using companyappbasic.Services.RabbitMQServices;

//namespace companyappbasic.Tests.Services
//{
//    public class RabbitProducerTests
//    {
//        private readonly Mock<IConfiguration> _configurationMock;
//        private readonly Mock<IConnection> _connectionMock;
//        private readonly Mock<IModel> _channelMock;
//        private readonly RabbitProducer _rabbitProducer;

//        public RabbitProducerTests()
//        {
//            // Configuration mock'larını oluşturuyoruz
//            _configurationMock = new Mock<IConfiguration>();
//            _configurationMock.Setup(c => c["RabbitMQ:HostName"]).Returns("localhost");
//            _configurationMock.Setup(c => c["RabbitMQ:UserName"]).Returns("guest");
//            _configurationMock.Setup(c => c["RabbitMQ:Password"]).Returns("guest");

//            // RabbitMQ connection ve channel mock'larını oluşturuyoruz
//            _connectionMock = new Mock<IConnection>();
//            _channelMock = new Mock<IModel>();

//            // Connection ve channel için metodları mock'lıyoruz
//            _connectionMock.Setup(c => c.CreateModel()).Returns(_channelMock.Object);

//            // RabbitProducer sınıfını başlatıyoruz
//            _rabbitProducer = new RabbitProducer(_configurationMock.Object);
//        }

//        [Fact]
//        public async Task SendMessageAsync_ShouldPublishMessageToQueue()
//        {
//            // Arrange
//            var email = "test@example.com";
//            var subject = "Test Subject";
//            var body = "Test Body";
//            var queueName = "mailatma";

//            var messageObject = new { email, subject, body };
//            var message = JsonConvert.SerializeObject(messageObject);
//            var bodyBytes = Encoding.UTF8.GetBytes(message);

//            // Act
//            await _rabbitProducer.SendMessageAsync(email, subject, body);

//            // Assert - Verify if BasicPublish was called with the correct parameters
//            _channelMock.Verify(c =>
//                c.BasicPublish(It.IsAny<string>(),
//                               queueName, // expected routingKey
//                               It.IsAny<IBasicProperties>(),
//                               It.Is<byte[]>(b => b.SequenceEqual(bodyBytes))), // matching byte array
//                Times.Once); // verify it was called exactly once
//        }
//    }
//}

//using Moq;
//using Newtonsoft.Json;
//using RabbitMQ.Client;
//using System.Text;
//using System.Threading.Tasks;
//using Xunit;
//using Microsoft.Extensions.Configuration;
//using companyappbasic.Services.RabbitMQServices;

//namespace companyappbasic.Tests.Services
//{
//    public class RabbitProducerTests
//    {
//        private readonly Mock<IConfiguration> _configurationMock;
//        private readonly Mock<IConnection> _connectionMock;
//        private readonly Mock<IModel> _channelMock;
//        private readonly RabbitProducer _rabbitProducer;

//        public RabbitProducerTests()
//        {
//            // Configuration mock'larını oluşturuyoruz
//            _configurationMock = new Mock<IConfiguration>();
//            _configurationMock.Setup(c => c["RabbitMQ:HostName"]).Returns("localhost");
//            _configurationMock.Setup(c => c["RabbitMQ:UserName"]).Returns("guest");
//            _configurationMock.Setup(c => c["RabbitMQ:Password"]).Returns("guest");

//            // RabbitMQ connection ve channel mock'larını oluşturuyoruz
//            _connectionMock = new Mock<IConnection>();
//            _channelMock = new Mock<IModel>();

//            // Connection ve channel için metodları mock'lıyoruz
//            _connectionMock.Setup(c => c.CreateModel()).Returns(_channelMock.Object);

//            // RabbitProducer sınıfını başlatıyoruz
//            _rabbitProducer = new RabbitProducer(_configurationMock.Object);
//        }

//        [Fact]
//        public async Task SendMessageAsync_ShouldPublishMessageToQueue()
//        {
//            // Arrange
//            var email = "test@example.com";
//            var subject = "Test Subject";
//            var body = "Test Body";
//            var queueName = "mailatma";

//            var messageObject = new { email, subject, body };
//            var message = JsonConvert.SerializeObject(messageObject);
//            var bodyBytes = Encoding.UTF8.GetBytes(message);

//            // BasicPublish metodunun çalışmasını doğrulamak için callback kullanıyoruz
//            _channelMock
//                .Setup(c => c.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IBasicProperties>(), It.IsAny<byte[]>()))
//                .Callback<string, string, IBasicProperties, byte[]>((exchange, routingKey, properties, bodyData) =>
//                {
//                    // Parametrelerin doğruluğunu kontrol ediyoruz
//                    Assert.Equal(queueName, routingKey);
//                    Assert.Equal(bodyBytes, bodyData);
//                });

//            // Act
//            await _rabbitProducer.SendMessageAsync(email, subject, body);

//            // Assert
//            _channelMock.Verify(c => c.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IBasicProperties>(), It.IsAny<byte[]>()), Times.Once);
//        }
//    }
//}


//using Moq;
//using Newtonsoft.Json;
//using RabbitMQ.Client;
//using System.Text;
//using System.Threading.Tasks;
//using Xunit;
//using Microsoft.Extensions.Configuration;
//using companyappbasic.Services.RabbitMQServices;

//namespace companyappbasic.Tests.Services
//{
//    public class RabbitProducerTests
//    {
//        private readonly Mock<IConfiguration> _configurationMock;
//        private readonly Mock<IConnection> _connectionMock;
//        private readonly Mock<IModel> _channelMock;
//        private readonly RabbitProducer _rabbitProducer;

//        public RabbitProducerTests()
//        {
//            // Configuration mock'larını oluşturuyoruz
//            _configurationMock = new Mock<IConfiguration>();
//            _configurationMock.Setup(c => c["RabbitMQ:HostName"]).Returns("localhost");
//            _configurationMock.Setup(c => c["RabbitMQ:UserName"]).Returns("guest");
//            _configurationMock.Setup(c => c["RabbitMQ:Password"]).Returns("guest");

//            // RabbitMQ connection ve channel mock'larını oluşturuyoruz
//            _connectionMock = new Mock<IConnection>();
//            _channelMock = new Mock<IModel>();

//            // Connection ve channel için metodları mock'lıyoruz
//            _connectionMock.Setup(c => c.CreateModel()).Returns(_channelMock.Object);

//            // RabbitProducer sınıfını başlatıyoruz
//            _rabbitProducer = new RabbitProducer(_configurationMock.Object);
//        }

//        [Fact]
//        public async Task SendMessageAsync_ShouldPublishMessageToQueue()
//        {
//            // Arrange
//            var email = "test@example.com";
//            var subject = "Test Subject";
//            var body = "Test Body";
//            var queueName = "mailatma";

//            var messageObject = new { email, subject, body };
//            var message = JsonConvert.SerializeObject(messageObject);
//            var bodyBytes = Encoding.UTF8.GetBytes(message);

//            // BasicPublish metodunun çağrılmasını doğrulamak için callback kullanıyoruz
//            _channelMock
//                .Setup(c => c.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IBasicProperties>(), It.IsAny<byte[]>()))
//                .Callback<string, string, IBasicProperties, byte[]>((exchange, routingKey, properties, bodyData) =>
//                {
//                    // Burada istediğiniz doğrulamayı yapabilirsiniz
//                    Assert.Equal(queueName, routingKey);
//                    Assert.Equal(bodyBytes, bodyData);
//                });

//            // Act
//            await _rabbitProducer.SendMessageAsync(email, subject, body);

//            // Assert
//            _channelMock.Verify(c => c.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IBasicProperties>(), It.IsAny<byte[]>()), Times.Once);
//        }
//    }
//}


//using Moq;
//using Newtonsoft.Json;
//using RabbitMQ.Client;
//using System.Text;
//using System.Threading.Tasks;
//using Xunit;
//using Microsoft.Extensions.Configuration;
//using companyappbasic.Services.RabbitMQServices;

//namespace companyappbasic.Tests.Services
//{
//    public class RabbitProducerTests
//    {
//        private readonly Mock<IConfiguration> _configurationMock;
//        private readonly Mock<IConnection> _connectionMock;
//        private readonly Mock<IModel> _channelMock;
//        private readonly RabbitProducer _rabbitProducer;

//        public RabbitProducerTests()
//        {
//            // Configuration mock'larını oluşturuyoruz
//            _configurationMock = new Mock<IConfiguration>();
//            _configurationMock.Setup(c => c["RabbitMQ:HostName"]).Returns("localhost");
//            _configurationMock.Setup(c => c["RabbitMQ:UserName"]).Returns("guest");
//            _configurationMock.Setup(c => c["RabbitMQ:Password"]).Returns("guest");

//            // RabbitMQ connection ve channel mock'larını oluşturuyoruz
//            _connectionMock = new Mock<IConnection>();
//            _channelMock = new Mock<IModel>();

//            // Connection ve channel için metodları mock'lıyoruz
//            _connectionMock.Setup(c => c.CreateModel()).Returns(_channelMock.Object);

//            // RabbitProducer sınıfını başlatıyoruz
//            _rabbitProducer = new RabbitProducer(_configurationMock.Object);
//        }

//        [Fact]
//        public async Task SendMessageAsync_ShouldPublishMessageToQueue()
//        {
//            // Arrange
//            var email = "test@example.com";
//            var subject = "Test Subject";
//            var body = "Test Body";
//            var queueName = "mailatma";

//            var messageObject = new { email, subject, body };
//            var message = JsonConvert.SerializeObject(messageObject);
//            var bodyBytes = Encoding.UTF8.GetBytes(message);

//            // Channel'dan BasicPublish metodunu mock'lıyoruz
//            _channelMock.Setup(c => c.BasicPublish(
//                It.IsAny<string>(),  // exchange
//                It.IsAny<string>(),  // routingKey
//                It.IsAny<IBasicProperties>(),  // basicProperties
//                It.IsAny<byte[]>()))  // body
//            .Verifiable();  // Bu metodun çağrıldığını doğrulamak için

//            // Act
//            await _rabbitProducer.SendMessageAsync(email, subject, body);

//            // Assert
//            // BasicPublish metodunun çağrılıp çağrılmadığını doğruluyoruz
//            _channelMock.Verify(c => c.BasicPublish(
//                "",  // exchange (boş bıraktık çünkü bu örnekte boş)
//                queueName,  // routingKey
//                null,  // basicProperties (bu test için null)
//                bodyBytes), Times.Once);  // Mesajın doğru şekilde gönderildiğini doğruluyoruz
//        }
//    }
//}
//using Moq;
//using Xunit;
//using Microsoft.Extensions.Configuration;
//using RabbitMQ.Client;
//using System.Text;
//using System.Threading.Tasks;
//using companyappbasic.Services.RabbitMQServices;

//namespace companyappbasic.Tests
//{
//    public class RabbitProducerTests
//    {
//        private readonly Mock<IConfiguration> _configurationMock;
//        private readonly Mock<IConnection> _connectionMock;
//        private readonly Mock<IModel> _channelMock;

//        public RabbitProducerTests()
//        {
//            // Mock bağımlılıkları oluşturuyoruz
//            _configurationMock = new Mock<IConfiguration>();
//            _connectionMock = new Mock<IConnection>();
//            _channelMock = new Mock<IModel>();

//            // RabbitMQ yapılandırmasını ayarlıyoruz
//            _configurationMock.Setup(config => config["RabbitMQ:HostName"]).Returns("localhost");
//            _configurationMock.Setup(config => config["RabbitMQ:UserName"]).Returns("guest");
//            _configurationMock.Setup(config => config["RabbitMQ:Password"]).Returns("guest");

//            // Mock bağlantı ve kanal ayarlarını yapılandırıyoruz
//            _connectionMock.Setup(conn => conn.CreateModel()).Returns(_channelMock.Object);
//        }

//        [Fact]
//        public async Task SendMessageAsync_ShouldPublishMessageToQueue()
//        {
//            // Arrange
//            var email = "test@gmail.com";
//            var subject = "Test Subject";
//            var body = "Test Body";

//            var producer = new RabbitProducer(_configurationMock.Object);

//            var factoryMock = new Mock<ConnectionFactory>();
//            factoryMock.Setup(f => f.CreateConnection()).Returns(_connectionMock.Object);

//            // Kanalın BasicPublish çağrısını izlemek için ayar yapıyoruz
//            _channelMock.Setup(ch => ch.BasicPublish(
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<IBasicProperties>(),
//                It.Is<byte[]>(b =>
//                {
//                    var json = Encoding.UTF8.GetString(b);
//                    return json.Contains(email) && json.Contains(subject) && json.Contains(body);
//                }))).Verifiable();
//            // Act
//            await producer.SendMessageAsync(email, subject, body);

//            // Assert
//            _channelMock.Verify(ch => ch.BasicPublish(
//              It.Is<string>(exchange => exchange == ""),
//              It.Is<string>(routingKey => routingKey == "mailatma"),
//              It.IsAny<IBasicProperties>(),
//              It.IsAny<byte[]>()),
//              Times.Once);
//        }
//    }
//}


using companyappbasic.Services.RabbitMQServices;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace companyappbasic.Tests.RabbitProducerTests
{
    public class RabbitProducerTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IModel> _channelMock;
        private readonly Mock<IConnection> _connectionMock;
        private readonly RabbitProducer _rabbitProducer;

        public RabbitProducerTests()
        {
            // Mock bağımlılıkları oluşturuyoruz
            _configurationMock = new Mock<IConfiguration>();
            _channelMock = new Mock<IModel>();
            _connectionMock = new Mock<IConnection>();

            // RabbitMQ yapılandırmasını ayarlıyoruz
            _configurationMock.Setup(config => config["RabbitMQ:HostName"]).Returns("localhost");
            _configurationMock.Setup(config => config["RabbitMQ:UserName"]).Returns("Melikenur");
            _configurationMock.Setup(config => config["RabbitMQ:Password"]).Returns("Mk546865");

            // Bağlantı ve kanal mock'larını oluşturuyoruz
            _connectionMock.Setup(conn => conn.CreateModel()).Returns(_channelMock.Object);

            // Sınıfın test edilecek örneğini oluşturuyoruz
            _rabbitProducer = new RabbitProducer(_configurationMock.Object);
        }

        [Fact]
        public async Task SendMessageAsync_ShouldSendMessageToQueue()
        {
            // Arrange
            var email = "test@example.com";
            var subject = "Test Subject";
            var body = "Test Body";

            // Mesajı JSON formatında serileştiriyoruz
            var emailMessage = new { email, subject, body };
            var expectedMessage = JsonConvert.SerializeObject(emailMessage);
            var expectedBodyBytes = Encoding.UTF8.GetBytes(expectedMessage);


            var mockChannel = new Mock<IModel>();
            mockChannel.Setup(channel => channel.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IBasicProperties>(), It.IsAny<byte[]>()))
                .Callback((string exchange, string routingKey, IBasicProperties properties, byte[] body) =>
                {
                    // Parametreleri burada kontrol edebilirsiniz.
                    Assert.Equal("mailatma", routingKey);
                    Assert.NotNull(body);
                });

            // Act
            await _rabbitProducer.SendMessageAsync("test@example.com", "Subject", "Body");

            // Assert
            mockChannel.Verify(m => m.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IBasicProperties>(), It.IsAny<byte[]>()), Times.Once);
        }
    }
}





