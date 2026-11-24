-- ===================================================================================
-- 2. Stored Procedures for Medication Processing, Workflow Append, and Notifications
-- Database: Your Org Database
-- Schema: gen
-- ===================================================================================

-- رویه ذخیره در فرم فرآیند (بر اساس WFFieldId, WorkFlowHdrId) و ارسال پیامک/ایمیل
CREATE OR ALTER PROCEDURE [gen].[sp_SaveMedicationWorkflowAndNotify]
    @WorkFlowHdrId INT,
    @PersonId INT,
    @WFFieldId INT,
    @MedicationListText NVARCHAR(MAX),
    @ActionType NVARCHAR(50), -- 'ISSUE' یا 'DELIVERY'
    @OperatorUserName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY

        -- ۱. ذخیره یا الحاق دیتا در جدول فیلدهای جریان کار (بر اساس WorkFlowHdrId و WFFieldId)
        IF EXISTS (
            SELECT 1 
            FROM [wf].[WorkFlowFieldData] 
            WHERE [WorkFlowHdrId] = @WorkFlowHdrId AND [WFFieldId] = @WFFieldId
        )
        BEGIN
            -- در صورت وجود دیتا از پیش، این دیتا به دیتای قبلی Append می‌گردد
            UPDATE [wf].[WorkFlowFieldData]
            SET [FieldValue] = [FieldValue] + CHAR(13) + CHAR(10) + @MedicationListText,
                [LastUpdated] = GETDATE()
            WHERE [WorkFlowHdrId] = @WorkFlowHdrId AND [WFFieldId] = @WFFieldId;
        END
        ELSE
        BEGIN
            -- در صورت نبود آن Insert می‌شود
            INSERT INTO [wf].[WorkFlowFieldData] ([WorkFlowHdrId], [WFFieldId], [FieldValue], [LastUpdated])
            VALUES (@WorkFlowHdrId, @WFFieldId, @MedicationListText, GETDATE());
        END

        -- ۲. فراخوانی منطق یا پراسیجر ارسال ایمیل و پیامک به پرسنل
        DECLARE @Mobile NVARCHAR(20);
        DECLARE @Email NVARCHAR(100);
        DECLARE @FullName NVARCHAR(200);

        SELECT 
            @Mobile = [Mobile], 
            @Email = [Email],
            @FullName = [FirstName] + ' ' + [LastName]
        FROM [dbo].[Person]
        WHERE [PersonId] = @PersonId;

        DECLARE @NotificationMessage NVARCHAR(MAX);

        IF (@ActionType = 'ISSUE')
        BEGIN
            SET @NotificationMessage = N'همکار گرامی ' + ISNULL(@FullName, '') + 
                N'؛ حواله انبار برای داروهای درخواستی شما به شرح زیر صادر گردید:' + CHAR(13) + CHAR(10) +
                @MedicationListText + CHAR(13) + CHAR(10) +
                N'لطفاً جهت دریافت به دبیرخانه مراجعه فرمایید.';
        END
        ELSE IF (@ActionType = 'DELIVERY')
        BEGIN
            SET @NotificationMessage = N'همکار گرامی ' + ISNULL(@FullName, '') + 
                N'؛ داروهای زیر به شما تحویل گردید:' + CHAR(13) + CHAR(10) +
                @MedicationListText;
        END

        -- ثبت لاگ پیامک / ایمیل یا فراخوانی وب‌سرویس / پراسیجر پیامک سازمان
        /*
        IF (@Mobile IS NOT NULL AND LEN(@Mobile) > 9)
        BEGIN
            EXEC [dbo].[sp_SendSMS] @Recipient = @Mobile, @Message = @NotificationMessage;
        END

        IF (@Email IS NOT NULL AND CHARINDEX('@', @Email) > 0)
        BEGIN
            EXEC [dbo].[sp_SendEmail] @Recipient = @Email, @Subject = N'اطلاعیه درخواست دارو', @Body = @NotificationMessage;
        END
        */

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END
GO
