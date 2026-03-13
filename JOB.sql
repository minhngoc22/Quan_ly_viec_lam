-- =======================================
-- TẠO DATABASE JOB
-- =======================================
CREATE DATABASE JOB;
GO

USE JOB;
GO

-- =======================================
-- 1. Bảng Users (Người dùng hệ thống)
--  - Lưu thông tin tài khoản chung
--  - Role: Candidate | Employer | Admin
-- =======================================
CREATE TABLE Users (
    UserID INT IDENTITY PRIMARY KEY,              -- Khóa chính
    FullName NVARCHAR(100) NOT NULL,              -- Họ và tên
    Email NVARCHAR(100) UNIQUE NOT NULL,          -- Email đăng nhập
    PasswordHash NVARCHAR(255) NOT NULL,          -- Mật khẩu (hash)
    Role NVARCHAR(20) CHECK (Role IN ('Candidate','Employer','Admin')) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),         -- Ngày tạo tài khoản
    Status BIT DEFAULT 1                          -- 1: Hoạt động, 0: Khóa
);

-- =======================================
-- 2. Bảng Candidates (Ứng viên)
--  - Thông tin mở rộng cho user role = Candidate
-- =======================================
CREATE TABLE Candidates (
    CandidateID INT PRIMARY KEY,                  -- Liên kết với Users.UserID
    Phone NVARCHAR(20),
    Address NVARCHAR(255),
    DateOfBirth DATE,
    Gender NVARCHAR(10),
    FOREIGN KEY (CandidateID) REFERENCES Users(UserID)
);

-- =======================================
-- 3. Bảng Employers (Nhà tuyển dụng)
--  - Thông tin mở rộng cho user role = Employer
-- =======================================
CREATE TABLE Employers (
    EmployerID INT PRIMARY KEY,                   -- Liên kết với Users.UserID
    CompanyName NVARCHAR(200),
    Position NVARCHAR(100),                       -- Chức vụ người đăng tin (HR, Giám đốc, ...)
    Phone NVARCHAR(20),
    FOREIGN KEY (EmployerID) REFERENCES Users(UserID)
);

-- =======================================
-- 4. Bảng JobCategories (Ngành nghề)
-- =======================================
CREATE TABLE JobCategories (
    CategoryID INT IDENTITY PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(MAX) NULL
);

-- =======================================
-- 5. Bảng Companies (Công ty)
--  - Mỗi Employer có thể quản lý 1 hoặc nhiều công ty
--  - Bổ sung Logo và Giấy phép kinh doanh
-- =======================================
CREATE TABLE Companies (
    CompanyID INT IDENTITY PRIMARY KEY,
    EmployerID INT NOT NULL,
    CompanyName NVARCHAR(200) NOT NULL,
    Email NVARCHAR(100), -- 👈 thêm email công ty
    Description NVARCHAR(MAX),
    Address NVARCHAR(255),
    Website NVARCHAR(100),
    Logo NVARCHAR(255),
    BusinessLicense NVARCHAR(255),
    FOREIGN KEY (EmployerID) REFERENCES Employers(EmployerID)
);


-- =======================================
-- 6. Bảng Provinces (Tỉnh/Thành phố)
-- =======================================
CREATE TABLE Provinces (
    ProvinceId INT IDENTITY PRIMARY KEY,
    ProvinceName NVARCHAR(100) NOT NULL
);

-- Thêm dữ liệu mẫu Provinces
INSERT INTO Provinces (ProvinceName) VALUES
(N'An Giang'),(N'Bắc Ninh'),(N'Cà Mau'),(N'Cao Bằng'),
(N'Đắk Lắk'),(N'Điện Biên'),(N'Đồng Nai'),(N'Đồng Tháp'),
(N'Gia Lai'),(N'Hà Tĩnh'),(N'Hưng Yên'),(N'Khánh Hòa'),
(N'Lai Châu'),(N'Lâm Đồng'),(N'Lào Cai'),(N'Lạng Sơn'),
(N'Ninh Bình'),(N'Nghệ An'),(N'Phú Thọ'),(N'Quảng Ngãi'),
(N'Quảng Ninh'),(N'Quảng Trị'),(N'Sơn La'),(N'TP. Cần Thơ'),
(N'TP. Đà Nẵng'),(N'TP. Hải Phòng'),(N'TP. Hà Nội'),(N'TP. HCM'),
(N'TP. Huế'),(N'Tây Ninh'),(N'Thái Nguyên'),(N'Thanh Hóa'),
(N'Tuyên Quang'),(N'Vĩnh Long');

-- =======================================
-- 7. Bảng Jobs (Tin tuyển dụng)
--  - Bổ sung cột IsApproved: chỉ hiển thị khi Admin duyệt
-- =======================================
CREATE TABLE Jobs (
    JobID INT IDENTITY PRIMARY KEY,
    EmployerID INT NOT NULL,
    CompanyID INT NOT NULL,
    CategoryID INT NULL,
    ProvinceId INT NULL,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    Requirements NVARCHAR(MAX), -- 👈 yêu cầu công việc
    Benefits NVARCHAR(MAX),     -- 👈 quyền lợi
    Location NVARCHAR(200),
    Salary DECIMAL(18,2),
    JobType NVARCHAR(20) CHECK (JobType IN ('Full-time','Part-time','Internship','Remote')), -- 👈 loại hình
    CreatedAt DATETIME DEFAULT GETDATE(),
    ExpireDate DATETIME,
    IsApproved BIT DEFAULT 0,
    FOREIGN KEY (EmployerID) REFERENCES Employers(EmployerID),
    FOREIGN KEY (CompanyID) REFERENCES Companies(CompanyID),
    FOREIGN KEY (CategoryID) REFERENCES JobCategories(CategoryID),
    FOREIGN KEY (ProvinceId) REFERENCES Provinces(ProvinceId)
);

-- =======================================
-- 8. Bảng Applications (Ứng tuyển)
-- =======================================
CREATE TABLE Applications (
    ApplicationID INT IDENTITY PRIMARY KEY,
    JobID INT NOT NULL,
    CandidateID INT NOT NULL,
    AppliedAt DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(50) DEFAULT 'Pending', -- Pending | Approved | Rejected
    FOREIGN KEY (JobID) REFERENCES Jobs(JobID),
    FOREIGN KEY (CandidateID) REFERENCES Candidates(CandidateID)
);

-- =======================================
-- 9. Bảng SavedJobs (Việc làm đã lưu)
-- =======================================
CREATE TABLE SavedJobs (
    SavedID INT IDENTITY PRIMARY KEY,
    CandidateID INT NOT NULL,
    JobID INT NOT NULL,
    SavedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CandidateID) REFERENCES Candidates(CandidateID),
    FOREIGN KEY (JobID) REFERENCES Jobs(JobID)
);

-- =======================================
-- 10. Bảng CVs (Hồ sơ ứng viên)
-- =======================================
CREATE TABLE CVs (
    CVID INT IDENTITY PRIMARY KEY,
    CandidateID INT NOT NULL,
    FullName NVARCHAR(100),
    Email NVARCHAR(100),
    Phone NVARCHAR(20),
    Skills NVARCHAR(MAX),
    Experience NVARCHAR(MAX),
    Education NVARCHAR(MAX),
    CareerGoal NVARCHAR(MAX),
    FilePath NVARCHAR(255),                       -- Đường dẫn file upload
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CandidateID) REFERENCES Candidates(CandidateID)
);

-- =======================================
-- 11. Bảng CoverLetters (Thư xin việc)
-- =======================================
CREATE TABLE CoverLetters (
    LetterID INT IDENTITY PRIMARY KEY,
    CandidateID INT NOT NULL,
    Title NVARCHAR(100),
    Content NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CandidateID) REFERENCES Candidates(CandidateID)
);

-- =======================================
-- 12. Bảng AdminActions (Log thao tác Admin)
--  - Lưu lại các hành động: duyệt job, khóa user...
-- =======================================
CREATE TABLE AdminActions (
    ActionID INT IDENTITY PRIMARY KEY,
    AdminID INT NOT NULL,                         -- Người thực hiện
    ActionType NVARCHAR(50),                      -- ApproveJob | BanUser | RejectJob ...
    TargetID INT NULL,                            -- JobID hoặc UserID tùy loại action
    Note NVARCHAR(MAX) NULL,                      -- Ghi chú chi tiết
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (AdminID) REFERENCES Users(UserID)
);

-- =======================================
-- 13. Bảng PasswordResetTokens (OTP Reset Password)
-- =======================================
CREATE TABLE PasswordResetTokens (
    TokenID INT IDENTITY PRIMARY KEY,
    UserID INT NOT NULL,                          -- Liên kết tới Users
    OTP NVARCHAR(10) NOT NULL,                    -- Mã OTP gửi qua email
    ExpiryTime DATETIME NOT NULL,                 -- Thời hạn OTP (vd: 10 phút)
    IsUsed BIT DEFAULT 0,                         -- 0 = chưa dùng, 1 = đã dùng
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- =======================================
-- 14. Bảng Reports (Báo cáo vi phạm)
--  - Người dùng có thể báo cáo job hoặc user
-- =======================================
CREATE TABLE Reports (
    ReportID INT IDENTITY PRIMARY KEY,
    ReporterID INT NOT NULL,                 -- Ai báo cáo
    JobID INT NULL,                          -- Báo cáo tin tuyển dụng
    TargetUserID INT NULL,                   -- Hoặc báo cáo user
    Reason NVARCHAR(MAX) NOT NULL,           -- Lý do
    Status NVARCHAR(50) DEFAULT N'Chưa xử lý', -- Chưa xử lý | Đã xử lý
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ReporterID) REFERENCES Users(UserID),
    FOREIGN KEY (JobID) REFERENCES Jobs(JobID),
    FOREIGN KEY (TargetUserID) REFERENCES Users(UserID)
);

-- =======================================
-- DỮ LIỆU MẪU
-- =======================================

-- Users
INSERT INTO Users (FullName, Email, PasswordHash, Role, Status)
VALUES
(N'Nguyễn Văn A', 'vana@gmail.com', '123456', 'Candidate', 1),
(N'Trần Thị B', 'thib@gmail.com', '123456', 'Candidate', 1),
(N'Phạm Văn C', 'vanc@gmail.com', '123456', 'Employer', 1),
(N'Lê Thị D', 'thid@gmail.com','123456', 'Employer', 1),
(N'Admin', 'admin@jobify.vn', 'admin123', 'Admin', 1);

-- Candidates
INSERT INTO Candidates (CandidateID, Phone, Address, DateOfBirth, Gender)
VALUES
(1, '0912345678', N'Hà Nội', '1995-05-12', N'Nam'),
(2, '0987654321', N'TP.HCM', '1997-08-20', N'Nữ');

-- Employers
INSERT INTO Employers (EmployerID, CompanyName, Position, Phone)
VALUES
(3, N'Công ty TNHH Phần mềm Sao Việt', N'HR Manager', '0901234567'),
(4, N'Công ty Cổ phần Thương mại Ánh Dương', N'Giám đốc nhân sự', '0934567890');

-- JobCategories
INSERT INTO JobCategories (CategoryName, Description)
VALUES
(N'IT - Phần mềm', N'Lập trình, phát triển phần mềm, công nghệ thông tin'),
(N'IT - Phần cứng/Mạng', N'Hệ thống, bảo mật, quản trị mạng'),
(N'Kinh doanh/Bán hàng', N'Bán hàng, tư vấn khách hàng, chăm sóc khách hàng'),
(N'Kế toán/Tài chính', N'Kế toán, tài chính, kiểm toán'),
(N'Marketing/Truyền thông', N'Tiếp thị, quảng cáo, truyền thông'),
(N'Nhân sự/Hành chính', N'Tuyển dụng, quản lý nhân sự, hành chính'),
(N'Xây dựng/Kiến trúc', N'Thiết kế, thi công, giám sát công trình xây dựng'),
(N'Cơ khí/Kỹ thuật', N'Chế tạo, bảo trì, sửa chữa máy móc'),
(N'Giáo dục/Đào tạo', N'Giảng dạy, đào tạo, nghiên cứu'),
(N'Y tế/Sức khỏe', N'Bác sĩ, y tá, dược sĩ'),
(N'Ngân hàng', N'Tín dụng, giao dịch viên, phân tích tài chính'),
(N'Luật/Pháp lý', N'Tư vấn pháp luật, luật sư'),
(N'Du lịch/Khách sạn', N'Lễ tân, hướng dẫn viên, quản lý khách sạn'),
(N'Nông nghiệp/Thủy sản', N'Trồng trọt, chăn nuôi, nuôi trồng thủy sản');

-- Companies (có Logo và Giấy phép KD)
INSERT INTO Companies (EmployerID, CompanyName, Description, Address, Website, Logo, BusinessLicense)
VALUES
(3, N'Công ty TNHH Phần mềm Sao Việt', N'Chuyên phát triển phần mềm, dịch vụ IT.', N'Hà Nội', 'www.saoviet.com',
 '/image/logo/1.png', 'GPĐKKD-12345'),
(4, N'Công ty Cổ phần Thương mại Ánh Dương', N'Chuyên phân phối hàng tiêu dùng.', N'TP.HCM', 'www.anhduong.vn',
 '/image/logo/3.png', 'GPĐKKD-67890');

-- Jobs (chú ý: IsApproved = 0 => chờ duyệt)
INSERT INTO Jobs (EmployerID, CompanyID, CategoryID, ProvinceId, Title, Description, Location, Salary, ExpireDate, IsApproved)
VALUES
(3, 1, 1, 28, N'Lập trình viên .NET', N'Phát triển ứng dụng web ASP.NET Core, SQL Server.', N'Quận 12', 15000000, '2025-12-31', 0),
(3, 1, 1, 34, N'Tester phần mềm', N'Thực hiện kiểm thử, viết test case.', N'Phường 3', 12000000, '2025-11-30', 0),
(4, 2, 2, 27, N'Nhân viên kinh doanh', N'Tìm kiếm khách hàng, tư vấn sản phẩm.', N'Quận Hai Bà Trưng', 10000000, '2025-10-31', 0),
(4, 2, 3, 27, N'Kế toán tổng hợp', N'Quản lý sổ sách, báo cáo tài chính.', N'Quận Cầu Giấy', 13000000, '2025-09-30', 0);

-- Applications
INSERT INTO Applications (JobID, CandidateID, Status)
VALUES
(1, 1, N'Pending'),
(2, 2, N'Approved'),
(3, 1, N'Rejected');

-- SavedJobs
INSERT INTO SavedJobs (CandidateID, JobID)
VALUES
(1, 2),
(1, 3),
(2, 1);

-- CVs
INSERT INTO CVs (CandidateID, FullName, Email, Phone, Skills, Experience, Education, CareerGoal, FilePath)
VALUES
(1, N'Nguyễn Văn A', 'vana@gmail.com', '0912345678',
 N'C#, SQL, ASP.NET', N'2 năm lập trình .NET',
 N'Đại học Bách khoa Hà Nội', N'Mong muốn trở thành Senior Developer',
 '/uploads/cv_nguyenvana.pdf'),
(2, N'Trần Thị B', 'thib@gmail.com', '0987654321',
 N'Excel, Kế toán, Phân tích tài chính', N'1 năm kế toán tổng hợp',
 N'Đại học Kinh tế TP.HCM', N'Phát triển sự nghiệp trong lĩnh vực kế toán',
 '/uploads/cv_tranthib.docx');

-- CoverLetters
INSERT INTO CoverLetters (CandidateID, Title, Content)
VALUES
(1, N'Thư xin việc .NET', N'Kính gửi nhà tuyển dụng, tôi có kinh nghiệm 2 năm lập trình .NET...'),
(2, N'Thư xin việc Kế toán', N'Xin chào, tôi tốt nghiệp chuyên ngành kế toán và có 1 năm kinh nghiệm...');

-- Reports
INSERT INTO Reports (ReporterID, JobID, Reason)
VALUES
(1, 3, N'Tin tuyển dụng sai thông tin liên hệ'),
(2, 1, N'Nội dung không phù hợp'),
(1, NULL, N'User đăng tin spam');

-- =======================================
-- Bảng Notifications (Thông báo hệ thống)
--  - Admin có thể gửi cho Candidate hoặc Employer
-- =======================================
CREATE TABLE Notifications (
    Id INT IDENTITY PRIMARY KEY,
    UserId INT NOT NULL,                          -- Gửi cho User nào (Candidate/Employer)
    Message NVARCHAR(MAX) NOT NULL,               -- Nội dung thông báo
    IsRead BIT NOT NULL DEFAULT 0,                -- Đã đọc hay chưa
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserID) -- Liên kết Users (Role quyết định là Candidate/Employer)
);

ALTER TABLE Jobs
ADD JobType NVARCHAR(20) 
    CHECK (JobType IN ('Full-time', 'Part-time', 'Internship', 'Remote')) 
    NULL; -- NULL nếu chưa xác định

UPDATE Jobs
SET JobType = 'Full-time'
WHERE JobID IN (1, 3);

UPDATE Jobs
SET JobType = 'Part-time'
WHERE JobID IN (2, 4);
