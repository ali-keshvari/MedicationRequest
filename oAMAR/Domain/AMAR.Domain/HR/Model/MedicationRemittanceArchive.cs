using System;

namespace AMAR.Domain.HR.Model
{
    /// <summary>
    /// مدل آرشیو حواله‌های انبار جهت بسته‌بندی دارو و گزارش‌گیری اکسل در هر زمان
    /// </summary>
    public class MedicationRemittanceArchive
    {
        public int ArchiveId { get; set; }
        public Guid RemittanceBatchId { get; set; }
        public string RemittanceNumber { get; set; }
        public Guid? SnapshotId { get; set; }
        public int WorkFlowHdrId { get; set; }
        public int PersonId { get; set; }
        public int? PersonelCode { get; set; }
        public string PersonFullName { get; set; }
        public string DeliveryLocation { get; set; }
        public string MedicationsSummary { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Qty { get; set; }
        public DateTime IssuedDate { get; set; }
        public string IssuedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual Person Person { get; set; }
    }
}
