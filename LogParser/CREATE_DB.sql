--CREATE DATABASE [LogDatabase];
--GO
--DROP TABLE [Log];
CREATE TABLE [Log] (
    Id int PRIMARY KEY IDENTITY(1, 1),
    RequestDateTime DATETIME NOT NULL,
    Host NVARCHAR(255) NOT NULL,
    [Route] NVARCHAR(MAX),
    [QueryParameters] NVARCHAR(MAX),
    [ResultCode] INT NOT NULL,
    [ResponseSize] INT NOT NULL,
    [Geolocation] NVARCHAR(MAX)
)