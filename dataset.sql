INSERT INTO AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail,
    EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
    PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount
)
VALUES
('user-candidate-1', 'candidate1', 'CANDIDATE1', 'candidate1@gmail.com', 'CANDIDATE1@GMAIL.COM',
1, 'AQAAAA...', NEWID(), NEWID(), 0, 0, 0, 0),

('user-recruiter-1', 'recruiter1', 'RECRUITER1', 'recruiter1@fpt.com', 'RECRUITER1@FPT.COM',
1, 'AQAAAA...', NEWID(), NEWID(), 0, 0, 0, 0),

('user-recruiter-2', 'recruiter2', 'RECRUITER2', 'recruiter2@techcorp.com', 'RECRUITER2@TECHCORP.COM',
1, 'AQAAAA...', NEWID(), NEWID(), 0, 0, 0, 0);

INSERT INTO CandidateProfiles
(UserId, FullName, Title, Phone, Location, AboutMe, AvatarUrl, CVUrl)
VALUES
('user-candidate-1', 
 N'Nguyễn Văn A',
 N'Java Developer',
 '0901234567',
 N'Hà Nội',
 N'2 năm kinh nghiệm Java Spring Boot',
 'https://randomuser.me/api/portraits/men/1.jpg',
 'https://example.com/cv1.pdf');

 INSERT INTO Skills (Name)
VALUES
('Java'),
('Spring Boot'),
('SQL'),
('React'),
('Docker');

INSERT INTO WorkExperiences
(CandidateProfileId, CompanyName, Position, StartDate, EndDate, Description)
VALUES
(1, 'FPT Software', 'Junior Java Developer', '2022-01-01', '2024-01-01',
 N'Phát triển hệ thống quản lý nội bộ bằng Spring Boot');

 INSERT INTO Educations
(CandidateProfileId, SchoolName, Degree, StartDate, EndDate)
VALUES
(1, N'Đại học Bách Khoa Hà Nội', N'Công nghệ thông tin',
 '2018-09-01', '2022-06-01');

 INSERT INTO Companies
(Name, Location, LogoUrl, Description, OwnerUserId)
VALUES
('FPT Software', N'Hà Nội',
 'https://upload.wikimedia.org/wikipedia/commons/2/29/FPT_Software_Logo.png',
 N'Công ty phần mềm hàng đầu Việt Nam',
 'user-recruiter-1'),

('TechCorp Vietnam', N'TP Hồ Chí Minh',
 'https://via.placeholder.com/150',
 N'Công ty startup công nghệ',
 'user-recruiter-2');

 INSERT INTO Categories (Name, Slug)
VALUES
('IT', 'it'),
('Marketing', 'marketing'),
('Design', 'design');

INSERT INTO Jobs
(Title, Description, Location, SalaryMin, SalaryMax, IsNegotiable,
 JobType, Status, CategoryId, CompanyId, CreatedByUserId, ExpiredAt)
VALUES

(N'Lập trình viên Java (Java Developer)',
 N'Phát triển hệ thống backend sử dụng Spring Boot',
 N'Hà Nội',
 15000000, 25000000, 0,
 1, 1, 1, 1, 'user-recruiter-1',
 '2026-03-30'),

(N'React Frontend Developer',
 N'Xây dựng giao diện SPA',
 N'Hà Nội',
 12000000, 20000000, 0,
 1, 1, 1, 1, 'user-recruiter-1',
 '2026-04-15'),

(N'DevOps Engineer',
 N'Quản lý CI/CD và Docker',
 N'TP Hồ Chí Minh',
 20000000, 35000000, 1,
 1, 1, 1, 2, 'user-recruiter-2',
 '2026-05-01');

 INSERT INTO Applications
(UserId, JobId, Status)
VALUES
('user-candidate-1', 1, 0),
('user-candidate-1', 2, 1);