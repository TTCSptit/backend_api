CREATE DATABASE JobPtit;
GO

USE JobPtit;
GO

CREATE TABLE AspNetUsers (
    Id NVARCHAR(450) PRIMARY KEY,
    UserName NVARCHAR(256),
    NormalizedUserName NVARCHAR(256),
    Email NVARCHAR(256),
    NormalizedEmail NVARCHAR(256),
    EmailConfirmed BIT NOT NULL,
    PasswordHash NVARCHAR(MAX),
    SecurityStamp NVARCHAR(MAX),
    ConcurrencyStamp NVARCHAR(MAX),
    PhoneNumber NVARCHAR(MAX),
    PhoneNumberConfirmed BIT NOT NULL,
    TwoFactorEnabled BIT NOT NULL,
    LockoutEnd DATETIMEOFFSET,
    LockoutEnabled BIT NOT NULL,
    AccessFailedCount INT NOT NULL,
    FullName NVARCHAR(256)
);

CREATE TABLE AspNetRoles (
    Id NVARCHAR(450) PRIMARY KEY,
    Name NVARCHAR(256),
    NormalizedName NVARCHAR(256),
    ConcurrencyStamp NVARCHAR(MAX)
);

CREATE TABLE AspNetUserRoles (
    UserId NVARCHAR(450) NOT NULL,
    RoleId NVARCHAR(450) NOT NULL,
    PRIMARY KEY (UserId, RoleId),

    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
);

CREATE TABLE AspNetUserClaims (
    Id INT IDENTITY PRIMARY KEY,
    UserId NVARCHAR(450) NOT NULL,
    ClaimType NVARCHAR(MAX),
    ClaimValue NVARCHAR(MAX),

    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

CREATE TABLE AspNetRoleClaims (
    Id INT IDENTITY PRIMARY KEY,
    RoleId NVARCHAR(450) NOT NULL,
    ClaimType NVARCHAR(MAX),
    ClaimValue NVARCHAR(MAX),

    FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
);

CREATE TABLE AspNetUserLogins (
    LoginProvider NVARCHAR(450),
    ProviderKey NVARCHAR(450),
    ProviderDisplayName NVARCHAR(MAX),
    UserId NVARCHAR(450),

    PRIMARY KEY (LoginProvider, ProviderKey),

    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

CREATE TABLE AspNetUserTokens (
    UserId NVARCHAR(450),
    LoginProvider NVARCHAR(450),
    Name NVARCHAR(450),
    Value NVARCHAR(MAX),

    PRIMARY KEY (UserId, LoginProvider, Name),

    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

CREATE TABLE CandidateProfiles (
    Id INT IDENTITY PRIMARY KEY,
    UserId NVARCHAR(450) NOT NULL UNIQUE,
    FullName NVARCHAR(255) NOT NULL,
    Title NVARCHAR(255),
    Phone NVARCHAR(50),
    Location NVARCHAR(255),
    AboutMe NVARCHAR(MAX),
    AvatarUrl NVARCHAR(500),
    CVUrl NVARCHAR(500),

    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

CREATE TABLE Skills (
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(150) UNIQUE NOT NULL
);

CREATE TABLE CandidateSkills (
    CandidateProfileId INT,
    SkillId INT,

    PRIMARY KEY (CandidateProfileId, SkillId),

    FOREIGN KEY (CandidateProfileId)
        REFERENCES CandidateProfiles(Id) ON DELETE CASCADE,

    FOREIGN KEY (SkillId)
        REFERENCES Skills(Id) ON DELETE CASCADE
);

CREATE TABLE Educations (
    Id INT IDENTITY PRIMARY KEY,
    CandidateProfileId INT NOT NULL,
    SchoolName NVARCHAR(255) NOT NULL,
    Degree NVARCHAR(255),
    StartDate DATE NOT NULL,
    EndDate DATE,

    FOREIGN KEY (CandidateProfileId)
        REFERENCES CandidateProfiles(Id) ON DELETE CASCADE
);

CREATE TABLE WorkExperiences (
    Id INT IDENTITY PRIMARY KEY,
    CandidateProfileId INT NOT NULL,
    CompanyName NVARCHAR(255) NOT NULL,
    Position NVARCHAR(255) NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE,
    Description NVARCHAR(MAX),

    FOREIGN KEY (CandidateProfileId)
        REFERENCES CandidateProfiles(Id) ON DELETE CASCADE
);

CREATE TABLE Companies (
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    Location NVARCHAR(255),
    LogoUrl NVARCHAR(500),
    Description NVARCHAR(MAX),
    OwnerUserId NVARCHAR(450) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    IsVerified BIT DEFAULT 0,

    FOREIGN KEY (OwnerUserId)
        REFERENCES AspNetUsers(Id)
);

CREATE TABLE Categories (
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    Slug NVARCHAR(255) UNIQUE NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

CREATE TABLE Jobs (
    Id INT IDENTITY PRIMARY KEY,
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    Location NVARCHAR(255),
    SalaryMin INT,
    SalaryMax INT,
    IsNegotiable BIT DEFAULT 0,
    JobType INT NOT NULL,
    Status INT NOT NULL,

    CategoryId INT NOT NULL,
    CompanyId INT NOT NULL,
    CreatedByUserId NVARCHAR(450) NOT NULL,

    ViewsCount INT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    ExpiredAt DATETIME2,

    FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
    FOREIGN KEY (CompanyId) REFERENCES Companies(Id),
    FOREIGN KEY (CreatedByUserId) REFERENCES AspNetUsers(Id)
);

CREATE TABLE Applications (
    Id INT IDENTITY PRIMARY KEY,
    UserId NVARCHAR(450) NOT NULL,
    JobId INT NOT NULL,
    Status INT NOT NULL,
    AppliedAt DATETIME2 DEFAULT GETDATE(),

    CONSTRAINT UQ_User_Job UNIQUE(UserId, JobId),

    FOREIGN KEY (UserId)
        REFERENCES AspNetUsers(Id)
        ON DELETE CASCADE,

    FOREIGN KEY (JobId)
        REFERENCES Jobs(Id)
        ON DELETE CASCADE
);

CREATE INDEX IX_Jobs_CategoryId
ON Jobs(CategoryId);

CREATE INDEX IX_Jobs_CompanyId
ON Jobs(CompanyId);

CREATE INDEX IX_Jobs_Location
ON Jobs(Location);

CREATE INDEX IX_Jobs_Status
ON Jobs(Status);

CREATE INDEX IX_Applications_UserId
ON Applications(UserId);

CREATE INDEX IX_Applications_JobId
ON Applications(JobId);