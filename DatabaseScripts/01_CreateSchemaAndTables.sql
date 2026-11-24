-- ===================================================================================
-- 1. Create Schema and Table for Medication Request & Remittance Archive
-- Database: Your Org Database
-- Schema: gen
-- Table: MedicationRequest, MedicationRemittanceArchive
-- ===================================================================================

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'gen')
BEGIN
    EXEC('CREATE SCHEMA [gen]');
END
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'wf')
BEGIN
    EXEC('CREATE SCHEMA [wf]');
END
GO

-- جدول درخواست‌های دارو (MedicationRequest)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[gen].[MedicationRequest]') AND type in (N'U'))
BEGIN
    CREATE TABLE [gen].[MedicationRequest](
        [UniqId] [int] IDENTITY(1,1) NOT NULL,
        [WorkFlowHdrId] [int] NOT NULL,
        [PersonId] [int] NOT NULL,
        [ProductId] [int] NOT NULL,
        [productName] [nvarchar](250) NOT NULL,
        [Qty] [int] NOT NULL,
        [DeliveryLocation] [nvarchar](150) NULL,
        [ShYear] [int] NULL,
        [ShSeason] [nvarchar](50) NULL,
        [Status] [int] NOT NULL CONSTRAINT [DF_MedicationRequest_Status] DEFAULT (0), 
        -- Status: 
        -- 0 = PendingApproval / تایید شده توسط پزشک و آماده پردازش
        -- 1 = Snapshotted / ثبت شده در اسنپ شات انبار
        -- 2 = Issued / حواله کشیده شده توسط انباردار
        -- 3 = Delivered / تحویل داده شده به پرسنل توسط دبیرخانه
        
        [SnapshotId] [uniqueidentifier] NULL,       -- شناسه گروهی اسنپ‌شات لحظه‌ای انبار
        [SnapshotDate] [datetime] NULL,             -- تاریخ ایجاد اسنپ‌شات
        [IssuedDate] [datetime] NULL,               -- تاریخی که حواله کشیده شده است
        [IssuedBy] [nvarchar](100) NULL,            -- نام کاربری انباردار
        [DeliveredDate] [datetime] NULL,            -- تاریخ تحویل به پرسنل
        [DeliveredBy] [nvarchar](100) NULL,         -- نام کاربری تحویل‌دهنده دبیرخانه
        [CreatedDate] [datetime] NOT NULL CONSTRAINT [DF_MedicationRequest_CreatedDate] DEFAULT (GETDATE()),

        CONSTRAINT [PK_MedicationRequest] PRIMARY KEY CLUSTERED ([UniqId] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_MedicationRequest_Status] 
        ON [gen].[MedicationRequest] ([Status]);

    CREATE NONCLUSTERED INDEX [IX_MedicationRequest_WF_Person] 
        ON [gen].[MedicationRequest] ([WorkFlowHdrId], [PersonId]);
END
GO

-- جدول آرشیو حواله‌های صادر شده انبار جهت بسته‌بندی دارو و خروجی اکسل در هر زمان
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[gen].[MedicationRemittanceArchive]') AND type in (N'U'))
BEGIN
    CREATE TABLE [gen].[MedicationRemittanceArchive](
        [ArchiveId] [int] IDENTITY(1,1) NOT NULL,
        [RemittanceBatchId] [uniqueidentifier] NOT NULL,   -- شناسه بچ حواله
        [RemittanceNumber] [nvarchar](50) NOT NULL,        -- شماره حواله تولید شده
        [SnapshotId] [uniqueidentifier] NULL,              -- شناسه اسنپ شات مرتبط
        [WorkFlowHdrId] [int] NOT NULL,                    -- شماره فرآیند
        [PersonId] [int] NOT NULL,                         -- شناسه شخص
        [PersonelCode] [int] NULL,                         -- کد پرسنلی شخص
        [PersonFullName] [nvarchar](200) NULL,             -- نام و نام خانوادگی جهت درج روی بسته
        [DeliveryLocation] [nvarchar](150) NULL,           -- محل تحویل جهت درج روی بسته
        [MedicationsSummary] [nvarchar](max) NULL,         -- لیست داروها برای درج روی بسته یا اکسل
        [ProductId] [int] NOT NULL,                        -- شناسه قلم دارو
        [ProductName] [nvarchar](250) NOT NULL,            -- نام قلم دارو
        [Qty] [int] NOT NULL,                              -- تعداد قلم دارو
        [IssuedDate] [datetime] NOT NULL,                  -- تاریخ و ساعت کشیدن حواله
        [IssuedBy] [nvarchar](100) NOT NULL,               -- کاربر صادرکننده حواله
        [CreatedDate] [datetime] NOT NULL CONSTRAINT [DF_MedicationRemittanceArchive_CreatedDate] DEFAULT (GETDATE()),

        CONSTRAINT [PK_MedicationRemittanceArchive] PRIMARY KEY CLUSTERED ([ArchiveId] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_MedicationRemittanceArchive_Batch] 
        ON [gen].[MedicationRemittanceArchive] ([RemittanceBatchId]);

    CREATE NONCLUSTERED INDEX [IX_MedicationRemittanceArchive_WF_Person] 
        ON [gen].[MedicationRemittanceArchive] ([WorkFlowHdrId], [PersonId]);
END
GO

-- ساختار نمونه جدول فیلدهای جریان کار (Workflow Field Data) جهت درج یا الحاق (Append)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[wf].[WorkFlowFieldData]') AND type in (N'U'))
BEGIN
    CREATE TABLE [wf].[WorkFlowFieldData](
        [WorkFlowHdrId] [int] NOT NULL,
        [WFFieldId] [int] NOT NULL,
        [FieldValue] [nvarchar](max) NULL,
        [LastUpdated] [datetime] NOT NULL CONSTRAINT [DF_WorkFlowFieldData_LastUpdated] DEFAULT (GETDATE()),
        CONSTRAINT [PK_WorkFlowFieldData] PRIMARY KEY CLUSTERED ([WorkFlowHdrId] ASC, [WFFieldId] ASC)
    );
END
GO

-- ساختار نمونه جدول Person سازمان جهت Join و بازیابی کد پرسنلی و نام پرسنل
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Person]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Person](
        [PersonId] [int] NOT NULL PRIMARY KEY,
        [PersonelCode] [int] NOT NULL,
        [FirstName] [nvarchar](100) NOT NULL,
        [LastName] [nvarchar](100) NOT NULL,
        [NationalCode] [nvarchar](20) NULL,
        [Mobile] [nvarchar](20) NULL,
        [Email] [nvarchar](100) NULL
    );
END
GO
