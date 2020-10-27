CREATE DATABASE [LogDatabase];
GO
USE [LogDatabase];
GO
CREATE TABLE [Log] (
    Id int PRIMARY KEY IDENTITY(1, 1),
    RequestDateTime DATETIME NOT NULL,
    Host NVARCHAR(255) NOT NULL,
    [Route] NVARCHAR(MAX),
    [QueryParameters] NVARCHAR(MAX),
    [ResultCode] INT NOT NULL,
    [ResponseSize] INT NOT NULL,
    [Geolocation] NVARCHAR(MAX)
);
GO
CREATE NONCLUSTERED INDEX [HostIndex] ON [dbo].[Log] ([Host]);
GO
CREATE NONCLUSTERED INDEX [RequestDateTimeIndex] ON [dbo].[Log] ([RequestDateTime]);