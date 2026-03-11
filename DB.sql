-- ============================================
-- Library Database Schema
-- ============================================

-- ===========================
-- TABLE: Books
-- ===========================
CREATE TABLE [dbo].[Books] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [ISBN]          NVARCHAR (MAX) NOT NULL,
    [Title]         NVARCHAR (MAX) NOT NULL,
    [Author]        NVARCHAR (MAX) NOT NULL,
    [PublishedYear] INT            NOT NULL,
    [IsAvailable]   BIT            NOT NULL,
    CONSTRAINT [PK_Books] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO


-- ===========================
-- TABLE: Members
-- ===========================
CREATE TABLE [dbo].[Members] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Username]    NVARCHAR (MAX) NOT NULL,
    [Email]       NVARCHAR (MAX) NOT NULL,
    [MemberSince] DATETIME2 (7)  DEFAULT ('0001-01-01T00:00:00.0000000') NOT NULL,
    CONSTRAINT [PK_Members] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO


-- ===========================
-- TABLE: Loans
-- ===========================
CREATE TABLE [dbo].[Loans] (
    [Id]         INT           IDENTITY (1, 1) NOT NULL,
    [BookId]     INT           NOT NULL,
    [ReturnDate] DATETIME2 (7) NULL,
    [MemberId]   INT           DEFAULT ((0)) NOT NULL,
    [DueDate]    DATETIME2 (7) DEFAULT ('0001-01-01T00:00:00.0000000') NOT NULL,
    [LoanDate]   DATETIME2 (7) DEFAULT ('0001-01-01T00:00:00.0000000') NOT NULL,
    CONSTRAINT [PK_Loans] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Loans_Members_MemberId] FOREIGN KEY ([MemberId]) 
        REFERENCES [dbo].[Members] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Loans_Books_BookId] FOREIGN KEY ([BookId]) 
        REFERENCES [dbo].[Books] ([Id]) ON DELETE CASCADE
);
GO


-- ===========================
-- INDEXES
-- ===========================
CREATE NONCLUSTERED INDEX [IX_Loans_MemberId]
    ON [dbo].[Loans]([MemberId] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Loans_BookId]
    ON [dbo].[Loans]([BookId] ASC);
GO
