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
ALTER TABLE Users
ADD Avatar NVARCHAR(255) NULL;
ALTER TABLE Users
ADD AvatarColor NVARCHAR(7) NULL; -- Lưu màu HEX, vd: #FF5733


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
-- 5. Bảng Companies (Công ty)
--  - Mỗi Employer có thể quản lý 1 hoặc nhiều công ty
--  - Bổ sung Logo và Giấy phép kinh doanh
-- =======================================
-- =======================================
-- 5. Bảng Companies (Công ty)
--  - Mỗi Employer có thể quản lý 1 hoặc nhiều công ty
--  - Liên kết với JobCategories (ngành nghề) và Provinces (tỉnh/thành phố)
-- =======================================
-- =======================================
-- Bảng Companies (Công ty)
--  - Lưu thông tin chi tiết của công ty tuyển dụng
--  - Mỗi công ty gắn với 1 Employer (người đại diện)
--  - Có phân loại theo Category và Province
-- =======================================
CREATE TABLE Companies (
    CompanyID INT IDENTITY PRIMARY KEY,              -- Khóa chính (tự tăng)
    EmployerID INT NOT NULL,                         -- FK sang Employers (người đại diện công ty)
    CategoryID INT NOT NULL,                         -- FK sang JobCategories (ngành nghề chính)
    ProvinceId INT NOT NULL,                         -- FK sang Provinces (địa phương)
    CompanyName NVARCHAR(200) NOT NULL,              -- Tên công ty
    Email NVARCHAR(100),                             -- Email liên hệ
    Description NVARCHAR(MAX),                       -- Mô tả chi tiết
    Address NVARCHAR(255),                           -- Địa chỉ công ty
    Website NVARCHAR(100),                           -- Website công ty
    Logo NVARCHAR(255),                              -- Link/logo công ty
    
    CreatedAt DATETIME DEFAULT GETDATE(),            -- Ngày tạo (mặc định = thời điểm thêm)

    -- Ràng buộc khóa ngoại
    FOREIGN KEY (EmployerID) REFERENCES Employers(EmployerID),
    FOREIGN KEY (CategoryID) REFERENCES JobCategories(CategoryID),
    FOREIGN KEY (ProvinceId) REFERENCES Provinces(ProvinceId)
)



-- =======================================
-- 7. Bảng Jobs (Tin tuyển dụng)
--  - Bổ sung cột IsApproved: chỉ hiển thị khi Admin duyệt
-- =======================================
-- TẠO BẢNG JOBS (Tin tuyển dụng)
-- =======================================
CREATE TABLE Jobs (
    JobID INT IDENTITY PRIMARY KEY,                              -- Khóa chính, tự tăng
    EmployerID INT NOT NULL,                                     -- Nhà tuyển dụng (FK)
    CompanyID INT NOT NULL,                                      -- Công ty (FK)
    CategoryID INT NULL,                                         -- Danh mục việc làm
    ProvinceId INT NULL,                                         -- Tỉnh / Thành phố
    Title NVARCHAR(200) NOT NULL,                                -- Tiêu đề công việc
    Description NVARCHAR(MAX),                                   -- Mô tả công việc
    Requirements NVARCHAR(MAX),                                  -- Yêu cầu ứng viên
    Benefits NVARCHAR(MAX),                                      -- Quyền lợi
    Location NVARCHAR(200),                                      -- Địa điểm làm việc
    Salary NVARCHAR(50) NOT NULL,                                -- Mức lương
    JobType NVARCHAR(20) CHECK (JobType IN ('Full-time','Part-time','Internship','Remote')),  
                                                                -- Loại hình công việc
	GenderRequirement NVARCHAR(20) DEFAULT N'Không yêu cầu',      -- Nam / Nữ / Không yêu cầu
    VacancyCount INT DEFAULT 1,                                  -- Số lượng tuyển
    CreatedAt DATETIME DEFAULT GETDATE(),                        -- Ngày đăng
    ExpireDate DATETIME,                                         -- Ngày hết hạn
    IsApproved BIT DEFAULT 0,                                    -- 0: chưa duyệt, 1: đã duyệt
    Keywords NVARCHAR(MAX) NULL,                                 -- Từ khóa tìm kiếm
    ImageUrl NVARCHAR(300) NULL,                                 -- 🔥 Ảnh minh họa công việc
    FOREIGN KEY (EmployerID) REFERENCES Employers(EmployerID),
    FOREIGN KEY (CompanyID) REFERENCES Companies(CompanyID),
    FOREIGN KEY (CategoryID) REFERENCES JobCategories(CategoryID),
    FOREIGN KEY (ProvinceId) REFERENCES Provinces(ProvinceId)
);
GO

ALTER TABLE Jobs
ADD Latitude FLOAT NULL,
    Longitude FLOAT NULL;
GO

ALTER TABLE Jobs ADD Priority INT DEFAULT 0;

ALTER TABLE Jobs 
ADD IsExpired BIT NOT NULL DEFAULT 0;
ALTER TABLE Jobs ADD SortOrder INT NOT NULL DEFAULT 0;

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
    FOREIGN KEY (CandidateID) REFERENCES Candidates(CandidateID),
	   CONSTRAINT UQ_Applications UNIQUE (JobID, CandidateID) -- ✅ Không cho ứng viên ứng tuyển 2 lần cùn

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

UPDATE Users
SET Avatar = ISNULL(Avatar, '/images/avatar/default.jpg'),
    AvatarColor = ISNULL(AvatarColor, 
                         CONCAT('#',
                                RIGHT('000000' + CAST(CONVERT(VARBINARY(3), CHECKSUM(UserID)) AS VARCHAR(6)), 6))
                        );

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

-- Companies
-- Companies
INSERT INTO Companies (EmployerID, CategoryID,ProvinceId, CompanyName, Email, Description, Address, Website, Logo)
VALUES
(3, 1, 27,N'Công ty TNHH Phần mềm Sao Việt', 'contact@saoviet.com', N'Chuyên phát triển phần mềm, dịch vụ IT.', N'Hà Nội', 'www.saoviet.com',
 '/image/logo/1.png'),

(4, 3,28, N'Công ty Cổ phần Thương mại Ánh Dương', 'hr@anhduong.vn', N'Chuyên phân phối hàng tiêu dùng.', N'TP.HCM', 'www.anhduong.vn',
 '/image/logo/3.png');



 -- =======================================
-- DỮ LIỆU MẪU CHO BẢNG JOBS
-- =======================================
INSERT INTO Jobs (
    EmployerID, CompanyID, CategoryID, ProvinceId,
    Title, Description, Requirements, Benefits,
    Location, Salary, JobType, GenderRequirement, VacancyCount,
    ExpireDate, IsApproved, Keywords, ImageUrl
)
VALUES
-- 1️⃣ Lập trình viên .NET
(3, 1, 1, 28, 
 N'Lập trình viên .NET', 
 N'Phát triển ứng dụng web ASP.NET Core, SQL Server.', 
 N'Tốt nghiệp CNTT, biết C#, SQL', 
 N'Lương thưởng hấp dẫn, BHXH đầy đủ', 
 N'Quận 12', 
 N'13 - 15 triệu', 
 N'Full-time', 
 N'Không yêu cầu', 
 2, 
 '2025-12-31', 
 1,N'Lập trình, net','/image/job/3.jpg'),

-- 2️⃣ Tester phần mềm
(3, 1, 1, 28, 
 N'Tester phần mềm', 
 N'Thực hiện kiểm thử, viết test case.', 
 N'Có kiến thức kiểm thử, ưu tiên biết Selenium', 
 N'Môi trường trẻ trung, năng động', 
 N'Phường 3', 
 N'10 - 13 triệu', 
 N'Part-time', 
 N'Nữ', 
 1, 
 '2025-11-30', 
 1,'','/image/job/4.png'),

-- 3️⃣ Nhân viên kinh doanh
(4, 2, 3, 27, 
 N'Nhân viên kinh doanh', 
 N'Tìm kiếm khách hàng, tư vấn sản phẩm.', 
 N'Kỹ năng giao tiếp tốt', 
 N'Hoa hồng cao, du lịch hằng năm', 
 N'Quận Hai Bà Trưng', 
 N'8 - 10 triệu', 
 N'Full-time', 
 N'Nam', 
 3, 
 '2025-10-31', 
 1,N'Kinh doanh, sale','/image/job/2.jpg'),

-- 4️⃣ Kế toán tổng hợp
(4, 2, 4, 27, 
 N'Kế toán tổng hợp', 
 N'Quản lý sổ sách, báo cáo tài chính.', 
 N'Tốt nghiệp kế toán, biết Excel nâng cao', 
 N'Chế độ nghỉ phép + bảo hiểm', 
 N'Quận Cầu Giấy', 
 N'10 - 13 triệu', 
 N'Part-time', 
 N'Nữ', 
 1, 
 '2025-09-30', 
 1,'','/image/job/5.jpg');
GO


-- =======================================
-- DỮ LIỆU MẪU CHO BẢNG APPLICATIONS
-- =======================================
--INSERT INTO Applications (JobID, CandidateID, Status)
--VALUES
---- Ứng viên Nguyễn Văn A ứng tuyển vào Lập trình viên .NET
--(1, 1, N'Pending'),

---- Ứng viên Trần Thị B ứng tuyển vào Tester phần mềm
--(2, 2, N'Approved'),

---- Ứng viên Nguyễn Văn A ứng tuyển vào Nhân viên kinh doanh
--(3, 1, N'Rejected');



-- SavedJobs
INSERT INTO SavedJobs (CandidateID, JobID)
VALUES
(1, 2),
(1, 3),
(2, 1);





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

-- Admin gửi thông báo cho Candidate
--INSERT INTO Notifications (UserId, Message)
--VALUES
--(1, N'Hồ sơ của bạn đã được duyệt bởi Admin.'),
--(2, N'Bạn có công việc mới phù hợp với kỹ năng Kế toán.');

---- Admin gửi thông báo cho Employer
--INSERT INTO Notifications (UserId, Message)
--VALUES
--(3, N'Tin tuyển dụng "Lập trình viên .NET" đã được phê duyệt.'),
--(4, N'Vui lòng bổ sung giấy phép kinh doanh cho công ty của bạn.');

ALTER TABLE Notifications
ADD Type NVARCHAR(50) NULL,
    Link NVARCHAR(255) NULL;



-- Gửi thông báo cho Employer của Job đó
INSERT INTO Notifications (UserId, Message, Type, Link)
SELECT j.EmployerID,
       N'Ứng viên mới vừa ứng tuyển vào công việc: ' + j.Title,
       'Application',
       '/Employer/Applications'
FROM Jobs j
WHERE j.JobID = 1;


UPDATE Applications
SET Status = CASE Status
    WHEN 'Pending' THEN N'Chờ duyệt'
    WHEN 'Approved' THEN N'Chấp nhận'
    WHEN 'Rejected' THEN N'Từ chối'
END;

ALTER TABLE Applications
ADD RejectReason NVARCHAR(MAX) NULL;


ALTER TABLE Users
ADD CONSTRAINT CK_Users_Role CHECK (
    Role IN ('Candidate', 'Employer', 'Admin', 'SubAdmin')
);


CREATE TABLE CareerGuide (
    GuideID INT IDENTITY PRIMARY KEY,         -- Khóa chính
    Title NVARCHAR(255) NOT NULL,             -- Tiêu đề bài viết
    Description NVARCHAR(500),                -- Mô tả ngắn
    Content NVARCHAR(MAX) NOT NULL,           -- Nội dung chính
    Image NVARCHAR(255),                      -- Ảnh minh họa
    Author NVARCHAR(100),                     -- Tác giả
    PostedDate DATETIME DEFAULT GETDATE(),    -- Ngày đăng
    Category NVARCHAR(100),                   -- Danh mục (VD: Writing CV, Interview Tips)
    Views INT DEFAULT 0,                      -- Lượt xem
    IsActive BIT DEFAULT 1                    -- 1: hiển thị, 0: ẩn
);

INSERT INTO CareerGuide (Title, Description, Content, Image, Author, PostedDate, Category, Views, IsActive)
VALUES
-- 📝 1. Viết CV chuyên nghiệp cùng Jobify
(N'Cách viết CV chuyên nghiệp gây ấn tượng với nhà tuyển dụng',
 N'Hướng dẫn chi tiết giúp bạn tạo CV nổi bật, thể hiện rõ năng lực và mục tiêu nghề nghiệp.',
 N'Một CV chuyên nghiệp là “tấm vé” đầu tiên giúp bạn bước vào vòng phỏng vấn. Hãy trình bày ngắn gọn, rõ ràng, tập trung vào kỹ năng và thành tựu nổi bật. Đừng quên điều chỉnh CV cho phù hợp với từng vị trí công việc cụ thể.',
 N'/image/camnang/Online learning-pana.svg',
 N'Đội ngũ Jobify',
 GETDATE(),
 N'Viết CV',
 420,
 1),

-- 💬 2. Mẹo phỏng vấn giúp bạn chinh phục nhà tuyển dụng
(N'10 bí quyết giúp bạn tự tin trong buổi phỏng vấn',
 N'Tổng hợp những kinh nghiệm thực tế giúp bạn ghi điểm với nhà tuyển dụng.',
 N'Trước khi đến phỏng vấn, hãy tìm hiểu kỹ về công ty và vị trí ứng tuyển. Giữ thái độ bình tĩnh, tự tin, và trả lời trung thực. Ngoài ra, hãy chuẩn bị câu hỏi để thể hiện sự quan tâm của bạn đến công ty.',
 N'/image/camnang/Team goals-pana.svg',
 N'Chuyên gia Jobify',
 GETDATE(),
 N'Phỏng vấn',
 315,
 1),

-- 💡 3. 5 kỹ năng mềm không thể thiếu trong môi trường làm việc hiện đại
(N'5 kỹ năng mềm giúp bạn nổi bật trong công việc',
 N'Phát triển kỹ năng mềm là yếu tố then chốt giúp bạn thăng tiến nhanh hơn.',
 N'Kỹ năng giao tiếp, làm việc nhóm, quản lý thời gian, tư duy phản biện và thích nghi nhanh là những năng lực được nhà tuyển dụng đánh giá cao. Đầu tư vào việc trau dồi kỹ năng mềm sẽ giúp bạn trở thành ứng viên sáng giá trong mọi môi trường.',
 N'/image/camnang/Questions-pana.svg',
 N'Trung tâm Phát triển Nghề nghiệp Jobify',
 GETDATE(),
 N'Kỹ năng mềm',
 260,
 1),

-- 📄 4. Viết thư xin việc như thế nào để nổi bật?
(N'Bí quyết viết thư xin việc thuyết phục nhà tuyển dụng',
 N'Thư xin việc là “vũ khí” giúp bạn tạo ấn tượng đầu tiên mạnh mẽ.',
 N'Hãy viết ngắn gọn, tập trung vào điểm mạnh của bạn và thể hiện sự phù hợp với công việc. Sử dụng ngôn từ tự nhiên, tránh sáo rỗng và kết thúc bằng lời cảm ơn chân thành.',
 N'/image/camnang/Processing-pana.svg',
 N'Jobify Career Blog',
 GETDATE(),
 N'Viết CV',
 240,
 1),

-- 🌍 5. Xây dựng thương hiệu cá nhân trên LinkedIn
(N'Cách xây dựng hồ sơ LinkedIn chuyên nghiệp',
 N'Tối ưu hồ sơ LinkedIn để tạo dấu ấn cá nhân và thu hút nhà tuyển dụng.',
 N'Hồ sơ LinkedIn là công cụ mạnh mẽ giúp bạn kết nối với nhà tuyển dụng và đồng nghiệp. Hãy chọn ảnh đại diện chuyên nghiệp, viết tiêu đề rõ ràng và chia sẻ thành tựu của mình. Đăng bài thường xuyên để tăng độ nhận diện thương hiệu cá nhân.',
 N'/image/camnang/Processing-pana.svg',
 N'Team Jobify',
 GETDATE(),
 N'Phát triển cá nhân',
 290,
 1),

-- 🚀 6. Đặt mục tiêu nghề nghiệp rõ ràng để phát triển bền vững
(N'Đặt mục tiêu nghề nghiệp như thế nào cho hiệu quả?',
 N'Đặt mục tiêu giúp bạn định hướng con đường sự nghiệp rõ ràng và có động lực hơn.',
 N'Bạn nên xác định mục tiêu ngắn hạn và dài hạn theo mô hình SMART: Cụ thể, Đo lường được, Có thể đạt được, Thực tế và Có thời hạn. Việc theo dõi tiến trình sẽ giúp bạn chủ động điều chỉnh chiến lược phát triển cá nhân.',
 N'/image/camnang/Business Plan-pana.svg',
 N'Jobify Coaching Center',
 GETDATE(),
 N'Định hướng nghề nghiệp',
 175,
 1),

-- ⏰ 7. Quản lý thời gian – Kỹ năng vàng của người thành công
(N'Bí quyết quản lý thời gian hiệu quả trong công việc',
 N'Kỹ năng quản lý thời gian giúp bạn làm việc năng suất và cân bằng cuộc sống.',
 N'Học cách sắp xếp công việc theo mức độ ưu tiên, sử dụng lịch và công cụ nhắc việc. Dành thời gian cho việc nghỉ ngơi, đọc sách hoặc tập thể dục để duy trì tinh thần và hiệu suất cao nhất.',
 N'/image/camnang/Creative team-pana.svg',
 N'Jobify Editors',
 GETDATE(),
 N'Kỹ năng làm việc',
 205,
 1);


 ALTER TABLE CareerGuide ADD DisplayOrder INT NULL;
UPDATE CareerGuide SET DisplayOrder = GuideID; -- gán tạm


-- =======================================
-- BẢNG CV CHÍNH (Hồ sơ ứng viên)
-- =======================================
CREATE TABLE CVs (
    CVID INT IDENTITY PRIMARY KEY,
    CandidateID INT NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100),
    Phone NVARCHAR(20),
    Address NVARCHAR(255),
    DateOfBirth DATE NULL,
    Gender NVARCHAR(10) NULL,
    CareerGoal NVARCHAR(MAX),
    Avatar NVARCHAR(255),                      -- Ảnh đại diện
    FilePath NVARCHAR(255),                    -- Đường dẫn file PDF upload
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CandidateID) REFERENCES Candidates(CandidateID)
);
GO

-- =======================================
-- BẢNG KỸ NĂNG (CV Skills)
-- =======================================
CREATE TABLE CvSkills (
    SkillID INT IDENTITY PRIMARY KEY,
    CVID INT NOT NULL,
    SkillName NVARCHAR(100) NOT NULL,
    SkillLevel NVARCHAR(50) NULL,              -- ví dụ: Cơ bản, Khá, Giỏi
    FOREIGN KEY (CVID) REFERENCES CVs(CVID)
);
GO

-- =======================================
-- BẢNG KINH NGHIỆM (CV Experiences)
-- =======================================
CREATE TABLE CvExperiences (
    ExpID INT IDENTITY PRIMARY KEY,
    CVID INT NOT NULL,
    Position NVARCHAR(150) NOT NULL,           -- Vị trí
    CompanyName NVARCHAR(150),
    StartDate DATE,
    EndDate DATE,
    Description NVARCHAR(MAX),
    FOREIGN KEY (CVID) REFERENCES CVs(CVID)
);
GO

-- =======================================
-- BẢNG HỌC VẤN (CV Education)
-- =======================================
CREATE TABLE CvEducations (
    EduID INT IDENTITY PRIMARY KEY,
    CVID INT NOT NULL,
    SchoolName NVARCHAR(150) NOT NULL,
    Major NVARCHAR(100),
    StartYear INT,
    EndYear INT,
    Description NVARCHAR(MAX),
    FOREIGN KEY (CVID) REFERENCES CVs(CVID)
);
GO

-- =======================================
-- BẢNG CHỨNG CHỈ (CV Certificates) – tuỳ chọn
-- =======================================
CREATE TABLE CvCertificates (
    CertID INT IDENTITY PRIMARY KEY,
    CVID INT NOT NULL,
    CertificateName NVARCHAR(150) NOT NULL,
    Organization NVARCHAR(150),
    Year INT,
    FOREIGN KEY (CVID) REFERENCES CVs(CVID)
);
GO

-- =======================================
-- DỮ LIỆU MẪU CHO CV
-- =======================================
INSERT INTO CVs (CandidateID, FullName, Email, Phone, Address, DateOfBirth, Gender, CareerGoal, Avatar, FilePath)
VALUES
(1, N'Nguyễn Văn A', 'vana@gmail.com', '0912345678', N'Hà Nội', '1995-05-12', N'Nam',
 N'Mong muốn trở thành Senior Developer, đóng góp vào các dự án phần mềm quy mô lớn.',
 '/images/avatar/vana.jpg', '/cvs/cv_nguyenvana.pdf'),

(2, N'Trần Thị B', 'thib@gmail.com', '0987654321', N'TP.HCM', '1997-08-20', N'Nữ',
 N'Phát triển sự nghiệp trong lĩnh vực kế toán - tài chính chuyên nghiệp.',
 '/images/avatar/tranthib.jpg', '/cvs/cv_tranthib.docx');

-- Kỹ năng
INSERT INTO CvSkills (CVID, SkillName, SkillLevel)
VALUES
(1, N'C#', N'Thành thạo'),
(1, N'SQL Server', N'Tốt'),
(1, N'ASP.NET Core', N'Tốt'),
(2, N'Excel', N'Thành thạo'),
(2, N'Phân tích tài chính', N'Tốt');

-- Kinh nghiệm
INSERT INTO CvExperiences (CVID, Position, CompanyName, StartDate, EndDate, Description)
VALUES
(1, N'Lập trình viên .NET', N'Công ty Sao Việt', '2022-01-01', '2024-10-01',
 N'Tham gia phát triển hệ thống quản lý nhân sự bằng ASP.NET Core và SQL Server.'),
(2, N'Kế toán tổng hợp', N'Công ty Ánh Dương', '2023-01-01', '2024-10-01',
 N'Lập báo cáo tài chính, phân tích số liệu và tối ưu chi phí.');

-- Học vấn
INSERT INTO CvEducations (CVID, SchoolName, Major, StartYear, EndYear, Description)
VALUES
(1, N'Đại học Bách khoa Hà Nội', N'Công nghệ thông tin', 2013, 2017, N'Tốt nghiệp loại Khá.'),
(2, N'Đại học Kinh tế TP.HCM', N'Kế toán tài chính', 2014, 2018, N'Tốt nghiệp loại Giỏi.');

-- Chứng chỉ
INSERT INTO CvCertificates (CVID, CertificateName, Organization, Year)
VALUES
(1, N'Microsoft Certified: C# Developer', N'Microsoft', 2023),
(2, N'Chứng chỉ Kế toán trưởng', N'Học viện Tài chính', 2024);
GO

-- =======================================
-- 11. Bảng JobComments (Bình luận công việc)
-- =======================================
CREATE TABLE JobComments (
    CommentID INT IDENTITY PRIMARY KEY,      
    JobID INT NOT NULL,                       
    UserID INT NOT NULL,                       
    CommentText NVARCHAR(MAX) NOT NULL,        
    CreatedAt DATETIME DEFAULT GETDATE(),      
    FOREIGN KEY (JobID) REFERENCES Jobs(JobID),
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- JobComments
INSERT INTO JobComments (JobID, UserID, CommentText)
VALUES
(1, 1, N'Tôi rất quan tâm công việc này, hy vọng có cơ hội phỏng vấn.'),
(1, 2, N'Công việc phù hợp với kỹ năng của tôi.'),
(3, 1, N'Tin tuyển dụng cần rõ ràng hơn về chế độ đãi ngộ.');

