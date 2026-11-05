# Jobify - Hệ thống Quản lý & Tuyển dụng Việc làm Trực tuyến

Hệ thống Website tuyển dụng việc làm trực tuyến xây dựng trên nền tảng **ASP.NET Core 8.0 MVC** và **Entity Framework Core 9**, kết nối cơ sở dữ liệu **Microsoft SQL Server**. Hệ thống kết nối toàn diện 3 đối tượng người dùng: **Ứng viên (Candidate)**, **Nhà tuyển dụng (Employer)** và **Quản trị viên (Admin)**, đồng thời tích hợp **Trí tuệ nhân tạo (AI Chatbot)** và **xử lý tự động tài liệu Word**.

---

## 🌟 Tính năng nổi bật

### 1. Phân hệ Ứng viên (Candidate)
- **Quản lý Hồ sơ & CV chuyên sâu**: Tạo và cập nhật CV đa thành phần (Mục tiêu nghề nghiệp, Kỹ năng, Kinh nghiệm làm việc, Học vấn, Chứng chỉ) hoặc đính kèm file CV (PDF / Word).
- **Tìm kiếm & Lọc việc làm nâng cao**: Tìm kiếm theo từ khóa, ngành nghề, tỉnh thành, loại hình làm việc (Full-time, Part-time, Internship, Remote) và mức lương.
- **Ứng tuyển & Lưu tin**: Nộp hồ sơ ứng tuyển trực tuyến, theo dõi tiến độ duyệt hồ sơ, xem lý do bị từ chối (nếu có); lưu việc làm yêu thích.
- **Gợi ý việc làm thông minh (Smart Job Matching)**: Thuật toán so khớp tự động giữa kỹ năng, mục tiêu nghề nghiệp trong CV với yêu cầu công việc để đề xuất việc làm tương thích nhất.
- **Tương tác**: Đọc và tra cứu Cẩm nang nghề nghiệp (`CareerGuide`), bình luận trao đổi tại tin tuyển dụng.
- **Jobify Bot**: Chatbot AI trực tuyến (OpenAI GPT) hỗ trợ tư vấn định hướng công việc và giải đáp câu hỏi.

### 2. Phân hệ Nhà tuyển dụng (Employer)
- **Đăng ký doanh nghiệp**: Đăng ký thông tin tài khoản đại diện và hồ sơ doanh nghiệp.
- **Đăng tin tuyển dụng đa phương thức**:
  - Nhập liệu trực quan qua biểu mẫu.
  - **Đăng tin tự động qua file Word**: Tải form Word mẫu (`JobFormTemplate.docx`), điền thông tin và upload để hệ thống tự động bóc tách (parse) bằng OpenXML.
- **Quản lý tin tuyển dụng**: Theo dõi trạng thái tin (chờ duyệt / đã duyệt / hết hạn), chỉnh sửa hoặc xóa tin.
- **Quản lý ứng viên**: Xem danh sách ứng viên nộp hồ sơ, xem CV chi tiết, duyệt hồ sơ hoặc từ chối kèm phản hồi.
- **Thông báo thời gian thực**: Nhận thông báo tự động khi có hồ sơ ứng tuyển mới.

### 3. Phân hệ Quản trị viên (Admin)
- **Dashboard phân tích tổng quan**: Thống kê số lượng người dùng, doanh nghiệp, tin tuyển dụng, bài viết cẩm nang, báo cáo vi phạm và top tin tuyển dụng được quan tâm nhiều nhất.
- **Kiểm duyệt tin tuyển dụng**: Phê duyệt hoặc từ chối tin tuyển dụng kèm ghi chú lý do.
- **Quản lý tài khoản**: Khóa / Mở khóa tài khoản vi phạm, cấp quyền Admin / SubAdmin.
- **Quản trị nội dung**: Quản lý danh mục ngành nghề (`JobCategories`), bài viết cẩm nang nghề nghiệp (`CareerGuides`).
- **Xử lý phản ánh / Báo cáo**: Tiếp nhận và xử lý báo cáo vi phạm từ người dùng.
- **Nhật ký kiểm toán (Admin Audit Logs)**: Ghi lại lịch sử các thao tác kiểm duyệt và quản trị.

---

## 🛠️ Công nghệ sử dụng

- **Backend**: C# .NET 8.0, ASP.NET Core MVC
- **Database / ORM**: Microsoft SQL Server, Entity Framework Core 9 (Code/Database First)
- **Tài liệu & Tự động hóa**: OpenXML SDK (`DocumentFormat.OpenXml`), `Xceed.Words.NET`
- **Mã hóa & Xác thực**: BCrypt (`BCrypt.Net-Next`), SHA-256, Session-based Authentication
- **Dịch vụ tích hợp**:
  - **OpenAI API**: Mô hình ngôn ngữ hỗ trợ tư vấn tìm việc.
  - **Gmail SMTP**: Dịch vụ gửi email mã OTP đặt lại mật khẩu.
- **Frontend**: Razor Views, HTML5, CSS3, JavaScript, Bootstrap, FontAwesome

---

## 🚀 Hướng dẫn cài đặt & Khởi chạy

### 1. Yêu cầu môi trường
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) trở lên.
- [Microsoft SQL Server](https://www.microsoft.com/sql-server) (2019 trở lên) hoặc SQL Server Express / LocalDB.
- [Visual Studio 2022](https://visualstudio.microsoft.com/) hoặc Visual Studio Code.

### 2. Cài đặt Cơ sở dữ liệu
1. Mở **SQL Server Management Studio (SSMS)** hoặc công cụ quản lý SQL tương đương.
2. Mở file kịch bản tạo cơ sở dữ liệu và dữ liệu mẫu: **[`JOB1.sql`](JOB1.sql)**.
3. Nhấn **Execute (F5)** để khởi tạo Database `JOB` cùng toàn bộ bảng và dữ liệu mẫu ban đầu.

### 3. Cấu hình ứng dụng
Mở file `DoanWeb/appsettings.json` và cấu hình các thông số phù hợp:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=JOB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;"
  },
  "EmailSettings": {
    "From": "your-email@gmail.com",
    "AppPassword": "your-app-password"
  },
  "OpenAI": {
    "ApiKey": "your-openai-api-key"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> **Lưu ý bảo mật**: Không commit thông tin mật khẩu email thực hoặc OpenAI API Key riêng lên kho lưu trữ công khai.

### 4. Biên dịch và khởi chạy
Mở terminal tại thư mục gốc của dự án hoặc thư mục `DoanWeb`:

```bash
# Phục hồi packages
dotnet restore

# Biên dịch dự án
dotnet build

# Chạy ứng dụng
dotnet run --project DoanWeb/DoanWeb.csproj
```

Truy cập website trên trình duyệt: `https://localhost:7083` hoặc `http://localhost:5083` (tùy cổng Kestrel được cấp).

---

## 🔑 Tài khoản thử nghiệm mặc định

Dữ liệu mẫu từ `JOB1.sql` cung cấp sẵn các tài khoản để kiểm thử:

| Vai trò | Email đăng nhập | Mật khẩu | Trang đăng nhập |
|---|---|---|---|
| **Admin** | `admin@jobify.vn` | `admin123` | `/Account/LoginAdmin` |
| **Nhà tuyển dụng** | `vanc@gmail.com` | `123456` | `/Account/LoginEmployer` |
| **Ứng viên** | `vana@gmail.com` | `123456` | `/Account/LoginCandidate` |

---

## 📁 Cấu trúc thư mục dự án

```text
DoanWeb/
├── DoanWeb.sln                  # Solution file
├── JOB1.sql                     # Kịch bản khởi tạo cơ sở dữ liệu đầy đủ
├── JobFormTemplate (1).docx     # Mẫu file Word đăng tin tuyển dụng
├── .gitignore                   # Cấu hình bỏ qua file nhị phân & cấu hình cá nhân
├── README.md                    # Tài liệu hướng dẫn dự án
└── DoanWeb/                     # Thư mục mã nguồn chính (ASP.NET Core Web App)
    ├── Controllers/             # Xử lý logic điều hướng và nghiệp vụ (MVC)
    │   ├── AccountController.cs
    │   ├── AdminController.cs
    │   ├── CandidateController.cs
    │   ├── CareerGuideController.cs
    │   ├── ChatBotController.cs
    │   ├── EmployerController.cs
    │   ├── ForgotPasswordController.cs
    │   ├── HomeController.cs
    │   ├── JobController.cs
    │   └── NotificationController.cs
    ├── Models/                  # Thực thể dữ liệu (EF Core) và ViewModels
    │   ├── JobContext.cs
    │   ├── User.cs, Job.cs, Company.cs, Application.cs, Cv.cs...
    │   └── Các ViewModels phục vụ tương tác View
    ├── Views/                   # Giao diện người dùng Razor Views (.cshtml)
    │   ├── Account/, Admin/, Candidate/, Employer/, Job/...
    │   └── Shared/ (_Layout.cshtml, _ChatBot.cshtml...)
    ├── wwwroot/                 # Tài nguyên tĩnh (CSS, JS, Hình ảnh, CV upload)
    ├── appsettings.json         # File cấu hình ứng dụng
    └── Program.cs               # File cấu hình Pipeline & Dependency Injection
```
