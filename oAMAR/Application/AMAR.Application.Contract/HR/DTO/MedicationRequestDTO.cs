using System;
using System.Collections.Generic;

namespace AMAR.Application.Contract.HR.DTO
{
    public class MedicationRequestDTO
    {
        public int UniqId { get; set; }
        public int WorkFlowHdrId { get; set; }
        public int PersonId { get; set; }
        public int ProductId { get; set; }
        public string productName { get; set; }
        public int Qty { get; set; }
        public string DeliveryLocation { get; set; }
        public int? ShYear { get; set; }
        public string ShSeason { get; set; }
        public int Status { get; set; }
        public string StatusTitle { get; set; }

        public Guid? SnapshotId { get; set; }
        public DateTime? SnapshotDate { get; set; }
        public string SnapshotDateShamsi { get; set; }

        public DateTime? IssuedDate { get; set; }
        public string IssuedDateShamsi { get; set; }
        public string IssuedBy { get; set; }

        public DateTime? DeliveredDate { get; set; }
        public string DeliveredDateShamsi { get; set; }
        public string DeliveredBy { get; set; }

        // فیلدهای حاصل از Join با جدول Person جهت نمایش به دبیرخانه
        public int? PersonelCode { get; set; }
        public string FullName { get; set; }
        public string NationalCode { get; set; }
        public string Mobile { get; set; }
    }

    /// <summary>
    /// خلاصه تجمیعی داروها جهت نمایش به انباردار قبل از اسنپ‌شات یا برای صدور حواله
    /// </summary>
    public class MedicationSummaryDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int TotalRequests { get; set; }
        public int TotalQty { get; set; }
    }

    /// <summary>
    /// مدل ورودی عملیات صدور حواله توسط انباردار
    /// </summary>
    public class IssueRemittanceCommandDTO
    {
        public List<int> SelectedUniqIds { get; set; } = new List<int>();
        public int WFFieldId { get; set; } = 101; // شناسه فیلد فرآیند برای حواله انبار
    }

    /// <summary>
    /// مدل ورودی عملیات تحویل دارو توسط دبیرخانه
    /// </summary>
    public class DeliverMedicationCommandDTO
    {
        public List<int> SelectedUniqIds { get; set; } = new List<int>();
        public int WFFieldId { get; set; } = 102; // شناسه فیلد فرآیند برای تحویل دبیرخانه
    }

    /// <summary>
    /// مدل فیلتر جستجوی دبیرخانه
    /// </summary>
    public class SecretariatSearchFilterDTO
    {
        public int? PersonelCode { get; set; }
        public string FullName { get; set; }
        public string DeliveryLocation { get; set; }
    }

    /// <summary>
    /// خلاصه بچ‌های حواله انبار جهت آرشیو و انتخاب خروجی اکسل
    /// </summary>
    public class RemittanceBatchSummaryDTO
    {
        public Guid RemittanceBatchId { get; set; }
        public string RemittanceNumber { get; set; }
        public DateTime IssuedDate { get; set; }
        public string IssuedDateShamsi { get; set; }
        public string IssuedBy { get; set; }
        public int TotalPersonsCount { get; set; }
        public int TotalItemsCount { get; set; }
    }

    /// <summary>
    /// لیست تفکیکی بسته‌بندی دارو به ازای هر پرسنل جهت درج روی بسته و خروجی اکسل
    /// </summary>
    public class MedicationPackagingSlipDTO
    {
        public int WorkFlowHdrId { get; set; }
        public int PersonId { get; set; }
        public int? PersonelCode { get; set; }
        public string PersonFullName { get; set; }
        public string DeliveryLocation { get; set; }
        public string MedicationsSummary { get; set; }
        public int TotalItemsCount { get; set; }
        public Guid RemittanceBatchId { get; set; }
        public string RemittanceNumber { get; set; }
        public DateTime IssuedDate { get; set; }
        public string IssuedDateShamsi { get; set; }
        public string IssuedBy { get; set; }

        public List<PackagingItemLineDTO> Items { get; set; } = new List<PackagingItemLineDTO>();
    }

    public class PackagingItemLineDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Qty { get; set; }
    }
}
