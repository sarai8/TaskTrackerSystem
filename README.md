# TaskTrackerSystem

Staj dokümanındaki **Basit Görev Takip Sistemi** gereksinimlerine göre hazırlanmış .NET 8 / ASP.NET Core MVC örnek projesidir. Gereksinimlerde C#/.NET, SQL Server, katmanlı mimari + MVC, doğrulama, Entity Framework, FluentValidation ve AutoMapper isteniyor; bu proje bunları temel alır.

## Katmanlar
- `TaskTrackerSystem.Web`: MVC Controllers, Razor Views, authentication/UI
- `TaskTrackerSystem.Application`: DTO, service, interface, validation, mapping
- `TaskTrackerSystem.Domain`: entity ve enumlar
- `TaskTrackerSystem.Infrastructure`: EF Core, SQL Server, Identity, repository
- `TaskTrackerSystem.Tests`: xUnit validation testleri

## Özellikler
- Kayıt / giriş / çıkış
- Kullanıcının kendi görevlerini listeleme
- Görev oluşturma, düzenleme, silme
- Görevi tamamlama
- Durum ve öncelik
- Son tarih kontrolü
- Dashboard: toplam, bekleyen, tamamlanan, geciken
- Kullanıcı bazlı erişim kontrolü
- FluentValidation
- AutoMapper
- EF Core + SQL Server
- ASP.NET Core Identity
- Bootstrap 5

## Visual Studio ile çalıştırma
1. Visual Studio 2022'de `TaskTrackerSystem.sln` dosyasını aç.
2. NuGet restore işleminin tamamlanmasını bekle.
3. SQL Server LocalDB'nin (`MSSQLLocalDB`) kurulu olduğundan emin ol. İstersen `TaskTrackerSystem.Web/appsettings.json` içindeki connection string'i kendi SQL Server'ına göre değiştir.
4. Package Manager Console'da Infrastructure projesini hedefleyerek migration oluştur:
   `Add-Migration InitialCreate -Project TaskTrackerSystem.Infrastructure -StartupProject TaskTrackerSystem.Web`
5. Veritabanını oluştur:
   `Update-Database -Project TaskTrackerSystem.Infrastructure -StartupProject TaskTrackerSystem.Web`
6. `TaskTrackerSystem.Web` projesini Startup Project yap ve çalıştır.

## CLI ile
```bash
dotnet restore
dotnet build
dotnet test
dotnet ef migrations add InitialCreate --project TaskTrackerSystem.Infrastructure --startup-project TaskTrackerSystem.Web
dotnet ef database update --project TaskTrackerSystem.Infrastructure --startup-project TaskTrackerSystem.Web
dotnet run --project TaskTrackerSystem.Web
```

> Bu çalışma ortamında .NET SDK kurulu olmadığı için burada derleme/migration çalıştırılmadı. Kaynak kod ve solution Visual Studio/.NET 8 ortamında çalıştırılmak üzere hazırlanmıştır.
