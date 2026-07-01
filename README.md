# 🚗 Araç Kiralama ve Takip Sistemi (MVC - DBFirst)

Modern arayüzlü ve performans odaklı, ASP.NET Core MVC tabanlı bir **Araç Kiralama Yönetim Sistemi** uygulamasıdır. Proje, hem müşteriler için şık bir vitrin (kullanıcı portalı) hem de yöneticiler için gelişmiş finansal/operasyonel analiz imkanları sunan bir yönetim paneli barındırmaktadır.

---

## ✨ Özellikler

### 👥 Müşteri Portalı (Kullanıcı Paneli)
*   **Modern Karanlık Tema:** Göz yormayan, premium koyu gri/siyah geçişli tasarım.
*   **Vitrin:** Öne çıkan araç modellerinin listesi ve teknik özellikleri.
*   **Hizmetlerimiz:** Sigorta, yol yardımı, teslimat vb. hizmetlerin şık ikonlarla gösterimi.
*   **İletişim Formu:** Müşterilerin doğrudan geri bildirim gönderebilmesi için tasarlanmış şık form alanları.

### 🛡️ Yönetim Paneli (Admin Dashboard)
*   **Canlı İstatistikler:** Toplam araç sayısı, aktif müşteri sayısı, güncel aktif sözleşmeler ve ciro bilgisi.
*   **Müşteri Yönetimi:** Müşteri ekleme, listeleme, güncelleme ve silme (CRUD) işlemleri.
*   **Araç Yönetimi:** Kilometre, günlük fiyat, plaka ve durum kontrolü ile araç envanter yönetimi.
*   **Kiralama Sözleşmeleri:** Başlangıç ve bitiş tarihlerine göre aktif/pasif kiralama kayıtları.
*   **Ödeme Takibi:** İşlem bazlı gelirlerin, ödeme tarihlerinin ve yöntemlerinin takibi.
*   **Finansal Raporlama:** VIP müşteriler, ortalama kiralama bedeli ve boşta yatan araç sayıları gibi iş analitiği raporları.
*   **Grafik Analizleri:** Son ödemeleri gösteren Chart.js tabanlı çizgi grafik.
*   **Dosya Çıktıları:** Tüm listelerin tek tıkla **PDF (QuestPDF)** veya **Excel (EPPlus)** formatında indirilmesi.

---

## 🛠️ Teknolojiler

*   **Framework:** .NET 9.0 / ASP.NET Core MVC
*   **ORM:** Entity Framework Core (Database First)
*   **Veritabanı:** Microsoft SQL Server
*   **Tasarım:** Bootstrap 5, Custom CSS3, Google Fonts (Inter & Outfit), FontAwesome 6
*   **Grafikler:** Chart.js
*   **Raporlama:** QuestPDF (PDF Raporları), EPPlus (Excel Raporları)

---

## 🚀 Kurulum Adımları

1.  **Veritabanı Kurulumu:**
    Projede Database First yaklaşımı kullanılmıştır. SQL Server veritabanınızda ilgili tabloların (`Customers`, `Vehicles`, `Rentals`, `Payments`) mevcut olduğundan emin olun.

2.  **Bağlantı Dizesi (Connection String):**
    `appsettings.json` dosyasında bulunan `"Default"` bağlantı dizesini kendi SQL Server adresiniz ve kimlik bilgilerinizle güncelleyin:
    ```json
    "ConnectionStrings": {
      "Default": "Server=YOUR_SERVER;Database=YOUR_DATABASE;Trusted_Connection=True;TrustServerCertificate=True;"
    }
    ```

3.  **Projenin Derlenmesi ve Çalıştırılması:**
    Proje dizininde terminali açarak aşağıdaki komutları uygulayabilirsiniz:
    ```bash
    # Bağımlılıkları geri yükleme
    dotnet restore

    # Projeyi derleme
    dotnet build

    # Projeyi çalıştırma
    dotnet run
    ```
    Tarayıcınızda `https://localhost:7198` veya terminalde belirtilen adresi açarak uygulamaya erişebilirsiniz.

---

## 📁 Dosya Yapısı

*   `Controllers/` - Uygulamanın iş mantığını ve raporlama metotlarını içeren denetleyiciler.
*   `Models/` - AppDbContext ve EF veritabanı entity sınıfları.
*   `Views/` - CSHTML formatındaki Razor sayfaları ve Layout bileşenleri.
*   `wwwroot/` - CSS, JS, görsel varlıklar ve şablon kütüphaneleri.
    *   `usertemplate/css/custom.css` - Müşteri paneli özel premium stil tanımları.
    *   `admintemplate/css/custom-admin.css` - Admin paneli modern UI tasarım tanımları.
