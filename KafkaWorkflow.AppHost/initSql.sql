-- SQL Server init script

-- Create the People database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'People')
BEGIN
  CREATE DATABASE People;
END;
GO

USE People;
GO

-- Create the Persons table
IF OBJECT_ID(N'Persons', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Persons](
	    [Id] [int] IDENTITY(1,1) NOT NULL,
	    [FirstName] [varchar](255) NOT NULL,
	    [LastName] [varchar](255) NOT NULL,
	    [Age] [int] NULL,
	    [Email] [varchar](255) NULL,
	    [Phone] [varchar](255) NULL,
	    [Address] [varchar](255) NULL,
    PRIMARY KEY CLUSTERED 
    (
	    [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
     CONSTRAINT [UQ_Persons_Email] UNIQUE NONCLUSTERED 
    (
	    [Id] ASC,
	    [Email] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END;
GO

IF OBJECT_ID(N'ContactInfos', N'U') IS NULL
BEGIN
    -- Create the ContactInfos table
    CREATE TABLE [dbo].[ContactInfos](
	    [Id] [int] IDENTITY(1,1) NOT NULL,
	    [PersonId] [int] NOT NULL,
	    [Phone] [varchar](255) NULL,
	    [Email] [varchar](255) NULL,
    PRIMARY KEY CLUSTERED 
    (
	    [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]

    ALTER TABLE [dbo].[ContactInfos]  WITH CHECK ADD  CONSTRAINT [FK_ContactInfos_Persons] FOREIGN KEY([PersonId])
    REFERENCES [dbo].[Persons] ([Id])
    ON DELETE CASCADE

    ALTER TABLE [dbo].[ContactInfos] CHECK CONSTRAINT [FK_ContactInfos_Persons]
END
GO

IF OBJECT_ID(N'Addresses', N'U') IS NULL
BEGIN
    -- Create the Addresses table
    CREATE TABLE [dbo].[Addresses](
	    [Id] [int] IDENTITY(1,1) NOT NULL,
	    [ContactInfoId] [int] NOT NULL,
	    [Street] [varchar](255) NULL,
	    [City] [varchar](255) NULL,
	    [State] [varchar](255) NULL,
	    [ZipCode] [varchar](255) NULL,
    PRIMARY KEY CLUSTERED 
    (
	    [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]

    ALTER TABLE [dbo].[Addresses]  WITH CHECK ADD  CONSTRAINT [FK_Addresses_ContactInfos] FOREIGN KEY([ContactInfoId])
    REFERENCES [dbo].[ContactInfos] ([Id])
    ON DELETE CASCADE

    ALTER TABLE [dbo].[Addresses] CHECK CONSTRAINT [FK_Addresses_ContactInfos]
END
GO


-- Insert some sample data into the Persons table
IF (SELECT COUNT(*) FROM Persons) = 0
BEGIN
    INSERT INTO Persons ([FirstName], [LastName], [Email], [Phone], [Address], [Age])
    VALUES
        ('John', 'Doe', 'john.doe@example.com', '555-123-4567', '123 Main St', 30),
        ('Jane', 'Doe', 'jane.doe@example.com', '555-234-5678', '456 Elm St', 25);

    INSERT INTO ContactInfos ([PersonId], [Phone], [Email])
    VALUES
        ((SELECT Id FROM Persons WHERE Email = 'john.doe@example.com'), '555-123-4567', 'email1@email.com'),
        ((SELECT Id FROM Persons WHERE Email = 'jane.doe@example.com'), '555-234-5678', 'email2@email.com');

    INSERT INTO Addresses ([ContactInfoId], [Street], [City], [State], [ZipCode])
    VALUES 
        ((SELECT Id FROM ContactInfos WHERE Email = 'email1@email.com'), '123 Main St', 'City1', 'State1', '12345'),
        ((SELECT Id FROM ContactInfos WHERE Email = 'email2@email.com'), '456 Elm St', 'City2', 'State2', '67890');
END;
GO