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

--     SELECT CAST(SCOPE_IDENTITY() AS INT) -- will select the ID for the newly inserted row (RETURN)

-- GO

-- CREATE PROCEDURE dbo.Blog_Delete
--     @BlogId INT
-- AS
--     UPDATE dbo.BlogComment
--     SET ActiveInd = CONVERT(BIT,0)
--     WHERE BlogId = @BlogId;

--     UPDATE dbo.Blog
--     SET PhotoId = NULL,
--         ActiveInd = CONVERT(BIT,0)
--     WHERE BlogId = @BlogId;

-- GO

-- CREATE PROCEDURE [aggregate].Blog_Get
--     @BlogId INT
-- AS
--     SELECT
--         [BlogId]
--         ,[ApplicationUserId]
--         ,[PhotoId]
--         ,[Title]
--         ,[Content]
--         ,[PublishDate]
--         ,[UpdateDate]
--     FROM
--         [aggregate].Blog
--     WHERE
--         BlogId = @BlogId AND ActiveInd = CONVERT(BIT,1)

-- GO

-- CREATE PROCEDURE [aggregate].Blog_GetAll
--     @Offset INT,
--     @PageSize INT
-- AS
--     SELECT
--         [BlogId]
--         ,[ApplicationUserId]
--         ,[Username]
--         ,[Title]
--         ,[Content]
--         ,[PhotoId]
--         ,[PublishDate]
--         ,[UpdateDate]
--     FROM
--         [BlogDB].[aggregate].[Blog]
--     WHERE
--         ActiveInd = CONVERT(BIT,1)
--     ORDER BY
--         BlogId
--     OFFSET @Offset ROWS --- where the query should start
--     FETCH NEXT @PageSize ROWS ONLY; -- continue from here

--     SELECT COUNT(*) FROM [aggregate].Blog t1 WHERE t1.[ActiveInd] = CONVERT(BIT,1)

-- CREATE PROCEDURE [dbo].Blog_GetAllFamous

-- AS
--     SELECT TOP 6
-- 		t1.[BlogId]
-- 		,t1.[ApplicationUserId]
--         ,t1.[Username]
-- 		,t1.[PhotoId]
-- 		,t1.[Title]
-- 		,t1.[Content]
-- 		,t1.[PublishDate]
-- 		,t1.[UpdateDate]
-- 		,t1.[ActiveInd]
--     FROM
--         [aggregate].Blog t1
--     INNER JOIN
--         dbo.BlogComment t2 ON t1.BlogId = t2.BlogId
--     WHERE
--         t1.ActiveInd = CONVERT(BIT, 1)
--         AND
--         t2.ActiveInd = CONVERT(BIT, 1)
--     GROUP BY --- CREATES A KEY FOR COMPARISON
-- 		t1.[BlogId]
-- 		,t1.[ApplicationUserId]
--         ,t1.[Username]
-- 		,t1.[PhotoId]
-- 		,t1.[Title]
-- 		,t1.[Content]
-- 		,t1.[PublishDate]
-- 		,t1.[UpdateDate]
-- 		,t1.[ActiveInd]
--     ORDER BY
--         COUNT(t2.BlogCommentId) DESC

-- GO

-- ALTER PROCEDURE  dbo.blog_GetByUserId
--     @ApplicationUserId INT
-- AS
--     SELECT
--         BlogId,
--         ApplicationUserId,
--         Username,
--         Title,
--         Content,
--         PhotoId,
--         PublishDate,
--         UpdateDate
--     FROM
--         [aggregate].[Blog]
--     WHERE
--         ApplicationUserId = @ApplicationUserId AND
--         ActiveInd = CONVERT(BIT, 1)

-- GO

-- CREATE PROCEDURE dbo.Blog_Upsert
--     @Blog BlogType READONLY,
--     @ApplicationUserId INT
-- AS
--     MERGE INTO dbo.Blog TARGET
--     USING
--     (
--         SELECT
--             b.BlogId,
--             @ApplicationUserId [ApplicationUserId],
--             b.Title,
--             b.Content,
--             b.PhotoId
--         FROM
--             @Blog b
--     ) AS SOURCE
--     ON TARGET.BlogId = SOURCE.BlogId AND TARGET.ApplicationUserId = SOURCE.ApplicationUserId
--     WHEN MATCHED THEN
--         UPDATE SET
--             TARGET.Title = SOURCE.Title,
--             TARGET.Content = SOURCE.Content,
--             TARGET.PhotoId = SOURCE.PhotoId,
--             TARGET.UpdateDate = GETDATE()
--     WHEN NOT MATCHED BY TARGET THEN
--         INSERT
--         (
--             ApplicationUserId,
--             Title,
--             Content,
--             PhotoId
--         )
--         VALUES
--         (
--             SOURCE.ApplicationUserId,
--             SOURCE.Title,
--             SOURCE.Content,
--             SOURCE.PhotoId
--         );
    
--     SELECT CAST(SCOPE_IDENTITY() AS INT);

-- GO

-- CREATE PROCEDURE dbo.BlogComment_Delete
--     @BlogCommentId INT
-- AS
--     DROP TABLE IF EXISTS #BlogCommentsToBeDeleted;

--     WITH cte_blogComments AS (

--         SELECT
--             t1.BlogCommentId,
--             t1.ParentBlogCommentId
--         FROM
--             dbo.BlogComment t1
--         WHERE
--             t1.BlogCommentId = @BlogCommentId

--         UNION ALL

--             SELECT
--                 t2.BlogCommentId,
--                 t2.ParentBlogCommentId
--             FROM
--                 dbo.BlogComment t2
--                 INNER JOIN cte_blogComments t3
--                 ON t2.ParentBlogCommentId = t3.BlogCommentId
--     )
--     --- Recursion statement
--     SELECT 
--         BlogCommentId,
--         ParentBlogCommentId
--     INTO #BlogCommentsToBeDeleted -- copy all columns into a new table
--     FROM cte_blogComments t1;

--     UPDATE t1
--     SET
--         t1.[ActiveInd] = CONVERT(BIT,0),
--         t1.[UpdateDate] = GETDATE()
--     FROM
--         dbo.BlogComments t1
--         INNER JOIN #BlogCommentsToBeDeleted t2
--             ON t1.BlogcommentId = t2.BlogCommentId

-- GO

-- CREATE PROCEDURE dbo.BlogComment_Get
--     @BlogCommentId INT
-- AS
--     SELECT
-- 		[BlogCommentId]
-- 		,[ParentBlogCommentId]
-- 		,[BlogId]
-- 		,[ApplicationUserId]
--         ,[Username]
-- 		,[Content]
-- 		,[PublishDate]
-- 		,[UpdateDate]
-- 		,[ActiveInd]
--     FROM
--         [aggregate].BlogComment
--     WHERE
--         BlogCommentId = @BlogCommentId AND
--         ActiveInd = CONVERT(BIT, 1)

-- GO

-- CREATE PROCEDURE dbo.BlogComment_GetAll
--     @BlogId INT
-- AS
--     SELECT
-- 		[BlogCommentId]
-- 		,[ParentBlogCommentId]
-- 		,[BlogId]
-- 		,[ApplicationUserId]
--         ,[Username]
-- 		,[Content]
-- 		,[PublishDate]
-- 		,[UpdateDate]
-- 		,[ActiveInd]
--     FROM
--         [aggregate].BlogComment
--     WHERE
--         BlogId = @BlogId AND
--         ActiveInd = CONVERT(BIT, 1)
--     ORDER BY
--         UpdateDate DESC

-- GO

-- CREATE PROCEDURE dbo.BlogComment_Upsert
--     @BlogComment BlogCommentType READONLY,
--     @ApplicationUserId INT
-- AS
--     MERGE INTO dbo.BlogComment AS TARGET
--     USING
--     (
--         SELECT
--             BlogCommentId,
--             ParentBlogCommentId,
--             BlogId,
--             Content,
--             @ApplicationUserId AS [ApplicationUserId]
--         FROM
--             @BlogComment
--     ) AS SOURCE
--     ON TARGET.BlogId = SOURCE.BlogId AND TARGET.ApplicationUserId = @ApplicationUserId
--     WHEN MATCHED THEN
--         UPDATE SET
--             ParentBlogCommentId = SOURCE.ParentBlogCommentId,
--             Content = SOURCE.Content
--     WHEN NOT MATCHED BY TARGET THEN
--         INSERT
--         (
--                 [ParentBlogCommentId]
--                 ,[BlogId]
--                 ,[ApplicationUserId]
--                 ,[Content]
--         )
--         VALUES
--         (
--             SOURCE.ParentBlogCommentId,
--             SOURCE.BlogId,
--             SOURCE.ApplicationUserId,
--             SOURCE.Content
--         );

--     SELECT CAST(SCOPE_IDENTITY() AS INT)

-- GO

-- CREATE PROCEDURE dbo.Photo_Delete
--     @PhotoId INT
-- AS
--     DELETE FROM dbo.Photo WHERE PhotoId = @PhotoId

-- GO

-- CREATE PROCEDURE dbo.Photo_Get
--     @PhotoID INT
-- AS
--     SELECT
--         t1.[PhotoId]
-- 		,t1.[ApplicationUserId]
-- 		,t1.[PublicId]
-- 		,t1.[ImageUrl]
-- 		,t1.[Description]
-- 		,t1.[PublishDate]
-- 		,t1.[UpdateDate]
--     FROM
--         dbo.Photo t1
--     WHERE
--         t1.PhotoId = @PhotoId

-- GO

-- CREATE PROCEDURE dbo.Photo_GetByUserId
--     @ApplicationUserID INT
-- AS
--     SELECT
--         t1.[PhotoId]
-- 		,t1.[ApplicationUserId]
-- 		,t1.[PublicId]
-- 		,t1.[ImageUrl]
-- 		,t1.[Description]
-- 		,t1.[PublishDate]
-- 		,t1.[UpdateDate]
--     FROM
--         dbo.Photo t1
--     WHERE
--         t1.ApplicationUserId = @ApplicationUserID

-- GO

-- CREATE PROCEDURE dbo.Photo_Insert
--     @ApplicationUserID INT,
--     @Photo PhotoType READONLY
-- AS
--     INSERT INTO [BlogDB].[dbo].[Photo]
--     (
--             [ApplicationUserId]
--             ,[PublicId]
--             ,[ImageUrl]
--             ,[Description]
--     )
--     SELECT
--         @ApplicationUserID
--         ,[PublicId]
--         ,[ImageUrl]
--         ,[Description]
--     FROM
--         @Photo

--     SELECT CAST(SCOPE_IDENTITY() AS INT);
-- GO
