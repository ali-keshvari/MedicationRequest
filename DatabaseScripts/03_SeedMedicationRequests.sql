-- ===================================================================================
-- نام اسکریپت: 03_SeedMedicationRequests.sql
-- هدف: درج داده‌های تستی / اولیه در جدول [gen].[MedicationRequest]
-- بر اساس 56 شناسه پرسنلی (PersonId) و 36 شناسه محصول (ProductId)
-- ===================================================================================

USE [pAMAR]; -- نام دیتابیس خود را در صورت تفاوت در اینجا اصلاح کنید
GO

SET NOCOUNT ON;

-- ===================================================================================
-- تنظیمات نحوه تولید داده
-- 1 = الگوی فرآیندی واقعی (هر فرد دارای یک شماره فرآیند مستقل و ۲ الی ۴ قلم داروی متفاوت)
-- 2 = حالت ضرب دکارتی کامل (تک‌تک ۳۶ دارو برای تمام ۵۶ نفر = ۲۰۱۶ رکورد)
-- ===================================================================================
DECLARE @GenerationMode INT = 1;

-- سال و فصل پیش‌فرض
DECLARE @ShYear INT = 1403;
DECLARE @ShSeason NVARCHAR(50) = N'پاییز';

-- وضعیت اولیه (0 = PendingApproval / آماده پردازش در کارتابل انبار)
DECLARE @InitialStatus INT = 0;

-- -----------------------------------------------------------------------------------
-- ۱. لیست ۵۶ شناسه فرد (PersonId)
-- -----------------------------------------------------------------------------------
DECLARE @Persons TABLE (
    RowNo INT IDENTITY(1, 1) PRIMARY KEY,
    PersonId INT NOT NULL
);

INSERT INTO @Persons (PersonId) VALUES
(12270), (12271), (12273), (12274), (12275), (12276), (12277), (12278), (12279), (12280),
(12281), (12282), (12283), (12284), (12285), (12286), (12287), (12288), (12289), (12290),
(12291), (12292), (12293), (12294), (12296), (12298), (12299), (12301), (12302), (12306),
(12307), (12308), (12310), (12311), (12312), (12313), (12314), (12315), (12316), (12317),
(12318), (12319), (12320), (12321), (12322), (12323), (12325), (12326), (12327), (12328),
(12329), (12330), (12331), (12332), (12333), (12334);

-- -----------------------------------------------------------------------------------
-- ۲. لیست ۳۶ شناسه دارو (ProductId) به همراه نام‌های استاندارد دارویی
-- -----------------------------------------------------------------------------------
DECLARE @Products TABLE (
    RowNo INT IDENTITY(1, 1) PRIMARY KEY,
    ProductId INT NOT NULL,
    ProductName NVARCHAR(250) NOT NULL
);

INSERT INTO @Products (ProductId, ProductName) VALUES
(27721, N'قرص استامینوفن کدئین 300/10'),
(41673, N'کپسول آموکسی‌سیلین 500 میلی‌گرم'),
(27549, N'قرص لوزارتان پتاسیم 25 میلی‌گرم'),
(4910,  N'قرص متفورمین 500 میلی‌گرم'),
(52967, N'قرص آتورواستاتین 20 میلی‌گرم'),
(52969, N'کپسول امپرازول 20 میلی‌گرم'),
(36651, N'قرص پنتوپرازول 40 میلی‌گرم'),
(36657, N'قرص سیتریزین 10 میلی‌گرم'),
(36663, N'قرص فاموتیدین 40 میلی‌گرم'),
(4983,  N'قطره چشمی اشک مصنوعی'),
(6247,  N'قرص ناپروکسن 500 میلی‌گرم'),
(5007,  N'قرص سرترالین 50 میلی‌گرم'),
(5024,  N'قرص کلیدینیوم سی'),
(5031,  N'قرص آلپرازولام 0.5 میلی‌گرم'),
(27289, N'قرص باکلوفن 10 میلی‌گرم'),
(7463,  N'قرص متوپرولول 50 میلی‌گرم'),
(5059,  N'قرص آملودیپین 5 میلی‌گرم'),
(5064,  N'قرص سیپروفلوکساسین 500 میلی‌گرم'),
(5083,  N'کپسول سفالکسین 500 میلی‌گرم'),
(5085,  N'قرص هیدروکلروتیازید 50 میلی‌گرم'),
(42699, N'قرص فکسوفنادین 120 میلی‌گرم'),
(5094,  N'اسپری استنشاقی سالبوتامول 100'),
(6230,  N'کپسول ژلوفن 400 میلی‌گرم'),
(7172,  N'کپسول گاباپنتین 100 میلی‌گرم'),
(40413, N'قرص دیسیکلومین 10 میلی‌گرم'),
(40414, N'قرص مونته لوکاست 10 میلی‌گرم'),
(36542, N'قرص لووتیروکسین سدیم 100 میکروگرم'),
(36779, N'قرص ویتامین د3 50000 واحد'),
(5150,  N'قرص ب-کمپلکس'),
(5153,  N'قرص کلسیم د3'),
(7175,  N'شربت اکسپکتورانت 60 میلی‌لیتر'),
(7170,  N'قرص پروپرانولول 20 میلی‌گرم'),
(33334, N'قطره چشمی سیپلکس 0.3%'),
(33150, N'پماد چشمی هیدروکورتیزون'),
(55451, N'قرص آزیترومایسین 250 میلی‌گرم'),
(8142,  N'شربت دیفن هیدرامین کامپاند');

-- در صورت تمایل اگر جدول داروها در دیتابیس شما وجود دارد می‌توانید نام‌ها را به‌روز کنید:
/*
IF OBJECT_ID('dbo.Product', 'U') IS NOT NULL
BEGIN
    UPDATE p
    SET p.ProductName = pr.ProductName
    FROM @Products p
    INNER JOIN dbo.Product pr ON p.ProductId = pr.ProductId;
END
*/

-- -----------------------------------------------------------------------------------
-- ۳. لیست محل‌های تحویل سازمانی (Delivery Locations)
-- -----------------------------------------------------------------------------------
DECLARE @Locations TABLE (
    LocId INT IDENTITY(1, 1) PRIMARY KEY,
    LocationName NVARCHAR(150) NOT NULL
);

INSERT INTO @Locations (LocationName) VALUES
(N'ستاد مرکزی - طبقه ۳'),
(N'کارخانه شماره ۱ - دفتر اداری'),
(N'کارخانه شماره ۲ - انبار و توزیع'),
(N'مجتمع اداری تهران - امور پرسنلی'),
(N'ساختمان پشتیبانی و مهندسی'),
(N'دفتر مدیریت - دبیرخانه مرکزی'),
(N'شعبه غرب - امور اداری'),
(N'ساختمان مالی و اداری');

-- -----------------------------------------------------------------------------------
-- ۴. عملیات درج در جدول [gen].[MedicationRequest]
-- -----------------------------------------------------------------------------------
BEGIN TRANSACTION;

BEGIN TRY

    IF (@GenerationMode = 1)
    BEGIN
        -- حالت ۱: الگوی واقعی گردش‌کار فرآیند
        -- به ازای هر پرسنل یک شماره فرم فرآیند (WorkFlowHdrId) یکتا اختصاص می‌یابد
        -- و هر فرد بین ۲ الی ۴ قلم دارو درخواست داده است. تمام ۳۶ دارو پوشش داده می‌شوند.

        INSERT INTO [gen].[MedicationRequest]
        (
            [WorkFlowHdrId],
            [PersonId],
            [ProductId],
            [productName],
            [Qty],
            [DeliveryLocation],
            [ShYear],
            [ShSeason],
            [Status],
            [CreatedDate]
        )
        SELECT
            -- شماره درخواست فرآیند برای هر فرد (مثلاً 50000 + RowNo)
            50000 + p.RowNo AS [WorkFlowHdrId],
            p.PersonId,
            pr.ProductId,
            pr.ProductName,
            -- تعداد تصادفی/متنوع بین ۱ تا ۴ بسته دارو
            ((p.RowNo * 3 + pr.RowNo * 7) % 4) + 1 AS [Qty],
            -- توزیع محل‌های تحویل بر اساس ردیف پرسنل
            loc.LocationName AS [DeliveryLocation],
            @ShYear AS [ShYear],
            @ShSeason AS [ShSeason],
            @InitialStatus AS [Status],
            DATEADD(MINUTE, -((p.RowNo * 17) % 2880), GETDATE()) AS [CreatedDate]
        FROM @Persons p
        CROSS APPLY (
            -- هر فرد ۲ الی ۳ دارو دریافت می‌کند، با الگوریتم چرخشی که کل ۳۶ دارو پوشش داده شوند
            SELECT TOP (2 + (p.RowNo % 2))
                pr_sub.RowNo,
                pr_sub.ProductId,
                pr_sub.ProductName
            FROM @Products pr_sub
            WHERE pr_sub.RowNo IN (
                ((p.RowNo * 2) % 36) + 1,
                ((p.RowNo * 2 + 7) % 36) + 1,
                ((p.RowNo * 2 + 19) % 36) + 1
            )
            ORDER BY pr_sub.RowNo
        ) pr
        CROSS APPLY (
            SELECT LocationName 
            FROM @Locations 
            WHERE LocId = ((p.RowNo % 8) + 1)
        ) loc
        ORDER BY p.PersonId, pr.ProductId;

        PRINT N'تعداد ' + CAST(@@ROWCOUNT AS NVARCHAR(20)) + N' رکورد با الگوی فرآیندی واقعی (حالت ۱) با موفقیت درج شد.';
    END
    ELSE
    BEGIN
        -- حالت ۲: ضرب دکارتی کامل (تمام ۵۶ پرسنل × تمام ۳۶ دارو = ۲۰۱۶ رکورد)
        INSERT INTO [gen].[MedicationRequest]
        (
            [WorkFlowHdrId],
            [PersonId],
            [ProductId],
            [productName],
            [Qty],
            [DeliveryLocation],
            [ShYear],
            [ShSeason],
            [Status],
            [CreatedDate]
        )
        SELECT
            50000 + p.RowNo AS [WorkFlowHdrId],
            p.PersonId,
            pr.ProductId,
            pr.ProductName,
            ((p.RowNo + pr.RowNo) % 3) + 1 AS [Qty],
            loc.LocationName AS [DeliveryLocation],
            @ShYear AS [ShYear],
            @ShSeason AS [ShSeason],
            @InitialStatus AS [Status],
            DATEADD(MINUTE, -((p.RowNo * 13) % 2880), GETDATE()) AS [CreatedDate]
        FROM @Persons p
        CROSS JOIN @Products pr
        CROSS APPLY (
            SELECT LocationName 
            FROM @Locations 
            WHERE LocId = ((p.RowNo % 8) + 1)
        ) loc
        ORDER BY p.PersonId, pr.ProductId;

        PRINT N'تعداد ' + CAST(@@ROWCOUNT AS NVARCHAR(20)) + N' رکورد به صورت ضرب دکارتی کامل (حالت ۲) با موفقیت درج شد.';
    END

    COMMIT TRANSACTION;
    PRINT N'تراکنش با موفقیت ثبت (COMMIT) گردید.';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    PRINT N'خطا در هنگام درج داده‌ها: ' + ERROR_MESSAGE();
    THROW;
END CATCH;
GO

-- -----------------------------------------------------------------------------------
-- ۵. گزارش و بررسی نتیجه درج
-- -----------------------------------------------------------------------------------
SELECT 
    COUNT(*) AS [TotalInsertedRequests],
    COUNT(DISTINCT [PersonId]) AS [DistinctPersonsCovered],
    COUNT(DISTINCT [ProductId]) AS [DistinctProductsCovered],
    COUNT(DISTINCT [WorkFlowHdrId]) AS [DistinctWorkflowsCovered]
FROM [gen].[MedicationRequest];

SELECT TOP 20
    [MedicationRequestId],
    [WorkFlowHdrId],
    [PersonId],
    [ProductId],
    [productName],
    [Qty],
    [DeliveryLocation],
    [Status],
    [CreatedDate]
FROM [gen].[MedicationRequest]
ORDER BY [MedicationRequestId] DESC;
GO
