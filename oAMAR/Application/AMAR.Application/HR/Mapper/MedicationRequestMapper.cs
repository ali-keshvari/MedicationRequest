using System;
using System.Globalization;
using AMAR.Application.Contract.HR.DTO;
using AMAR.Domain.HR.Model;

namespace AMAR.Application.HR.Mapper
{
    public static class MedicationRequestMapper
    {
        private static readonly PersianCalendar pc = new PersianCalendar();

        public static string ToShamsi(this DateTime? dt)
        {
            if (!dt.HasValue) return string.Empty;
            var d = dt.Value;
            return $"{pc.GetYear(d):0000}/{pc.GetMonth(d):00}/{pc.GetDayOfMonth(d):00} {d:HH:mm}";
        }

        public static string GetStatusTitle(int status)
        {
            switch ((MedicationStatus)status)
            {
                case MedicationStatus.Pending:
                    return "در انتظار بررسی انبار";
                case MedicationStatus.Snapshotted:
                    return "در اسنپ‌شات (در حال صدور حواله)";
                case MedicationStatus.Issued:
                    return "حواله صادر شده (آماده تحویل دبیرخانه)";
                case MedicationStatus.Delivered:
                    return "تحویل شده به پرسنل";
                default:
                    return "نامشخص";
            }
        }

        public static MedicationRequestDTO ToDTO(MedicationRequest entity)
        {
            if (entity == null) return null;

            return new MedicationRequestDTO
            {
                UniqId = entity.UniqId,
                WorkFlowHdrId = entity.WorkFlowHdrId,
                PersonId = entity.PersonId,
                ProductId = entity.ProductId,
                productName = entity.productName,
                Qty = entity.Qty,
                DeliveryLocation = entity.DeliveryLocation,
                ShYear = entity.ShYear,
                ShSeason = entity.ShSeason,
                Status = entity.Status,
                StatusTitle = GetStatusTitle(entity.Status),

                SnapshotId = entity.SnapshotId,
                SnapshotDate = entity.SnapshotDate,
                SnapshotDateShamsi = entity.SnapshotDate.ToShamsi(),

                IssuedDate = entity.IssuedDate,
                IssuedDateShamsi = entity.IssuedDate.ToShamsi(),
                IssuedBy = entity.IssuedBy,

                DeliveredDate = entity.DeliveredDate,
                DeliveredDateShamsi = entity.DeliveredDate.ToShamsi(),
                DeliveredBy = entity.DeliveredBy,

                // از فیلد PersonelCode با تایپ int خوانده می‌شود
                PersonelCode = entity.Person?.PersonelCode,
                FullName = entity.Person != null ? entity.Person.FullName : null,
                NationalCode = entity.Person?.NationalCode,
                Mobile = entity.Person?.Mobile
            };
        }
    }
}
