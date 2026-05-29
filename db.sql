CREATE DATABASE JobPtit;
GO

USE [JobPtit];
GO

--- 3. TẠO CÁC BẢNG QUẢN LÝ NGƯỜI DÙNG (ASP.NET IDENTITY)
CREATE TABLE [dbo].[AspNetUsers](
    [Id] [nvarchar](450) NOT NULL PRIMARY KEY,
    [UserName] [nvarchar](256) NULL,
    [NormalizedUserName] [nvarchar](256) NULL,
    [Email] [nvarchar](256) NULL,
    [NormalizedEmail] [nvarchar](256) NULL,
    [EmailConfirmed] [bit] NOT NULL,
    [PasswordHash] [nvarchar](max) NULL,
    [SecurityStamp] [nvarchar](max) NULL,
    [ConcurrencyStamp] [nvarchar](max) NULL,
    [PhoneNumber] [nvarchar](max) NULL,
    [PhoneNumberConfirmed] [bit] NOT NULL,
    [TwoFactorEnabled] [bit] NOT NULL,
    [LockoutEnd] [datetimeoffset](7) NULL,
    [LockoutEnabled] [bit] NOT NULL,
    [AccessFailedCount] [int] NOT NULL,
    [FullName] [nvarchar](256) NULL
);

CREATE TABLE [dbo].[AspNetRoles](
    [Id] [nvarchar](450) NOT NULL PRIMARY KEY,
    [Name] [nvarchar](256) NULL,
    [NormalizedName] [nvarchar](256) NULL,
    [ConcurrencyStamp] [nvarchar](max) NULL
);

CREATE TABLE [dbo].[AspNetUserRoles](
    [UserId] [nvarchar](450) NOT NULL,
    [RoleId] [nvarchar](450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId])
);

CREATE TABLE [dbo].[AspNetUserClaims](
    [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [UserId] [nvarchar](450) NOT NULL,
    [ClaimType] [nvarchar](max) NULL,
    [ClaimValue] [nvarchar](max) NULL
);

CREATE TABLE [dbo].[AspNetRoleClaims](
    [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [RoleId] [nvarchar](450) NOT NULL,
    [ClaimType] [nvarchar](max) NULL,
    [ClaimValue] [nvarchar](max) NULL
);

CREATE TABLE [dbo].[AspNetUserLogins](
    [LoginProvider] [nvarchar](450) NOT NULL,
    [ProviderKey] [nvarchar](450) NOT NULL,
    [ProviderDisplayName] [nvarchar](max) NULL,
    [UserId] [nvarchar](450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey])
);

CREATE TABLE [dbo].[AspNetUserTokens](
    [UserId] [nvarchar](450) NOT NULL,
    [LoginProvider] [nvarchar](450) NOT NULL,
    [Name] [nvarchar](450) NOT NULL,
    [Value] [nvarchar](max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name])
);

--- 4. TẠO CÁC BẢNG DANH MỤC VÀ KỸ NĂNG
CREATE TABLE [dbo].[Categories](
    [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] [nvarchar](255) NOT NULL,
    [Slug] [nvarchar](255) NOT NULL UNIQUE,
    [CreatedAt] [datetime2](7) NOT NULL DEFAULT (GETDATE())
);

CREATE TABLE [dbo].[Skills](
    [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] [nvarchar](150) NOT NULL UNIQUE
);

--- 5. TẠO CÁC BẢNG DOANH NGHIỆP VÀ CÔNG VIỆC
CREATE TABLE [dbo].[Companies](
    [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] [nvarchar](255) NOT NULL,
    [Location] [nvarchar](255) NULL,
    [LogoUrl] [nvarchar](500) NULL,
    [Description] [nvarchar](max) NULL,
    [OwnerUserId] [nvarchar](450) NOT NULL UNIQUE,
    [CreatedAt] [datetime2](7) NOT NULL DEFAULT (GETDATE()),
    [IsVerified] [bit] NOT NULL DEFAULT (0),
    [WebsiteUrl] [nvarchar](500) NULL,
    [Email] [nvarchar](500) NULL,
    [PhoneNumber] [nvarchar](500) NULL
);

CREATE TABLE [dbo].[Jobs](
    [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Title] [nvarchar](255) NOT NULL,
    [Description] [nvarchar](max) NULL,
    [Location] [nvarchar](255) NULL,
    [SalaryMin] [int] NULL,
    [SalaryMax] [int] NULL,
    [IsNegotiable] [bit] NOT NULL DEFAULT (0),
    [JobType] [int] NOT NULL,
    [Status] [int] NOT NULL DEFAULT (1),
    [CategoryId] [int] NOT NULL,
    [CompanyId] [int] NOT NULL,
    [ViewsCount] [int] NOT NULL DEFAULT (0),
    [CreatedAt] [datetime2](7) NOT NULL DEFAULT (GETDATE()),
    [ExpiredAt] [datetime2](7) NULL
);

--- 6. TẠO CÁC BẢNG HỒ SƠ ỨNG VIÊN
CREATE TABLE [dbo].[CandidateProfiles](
    [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [UserId] [nvarchar](450) NOT NULL UNIQUE,
    [FullName] [nvarchar](255) NOT NULL,
    [Title] [nvarchar](255) NULL,
    [Phone] [nvarchar](50) NULL,
    [Location] [nvarchar](255) NULL,
    [AboutMe] [nvarchar](max) NULL,
    [AvatarUrl] [nvarchar](500) NULL,
    [CVUrl] [nvarchar](500) NULL,
    [Email] [nvarchar](50) NULL
);

CREATE TABLE [dbo].[CandidateProfileSkills](
    [CandidateProfileId] [int] NOT NULL,
    [SkillId] [int] NOT NULL,
    PRIMARY KEY ([CandidateProfileId], [SkillId])
);

CREATE TABLE [dbo].[Educations](
    [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [CandidateProfileId] [int] NOT NULL,
    [SchoolName] [nvarchar](255) NOT NULL,
    [Degree] [nvarchar](255) NULL,
    [StartDate] [date] NOT NULL,
    [EndDate] [date] NULL
);

CREATE TABLE [dbo].[WorkExperiences](
    [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [CandidateProfileId] [int] NOT NULL,
    [CompanyName] [nvarchar](255) NOT NULL,
    [Position] [nvarchar](255) NOT NULL,
    [StartDate] [date] NOT NULL,
    [EndDate] [date] NULL,
    [Description] [nvarchar](max) NULL
);

--- 7. TẠO BẢNG ỨNG TUYỂN (APPLICATIONS)
CREATE TABLE [dbo].[Applications](
    [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [UserId] [nvarchar](450) NOT NULL,
    [JobId] [int] NOT NULL,
    [Status] [nvarchar](50) NOT NULL,
    [AppliedAt] [datetime2](7) NULL DEFAULT (GETDATE()),
    [AIScore] [int] NULL,
    [AIStrengths] [nvarchar](max) NULL,
    [AIWeaknesses] [nvarchar](max) NULL,
    [AIReasoning] [nvarchar](max) NULL,
    CONSTRAINT [UQ_User_Job] UNIQUE ([UserId], [JobId])
);

--- 8. THIẾT LẬP KHÓA NGOẠI (FOREIGN KEYS)
-- Identity
ALTER TABLE [dbo].[AspNetUserRoles] ADD CONSTRAINT [FK_UserRoles_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[AspNetUserRoles] ADD CONSTRAINT [FK_UserRoles_Roles] FOREIGN KEY([RoleId]) REFERENCES [dbo].[AspNetRoles]([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[AspNetUserClaims] ADD CONSTRAINT [FK_UserClaims_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[AspNetRoleClaims] ADD CONSTRAINT [FK_RoleClaims_Roles] FOREIGN KEY([RoleId]) REFERENCES [dbo].[AspNetRoles]([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[AspNetUserLogins] ADD CONSTRAINT [FK_UserLogins_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[AspNetUserTokens] ADD CONSTRAINT [FK_UserTokens_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE;

-- Business Logic
ALTER TABLE [dbo].[CandidateProfiles] ADD CONSTRAINT [FK_Profile_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[Companies] ADD CONSTRAINT [FK_Company_Users] FOREIGN KEY([OwnerUserId]) REFERENCES [dbo].[AspNetUsers]([Id]);
ALTER TABLE [dbo].[Jobs] ADD CONSTRAINT [FK_Jobs_Categories] FOREIGN KEY([CategoryId]) REFERENCES [dbo].[Categories]([Id]);
ALTER TABLE [dbo].[Jobs] ADD CONSTRAINT [FK_Jobs_Companies] FOREIGN KEY([CompanyId]) REFERENCES [dbo].[Companies]([Id]);
ALTER TABLE [dbo].[Applications] ADD CONSTRAINT [FK_Apps_Jobs] FOREIGN KEY([JobId]) REFERENCES [dbo].[Jobs]([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[Applications] ADD CONSTRAINT [FK_Apps_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[Educations] ADD CONSTRAINT [FK_Edu_Profile] FOREIGN KEY([CandidateProfileId]) REFERENCES [dbo].[CandidateProfiles]([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[WorkExperiences] ADD CONSTRAINT [FK_Work_Profile] FOREIGN KEY([CandidateProfileId]) REFERENCES [dbo].[CandidateProfiles]([Id]) ON DELETE CASCADE;
GO