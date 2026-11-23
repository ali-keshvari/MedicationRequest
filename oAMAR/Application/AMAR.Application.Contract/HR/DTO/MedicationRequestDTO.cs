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
        public string PersonCode { get; set; }
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
        public string PersonCode { get; set; }
        public string FullName { get; set; }
        public string DeliveryLocation { get; set; }
    }
}
