IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250722085502_v1.0.1.0'
)
BEGIN
    CREATE TABLE [ApiVersions] (
        [Id] int NOT NULL IDENTITY,
        [Major] int NOT NULL,
        [Minor] int NOT NULL,
        [Patch] int NOT NULL,
        [GitCommit] nvarchar(max) NULL,
        [GitBranch] nvarchar(max) NULL,
        [BuildDate] datetime2 NOT NULL,
        [Platform] nvarchar(max) NULL,
        CONSTRAINT [PK_ApiVersions] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250722085502_v1.0.1.0'
)
BEGIN
    CREATE TABLE [HealthCheckResults] (
        [Id] int NOT NULL IDENTITY,
        [Status] nvarchar(max) NOT NULL,
        [Message] nvarchar(max) NULL,
        [Service] nvarchar(max) NOT NULL,
        [Timestamp] datetime2 NOT NULL,
        CONSTRAINT [PK_HealthCheckResults] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250722085502_v1.0.1.0'
)
BEGIN
    CREATE TABLE [Servers] (
        [Id] int NOT NULL IDENTITY,
        [CreatedAt] datetime2 NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [IPAddress] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_Servers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250722085502_v1.0.1.0'
)
BEGIN
    CREATE TABLE [ServerScopes] (
        [Id] int NOT NULL IDENTITY,
        [ServerId] int NOT NULL,
        [ScopeType] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_ServerScopes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ServerScopes_Servers_ServerId] FOREIGN KEY ([ServerId]) REFERENCES [Servers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250722085502_v1.0.1.0'
)
BEGIN
    CREATE TABLE [ServersOS] (
        [Id] int NOT NULL IDENTITY,
        [ServerId] int NOT NULL,
        [ServerOSType] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_ServersOS] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ServersOS_Servers_ServerId] FOREIGN KEY ([ServerId]) REFERENCES [Servers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250722085502_v1.0.1.0'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Servers_IPAddress] ON [Servers] ([IPAddress]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250722085502_v1.0.1.0'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Servers_Name] ON [Servers] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250722085502_v1.0.1.0'
)
BEGIN
    CREATE INDEX [IX_ServerScopes_ServerId] ON [ServerScopes] ([ServerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250722085502_v1.0.1.0'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ServersOS_ServerId] ON [ServersOS] ([ServerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250722085502_v1.0.1.0'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250722085502_v1.0.1.0', N'8.0.14');
END;
GO

COMMIT;
GO

