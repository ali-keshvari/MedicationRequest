using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using AMAR.Domain.HR.IRepository;
using AMAR.Domain.HR.Model;

namespace AMAR.DataAccess.EF.Repository.HR
{
    public class MedicationRequestRepository : IMedicationRequestRepository
    {
        private readonly dbContext _context;

        public MedicationRequestRepository(dbContext context = null)
        {
            _context = context ?? new dbContext();
        }

        public IQueryable<MedicationRequest> GetAll()
        {
            return _context.MedicationRequests.AsNoTracking();
        }

        public MedicationRequest GetById(int uniqId)
        {
            return _context.MedicationRequests.Find(uniqId);
        }

        public List<MedicationRequest> GetPendingRequests()
        {
            return _context.MedicationRequests
                .Where(x => x.Status == (int)MedicationStatus.Pending)
                .OrderBy(x => x.productName)
                .ToList();
        }

        public Guid CreateSnapshot(string userName)
        {
            var pendingItems = _context.MedicationRequests
                .Where(x => x.Status == (int)MedicationStatus.Pending)
                .ToList();

            if (!pendingItems.Any())
                return Guid.Empty;

            var snapshotId = Guid.NewGuid();
            var now = DateTime.Now;

            foreach (var item in pendingItems)
            {
                item.SnapshotId = snapshotId;
                item.SnapshotDate = now;
                item.Status = (int)MedicationStatus.Snapshotted;
            }

            _context.SaveChanges();
            return snapshotId;
        }

        public List<MedicationRequest> GetSnapshottedRequests(Guid? snapshotId = null)
        {
            var query = _context.MedicationRequests
                .Where(x => x.Status == (int)MedicationStatus.Snapshotted);

            if (snapshotId.HasValue && snapshotId.Value != Guid.Empty)
            {
                query = query.Where(x => x.SnapshotId == snapshotId.Value);
            }

            return query.OrderBy(x => x.productName).ToList();
        }

        public void IssueRemittance(List<int> uniqIds, string userName, int issueWFFieldId)
        {
            if (uniqIds == null || !uniqIds.Any())
                return;

            var items = _context.MedicationRequests
                .Where(x => uniqIds.Contains(x.UniqId) && x.Status == (int)MedicationStatus.Snapshotted)
                .ToList();

            if (!items.Any())
                return;

            var now = DateTime.Now;
            foreach (var item in items)
            {
                item.Status = (int)MedicationStatus.Issued;
                item.IssuedDate = now;
                item.IssuedBy = userName;
            }

            _context.SaveChanges();

            // گروه بندی بر اساس distinct (WorkFlowHdrId, PersonId) جهت الحاق در فرم فرآیند و ارسال نوتیفیکیشن
            var grouped = items
                .GroupBy(x => new { x.WorkFlowHdrId, x.PersonId })
                .ToList();

            foreach (var group in grouped)
            {
                var medList = string.Join(" | ", group.Select(m => $"{m.productName} ({m.Qty} عدد)"));
                var fullText = $"[حواله انبار در تاریخ {now:yyyy/MM/dd HH:mm} توسط {userName}]: {medList}";

                SaveWorkflowAndNotify(group.Key.WorkFlowHdrId, group.Key.PersonId, issueWFFieldId, fullText, "ISSUE", userName);
            }
        }

        public List<MedicationRequest> GetIssuedRequests()
        {
            return _context.MedicationRequests
                .Where(x => x.Status == (int)MedicationStatus.Issued)
                .OrderByDescending(x => x.IssuedDate)
                .ToList();
        }

        public List<MedicationRequestWithPerson> GetIssuedRequestsWithPersonInfo(string personCode = null, string fullName = null, string deliveryLocation = null)
        {
            var sql = @"
                SELECT 
                    req.UniqId,
                    req.WorkFlowHdrId,
                    req.PersonId,
                    req.ProductId,
                    req.productName,
                    req.Qty,
                    req.DeliveryLocation,
                    req.ShYear,
                    req.ShSeason,
                    req.Status,
                    req.SnapshotId,
                    req.SnapshotDate,
                    req.IssuedDate,
                    req.IssuedBy,
                    req.DeliveredDate,
                    req.DeliveredBy,
                    req.CreatedDate,
                    ISNULL(p.PersonCode, '') AS PersonCode,
                    ISNULL(p.FirstName + ' ' + p.LastName, N'نامشخص') AS FullName,
                    ISNULL(p.NationalCode, '') AS NationalCode,
                    ISNULL(p.Mobile, '') AS Mobile
                FROM [gen].[MedicationRequest] req
                LEFT JOIN [dbo].[Person] p ON req.PersonId = p.PersonId
                WHERE req.Status = @status
            ";

            var results = _context.Database.SqlQuery<MedicationRequestWithPerson>(
                sql,
                new SqlParameter("@status", (int)MedicationStatus.Issued)
            ).ToList();

            IEnumerable<MedicationRequestWithPerson> filtered = results;

            if (!string.IsNullOrWhiteSpace(personCode))
            {
                filtered = filtered.Where(x => x.PersonCode != null && x.PersonCode.Contains(personCode.Trim()));
            }

            if (!string.IsNullOrWhiteSpace(fullName))
            {
                filtered = filtered.Where(x => x.FullName != null && x.FullName.Contains(fullName.Trim()));
            }

            if (!string.IsNullOrWhiteSpace(deliveryLocation))
            {
                filtered = filtered.Where(x => x.DeliveryLocation != null && x.DeliveryLocation.Equals(deliveryLocation.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            return filtered.OrderByDescending(x => x.IssuedDate).ToList();
        }

        public void DeliverMedications(List<int> uniqIds, string userName, int deliveryWFFieldId)
        {
            if (uniqIds == null || !uniqIds.Any())
                return;

            var items = _context.MedicationRequests
                .Where(x => uniqIds.Contains(x.UniqId) && x.Status == (int)MedicationStatus.Issued)
                .ToList();

            if (!items.Any())
                return;

            var now = DateTime.Now;
            foreach (var item in items)
            {
                item.Status = (int)MedicationStatus.Delivered;
                item.DeliveredDate = now;
                item.DeliveredBy = userName;
            }

            _context.SaveChanges();

            // گروه بندی بر اساس distinct (WorkFlowHdrId, PersonId) جهت الحاق تحویل در فرم فرآیند و ارسال نوتیفیکیشن
            var grouped = items
                .GroupBy(x => new { x.WorkFlowHdrId, x.PersonId })
                .ToList();

            foreach (var group in grouped)
            {
                var medList = string.Join(" | ", group.Select(m => $"{m.productName} ({m.Qty} عدد)"));
                var fullText = $"[تحویل دبیرخانه در تاریخ {now:yyyy/MM/dd HH:mm} توسط {userName}]: {medList}";

                SaveWorkflowAndNotify(group.Key.WorkFlowHdrId, group.Key.PersonId, deliveryWFFieldId, fullText, "DELIVERY", userName);
            }
        }

        public List<string> GetDeliveryLocations()
        {
            return _context.MedicationRequests
                .Where(x => x.DeliveryLocation != null && x.DeliveryLocation != "")
                .Select(x => x.DeliveryLocation)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        public void SaveWorkflowAndNotify(int workFlowHdrId, int personId, int wfFieldId, string medicationListText, string actionType, string userName)
        {
            var pWfHdrId = new SqlParameter("@WorkFlowHdrId", workFlowHdrId);
            var pPersonId = new SqlParameter("@PersonId", personId);
            var pWfFieldId = new SqlParameter("@WFFieldId", wfFieldId);
            var pText = new SqlParameter("@MedicationListText", medicationListText ?? string.Empty);
            var pAction = new SqlParameter("@ActionType", actionType);
            var pUser = new SqlParameter("@OperatorUserName", userName ?? "System");

            _context.Database.ExecuteSqlCommand(
                "EXEC [gen].[sp_SaveMedicationWorkflowAndNotify] @WorkFlowHdrId, @PersonId, @WFFieldId, @MedicationListText, @ActionType, @OperatorUserName",
                pWfHdrId, pPersonId, pWfFieldId, pText, pAction, pUser
            );
        }
    }
}
