# CompanyAppXunitTestProject

Bu proje, **CompanyAppBackend** projesinin servis ve controller katmanları için yazılmış kapsamla xUnit testlerini içermektedir. Amaç, yazılan kodların doğruluğunu, hatasız çalışıp çalışmadığını doğrulamak ve sistemin daha güvenilir hale getirilmesini sağlamaktır.

---

## 🚀 Kullanılan Teknolojiler

* **.NET Core 8.0**
* **xUnit** (birim testi framework)
* **Moq** (mock nesneler oluşturmak için)
* **FluentAssertions** (daha okunabilir test ifadeleri)
* **Entity Framework Core InMemory** (veritabanı simülasıyonu)

---

## 🔧 Test Edilen Katmanlar

### 1. Controller Testleri (`ControllerTests`)

* `AccountControllerTest.cs`
* `AdminControllerTest.cs`
* `EmployeeControllerTest.cs`
* `TaskControllerTest.cs`

### 2. Servis Testleri (`ServicesTests`)

* `AdminServicesTest.cs`
* `EmployeeServicesTest.cs`
* `TaskServicesTest.cs`
* `TokenServicesTest.cs`
* `EmailServicesTest.cs`
* `RabbitConsumerServicesTest.cs`
* `RabbitProducerServicesTest.cs`
* `BackgroundServicesTest.cs`

---

## 🔹 Testlerde Amaçlananlar

* Servislerin bağımlılıklarını mock'layarak yalnızca kendi davranışını test etmek
* Controller için doğru HTTP yanıtlarını ve durum kodlarını gözlemlemek
* RabbitMQ işlemlerinin doğru çalışıp çalışmadığını test etmek
* Token üretiminin ve JWT içeriklerinin doğruluğunu doğrulamak

---

## 📆 Nasıl Çalıştırılır?

1. Visual Studio ile `CompanyAppTestProject.sln` dosyasını aç.
2. Test Explorer'ı açarak `Run All Tests` butonuna tıkla.
3. CLI ile test çalıştırmak istersen:

```bash
cd CompanyAppXunitTestProject-main
dotnet test
```

---

## 😍 Geliştirici Notları

* Tüm servisler mocking ile bağımsız olarak test edilmiştir.
* Controller testleri `Arrange-Act-Assert` yapısına uygun yazılmıştır.
* Kod okunabilirliği için `FluentAssertions` tercih edilmiştir.

---
## 👤 Geliştirici

Bu test projesi, **CompanyAppBackend** sisteminin test kapsamasını artırmak için **Melikenur Kaya** tarafından geliştirilmiştir.
