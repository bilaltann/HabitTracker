
# HabitTracker

HabitTracker, kişisel alışkanlıklarınızı takip edebileceğiniz, oyunlaştırma (gamification) dinamikleriyle motivasyonunuzu yüksek tutan ve arkadaşlarınızla alışkanlıklarınızı paylaşabileceğiniz tam donanımlı bir Full-Stack web uygulamasıdır. 

Bu proje, yerel geliştirme ortamından çıkarak AWS EC2 bulut sunucularında Docker kullanılarak baştan sona canlıya alınmış (Production-Ready) bir mimariye sahiptir.

## Öne Çıkan Özellikler

* **Gelişmiş Kimlik Doğrulama:** Standart JWT tabanlı Email/Şifre girişi, Google OAuth2 ile Sosyal Giriş (SSO) ve MailKit üzerinden "Şifremi Unuttum" e-posta doğrulama akışı.
* **Oyunlaştırma (Gamification):** Alışkanlıklar yerine getirildikçe kazanılan puanlar, seviye (Level) atlama sistemi ve özel başarım rozetleri (Badges).
* **Sosyal Etkileşim:** Arkadaş ekleme sistemi ve ortak alışkanlıkları paylaşabilme/takip edebilme.
* **Yönetici Paneli (Admin):** Kullanıcıları yönetme, rozet atama ve detaylı sistem loglarını (SystemLogs) görüntüleme arayüzü.
* **Konteyner Mimarisi:** Frontend (Nginx), Backend (.NET) ve Veritabanı (Azure SQL Edge) servislerinin docker-compose ile tam izolasyonlu çalışması.

## Kullanılan Teknolojiler

* **Backend:** C# & .NET 8 (ASP.NET Core Web API), Entity Framework Core (Code-First), Katmanlı Mimari (Clean/Onion Architecture Prensipleri), MailKit (SMTP Mail Gönderimi)
* **Frontend:** HTML5, CSS3, Vanilla JavaScript (ES6+), Responsive Tasarım & Dinamik UI (DOM Manipülasyonu)
* **Veritabanı & DevOps:** Microsoft Azure SQL Edge (Düşük kaynak tüketimi için optimize edilmiş MSSQL), Docker & Docker Compose, AWS EC2 (Ubuntu Linux)

## Yerel Ortamda Kurulum (Local Development)

Projeyi kendi bilgisayarınızda çalıştırmak için aşağıdaki adımları izleyebilirsiniz.

### Ön Koşullar

* Docker masaüstü uygulaması
* .NET 8 SDK (Backend geliştirme için)
* Google Cloud Console üzerinden alınmış OAuth Client ID
* Gmail Uygulama Şifresi (App Passwords)

### Kurulum Adımları

1. Repoyu bilgisayarınıza klonlayın:
```bash
git clone [https://github.com/KULLANICI_ADIN/HabitTracker.git](https://github.com/KULLANICI_ADIN/HabitTracker.git)
cd HabitTracker
HabitTracker.API/appsettings.json dosyasını kendi veritabanı, Google OAuth ve Mail ayarlarınıza göre yapılandırın.

Docker compose ile tüm sistemi ayağa kaldırın:

Bash
docker-compose up -d --build
Veritabanı tablolarını oluşturmak için Entity Framework Migration'larını uygulayın (Package Manager Console):

Bash
Update-Database
AWS EC2 Canlı Ortam Kurulumu (Deployment)
Projenin canlıya alınma sürecinde, bulut kaynaklarını en verimli şekilde kullanmak üzere özel yapılandırmalar yapılmıştır.

Sunucu ve Disk Yapılandırması
Instance Tipi: AWS Free Tier - t2.micro (1 GB RAM, 1 vCPU)

EBS Disk: Varsayılan 8 GB disk, .NET derleme (build) süreçlerinde ve MSSQL imajında yetersiz kaldığı için 20 GB kapasiteye yükseltilmiştir.

Swap (Sanal RAM) Yönetimi
t2.micro'nun fiziksel 1 GB RAM'i SQL Server'ın ayağa kalkması için yetersiz olduğundan, Linux üzerinde 2 GB Swap (Takas Alanı) oluşturulmuş ve Microsoft'un hafif sürümü olan Azure SQL Edge imajı kullanılarak RAM darboğazı aşılmıştır.

Bash
sudo fallocate -l 2G /swapfile
sudo chmod 600 /swapfile
sudo mkswap /swapfile
sudo swapon /swapfile
AWS Güvenlik Grubu (Security Group) Ayarları
Dış dünyadan uygulamaya ve veritabanına erişim için AWS panelinden aşağıdaki portlar dışarıya (Inbound Rules) açılmıştır:

80 (HTTP): Nginx / Frontend erişimi için.

8080 (Custom TCP): .NET Backend API erişimi için.

1433 (MSSQL): Yerel bilgisayardan SQL Server Management Studio (SSMS) ile canlı veritabanını yönetmek için.

Canlıya Alma Komutları
Sunucuya SSH ile bağlandıktan sonra:

Bash
git pull origin master
sudo docker compose down -v # Temiz bir başlangıç için (Opsiyonel)
sudo docker compose up -d --build
Sistem başarıyla ayağa kalktığında http://[AWS-PUBLIC-IP] adresi üzerinden uygulamaya erişilebilir.

Katkıda Bulunma
Bu proje geliştirmeye açıktır. Katkıda bulunmak isterseniz lütfen bir Pull Request oluşturun veya Issue açarak fikirlerinizi paylaşın.
