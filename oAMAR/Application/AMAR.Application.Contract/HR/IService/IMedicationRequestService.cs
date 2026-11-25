using System;
using System.Collections.Generic;
using AMAR.Application.Contract.HR.DTO;

namespace AMAR.Application.Contract.HR.IService
{
    public interface IMedicationRequestService
    {
        // ======================= عملیات انباردار =======================
        
        /// <summary>
        /// دریافت لیست تجمیعی به ازای هر دارو جهت نمایش تعداد درخواست‌ها به انباردار
        /// </summary>
        List<MedicationSummaryDTO> GetPendingMedicationSummary();

        /// <summary>
        /// دریافت ریز رکوردهای در انتظار قبل از اسنپ شات
        /// </summary>
        List<MedicationRequestDTO> GetPendingRequests();

        /// <summary>
        /// ایجاد اسنپ شات لحظه‌ای برای درخواست‌های تایید شده (امکان فیلتر بر اساس داروهای انتخابی)
        /// </summary>
        Guid TakeWarehouseSnapshot(string userName, List<int> productIds = null);

        /// <summary>
        /// دریافت لیست رکوردهای اسنپ شات شده جهت انتخاب و صدور حواله
        /// </summary>
        List<MedicationRequestDTO> GetSnapshottedRequests(Guid? snapshotId = null);

        /// <summary>
        /// صدور یکجای حواله برای داروهای انتخاب شده + ذخیره در آرشیو دائمی جهت خروجی اکسل و بسته‌بندی + ارسال نوتیفیکیشن
        /// </summary>
        void IssueWarehouseRemittance(List<int> selectedMedicationRequestIds, string userName, int issueWFFieldId);
        void IssueWarehouseRemittance(List<int> selectedMedicationRequestIds, List<int> selectedProductIds, Guid? snapshotId, string userName, int issueWFFieldId);

        /// <summary>
        /// لغو و بازگرداندن اقلام انتخابی از اسنپ‌شات جاری به صف انتظار اولیه (Status = 0)
        /// </summary>
        void RevertSnapshot(Guid? snapshotId, List<int> productIds = null);

        /// <summary>
        /// دریافت لیست بچ‌های حواله ثبت شده در آرشیو
        /// </summary>
        List<RemittanceBatchSummaryDTO> GetRemittanceBatches();

        /// <summary>
        /// دریافت لیست بسته‌بندی دارو به ازای هر پرسنل جهت درج روی بسته و خروجی اکسل
        /// </summary>
        List<MedicationPackagingSlipDTO> GetPackagingSlip(Guid? batchId = null);


        // ======================= عملیات دبیرخانه =======================

        /// <summary>
        /// دریافت لیست داروهای حواله کشیده شده به همراه تاریخ صدور حواله، با امکان فیلتر بر اساس کد پرسنلی، نام و محل تحویل
        /// </summary>
        List<MedicationRequestDTO> GetSecretariatRequests(SecretariatSearchFilterDTO filter);
        List<MedicationRequestDTO> GetDeliveredRequests(SecretariatSearchFilterDTO filter);

        /// <summary>
        /// ثبت تحویل دارو به پرسنل مراجعه‌کننده + Concat بر اساس WorkFlowHdrId و PersonId و ذخیره در فیلد دیگر فرم و ارسال نوتیفیکیشن
        /// </summary>
        void DeliverMedications(List<int> selectedMedicationRequestIds, string userName, int deliveryWFFieldId);

        /// <summary>
        /// دریافت لیست محل‌های تحویل جهت فیلتر
        /// </summary>
        List<string> GetDeliveryLocations();
    }
}
