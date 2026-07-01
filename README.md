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

## 📸 Uygulama Görselleri

### Arayüz

<p align="center">
<img width="1228" height="652" alt="image" src="https://github.com/user-attachments/assets/976a1055-c966-475f-b72a-e6a1baa80892" />
</p>

---

### Arayüz
<p align="center">
<img width="1598" height="846" alt="image" src="https://github.com/user-attachments/assets/cd74d7fd-355f-4784-af30-e25253dd7dba" />
</p>

### Admin Panel Giriş Ekranı

<p align="center">
<img width="1498" height="940" alt="image" src="https://github.com/user-attachments/assets/ea581182-56f6-47de-b9d7-9099a8872051" />
</p>

---

### Müşteri Yönetim Ekranı

<p align="center">
   <img width="1463" height="903" alt="image" src="https://github.com/user-attachments/assets/e6777c1f-ec6d-4630-981d-fa7657108ed3" />
</p>

---

### Araç Yönetim Ekranı

<p align="center">
<img width="1637" height="902" alt="image" src="https://github.com/user-attachments/assets/957ab230-3f3a-4da8-9e30-6773749da13b" />
</p>

---

### Kiralama İşlemleri Yönetim Ekranı

<p align="center">
 <img width="1642" height="905" alt="image" src="https://github.com/user-attachments/assets/335ebc8f-d751-4d65-97c0-5ef6adc8902c" />

</p>

---

### Ödeme İşlemleri Yönetim Ekranı

<p align="center">
  <img width="1627" height="857" alt="image" src="https://github.com/user-attachments/assets/700cda8f-8f40-494c-a74c-c325d388836d" />
</p>

---

### Sistem Raporu Ekranı

<p align="center">
<img width="1650" height="855" alt="image" src="https://github.com/user-attachments/assets/fd443c84-14fd-4d29-990b-35c22695a7a9" />
</p>

