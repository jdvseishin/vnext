IF OBJECT_ID(N'[dbo].[Devices]', N'U') IS NULL 
BEGIN
    CREATE TABLE [dbo].[Devices]
    (
        [DeviceId] NVARCHAR(100) NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(MAX) NULL,
        [Location] NVARCHAR(MAX) NULL,
        [Type] NVARCHAR(MAX) NULL,
        [AssetId] NVARCHAR(MAX) NULL
    )
END
GO
