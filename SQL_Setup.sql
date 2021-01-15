-- CREATE DATABASE BlogDB

-- GO

-- CREATE TABLE ApplicationUser
-- (
--     ApplicationUserId INT NOT NULL IDENTITY(1,1),
--     Username VARCHAR(20) NOT NULL,
--     NormalizedUsername VARCHAR(20) NOT NULL, --- case (in)sensitive?
--     Email VARCHAR(30) NOT NULL,
--     NormalizedEmail VARCHAR(30) NOT NULL, --- case (in)sensitive?
--     Fullname VARCHAR(30) NULL,
--     PasswordHash NVARCHAR(MAX) NOT NULL, --- unicode chars as well
--     PRIMARY KEY (ApplicationUserId)
-- )

-- CREATE INDEX IX_ApplicationUser_NormalizedUsername ON dbo.ApplicationUser(NormalizedUsername)

-- CREATE INDEX IX_ApplicationUser_NormalizedEmail ON dbo.ApplicationUser(NormalizedEmail)

-- CREATE TABLE Photo
-- (
--     PhotoId INT NOT NULL IDENTITY (1,1),
--     ApplicationUserId INT NOT NULL,
--     PublicId VARCHAR(50) NOT NULL,
--     ImageUrl VARCHAR(250) NOT NULL,
--     [Description] VARCHAR (30) NOT NULL,
--     PublishDate DATETIME NOT NULL DEFAULT GETDATE(),
--     UpdateDate DATETIME NOT NULL DEFAULT GETDATE(),
--     PRIMARY KEY (PhotoId),
--     FOREIGN KEY (ApplicationUserId) REFERENCES ApplicationUser(ApplicationUserId)
-- )

-- CREATE TABLE Blog
-- (
--     BlogId INT NOT NULL IDENTITY(1,1),
--     ApplicationUserId INT NOT NULL,
--     PhotoId INT NULL,
--     Title VARCHAR(50) NOT NULL,
--     Content VARCHAR(MAX) NOT NULL,
--     PublishDate DATETIME NOT NULL DEFAULT GETDATE(),
--     UpdateDate DATETIME NOT NULL DEFAULT GETDATE(),
--     ActiveInd BIT NOT NULL DEFAULT CONVERT(BIT, 1),
--     PRIMARY KEY (BlogId),
--     FOREIGN KEY (ApplicationUserId) REFERENCES ApplicationUser(ApplicationUserId),
--     FOREIGN KEY (PhotoId) REFERENCES Photo(PhotoId),
-- )

-- CREATE TABLE BlogComment
-- (
--     BlogCommentId INT NOT NULL IDENTITY(1,1),
--     ParentBlogCommentId INT NOT NULL,
--     BlogId INT NOT NULL,
--     ApplicationUserId INT NOT NULL,
--     Content VARCHAR(300) NOT NULL,
--     PublishDate DATETIME NOT NULL DEFAULT GETDATE(),
--     UpdateDate DATETIME NOT NULL DEFAULT GETDATE(),
--     ActiveInd BIT NOT NULL DEFAULT CONVERT(BIT, 1),
--     PRIMARY KEY (BlogCommentId),
--     FOREIGN KEY (BlogId) REFERENCES Blog(BlogId),
--     FOREIGN KEY (ApplicationUserId) REFERENCES ApplicationUser(ApplicationUserId)
-- )

-- GO

-- CREATE SCHEMA [aggregate]

-- GO

-- CREATE VIEW [aggregate].Blog
-- AS
-- (
--     SELECT
--         b.BlogId,
--         b.ApplicationUserId,
--         au.Username, -- only entry from other table
--         b.Title,
--         b.Content,
--         b.PhotoId,
--         b.PublishDate,
--         b.UpdateDate,
--         b.ActiveInd
--     FROM
--         dbo.Blog b
--     INNER JOIN
--         dbo.ApplicationUser au ON b.ApplicationUserId = b.ApplicationUserId 
-- )

-- GO

-- CREATE VIEW [aggregate].BlogComment
-- AS
-- (
--     SELECT
--         t1.BlogCommentId,
--         t1.ParentBlogCommentId,
--         t1.BlogId,
--         t1.Content,
--         t2.ApplicationUserId,
--         t2.Username,
--         t1.PublishDate,
--         t1.UpdateDate,
--         t1.ActiveInd
--     FROM
--         dbo.BlogComment t1
--     INNER JOIN
--         dbo.ApplicationUser t2 ON t1.ApplicationUserId = t2.ApplicationUserId 
-- )

-- GO

-- CREATE TYPE AccountType AS TABLE
-- (
--     Username VARCHAR(20) NOT NULL,
--     NormalizedUsername VARCHAR(20) NOT NULL, 
--     Email VARCHAR(30) NOT NULL,
--     NormalizedEmail VARCHAR(30) NOT NULL,
--     Fullname VARCHAR(30) NULL,
--     PasswordHash NVARCHAR(MAX) NOT NULL
-- )

-- GO

-- CREATE TYPE dbo.PhotoType AS TABLE
-- (
--     PublicId VARCHAR(50) NOT NULL,
--     ImageUrl VARCHAR(250) NOT NULL,
--     [Description] VARCHAR (30) NOT NULL
-- )

-- GO

-- CREATE TYPE dbo.BlogType AS TABLE
-- (
--     BlogId INT NOT NULL,
--     PhotoId INT NULL,
--     Title VARCHAR(50) NOT NULL,
--     Content VARCHAR(MAX) NOT NULL
-- )

-- GO

-- CREATE TYPE dbo.BlogCommentType AS TABLE
-- (
--     BlogCommentId INT NOT NULL,
--     ParentBlogCommentId INT NULL,
--     BlogId INT NOT NULL,
--     Content VARCHAR(300) NOT NULL
-- )

-- GO

-- CREATE PROCEDURE dbo.Account_GetByUsername
--     @NormalizedUsername VARCHAR(20)
-- AS
-- (
--     SELECT
-- 		ApplicationUserId
-- 		,Username
-- 		,NormalizedUsername
-- 		,Email
-- 		,NormalizedEmail
-- 		,Fullname
-- 		,PasswordHash
--     FROM
--         dbo.ApplicationUser
--     WHERE
--         NormalizedUsername = @NormalizedUsername
-- )

-- GO

-- CREATE PROCEDURE dbo.Account_Insert
--     @Account AccountType READONLY
-- AS
--     INSERT INTO [dbo].[ApplicationUser]
--     (
--             [Username]
--             ,[NormalizedUsername]
--             ,[Email]
--             ,[NormalizedEmail]
--             ,[Fullname]
--             ,[PasswordHash]
--     )
--     SELECT
--         [Username]
--         ,[NormalizedUsername]
--         ,[Email]
--         ,[NormalizedEmail]
--         ,[Fullname]
--         ,[PasswordHash]
--     FROM
--         @Account

--     SELECT CAST(SCOPE_IDENTITY() AS INT)

-- GO