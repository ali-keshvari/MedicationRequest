using System;

namespace AMAR.Domain.HR.Model
{
    public enum MedicationStatus
    {
        Pending = 0,       // در انتظار پردازش (ثبت شده پس از تایید پزشک)
        Snapshotted = 1,   // اسنپ شات گرفته شده توسط انبار
        Issued = 2,        // حواله کشیده شده توسط انباردار
        Delivered = 3      // تحویل داده شده به پرسنل در دبیرخانه
    }

    /// <summary>
    /// مدل موجودیت جدول gen.MedicationRequest
    /// </summary>
    public class MedicationRequest
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

        // فیلدهای تکمیلی جهت اسنپ‌شات، صدور حواله انبار و تحویل دبیرخانه
        public Guid? SnapshotId { get; set; }
        public DateTime? SnapshotDate { get; set; }
        public DateTime? IssuedDate { get; set; }
        public string IssuedBy { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public string DeliveredBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// مدل خروجی درخواست دارو همراه با مشخصات فردی (حاصل Join در ریپازیتوری)
    /// </summary>
    public class MedicationRequestWithPerson : MedicationRequest
    {
        public string PersonCode { get; set; }
        public string FullName { get; set; }
        public string NationalCode { get; set; }
        public string Mobile { get; set; }
    }
}
