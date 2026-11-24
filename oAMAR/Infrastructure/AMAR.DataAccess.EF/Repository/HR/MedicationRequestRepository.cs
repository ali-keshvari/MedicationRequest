using System;
using System.Collections.Generic;
using System.Data.Entity;
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

        public Guid CreateSnapshot(string userName, List<int> productIds = null)
        {
            var query = _context.MedicationRequests
                .Where(x => x.Status == (int)MedicationStatus.Pending);

            if (productIds != null && productIds.Any())
            {
                query = query.Where(x => productIds.Contains(x.ProductId));
            }

            var pendingItems = query.ToList();

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
            IssueRemittance(uniqIds, null, null, userName, issueWFFieldId);
        }

        public void IssueRemittance(List<int> selectedUniqIds, List<int> selectedProductIds, Guid? snapshotId, string userName, int issueWFFieldId)
        {
            // ۱. استخراج اقلامی که تیک خورده‌اند و حواله آن‌ها باید صادر شود
            var issueQuery = _context.MedicationRequests
                .Include(x => x.Person)
                .Where(x => x.Status == (int)MedicationStatus.Snapshotted);

            if (snapshotId.HasValue && snapshotId.Value != Guid.Empty)
            {
                issueQuery = issueQuery.Where(x => x.SnapshotId == snapshotId.Value);
            }

            if (selectedProductIds != null && selectedProductIds.Any())
            {
                issueQuery = issueQuery.Where(x => selectedProductIds.Contains(x.ProductId));
            }
            else if (selectedUniqIds != null && selectedUniqIds.Any())
            {
                issueQuery = issueQuery.Where(x => selectedUniqIds.Contains(x.UniqId));
            }
            else
            {
                issueQuery = issueQuery.Where(x => false);
            }

            var itemsToIssue = issueQuery.ToList();

            // ۲. بازگرداندن اقلام تیک‌نخورده از همین اسنپ‌شات به وضعیت اولیه (Status = 0 و SnapshotId = null)
            var resetQuery = _context.MedicationRequests
                .Where(x => x.Status == (int)MedicationStatus.Snapshotted);

            if (snapshotId.HasValue && snapshotId.Value != Guid.Empty)
            {
                resetQuery = resetQuery.Where(x => x.SnapshotId == snapshotId.Value);
            }

            if (selectedProductIds != null && selectedProductIds.Any())
            {
                resetQuery = resetQuery.Where(x => !selectedProductIds.Contains(x.ProductId));
            }
            else if (selectedUniqIds != null && selectedUniqIds.Any())
            {
                resetQuery = resetQuery.Where(x => !selectedUniqIds.Contains(x.UniqId));
            }

            var itemsToReset = resetQuery.ToList();

            foreach (var item in itemsToReset)
            {
                item.Status = (int)MedicationStatus.Pending;
                item.SnapshotId = null;
                item.SnapshotDate = null;
            }

            if (!itemsToIssue.Any())
            {
                _context.SaveChanges();
                return;
            }

            var now = DateTime.Now;
            var batchId = Guid.NewGuid();
            var remittanceNo = $"REM-{now:yyyyMMdd-HHmmss}";

            foreach (var item in itemsToIssue)
            {
                item.Status = (int)MedicationStatus.Issued;
                item.IssuedDate = now;
                item.IssuedBy = userName;
            }

            // گروه‌بندی بر اساس distinct (WorkFlowHdrId, PersonId) جهت ایجاد لیست بسته‌بندی و درج در آرشیو
            var grouped = itemsToIssue
                .GroupBy(x => new { x.WorkFlowHdrId, x.PersonId })
                .ToList();

            foreach (var group in grouped)
            {
                var person = group.First().Person ?? _context.Persons.Find(group.Key.PersonId);
                var personelCode = person?.PersonelCode;
                var fullName = person != null ? $"{person.FirstName} {person.LastName}".Trim() : "نامشخص";
                var deliveryLoc = group.First().DeliveryLocation ?? string.Empty;

                // Concat لیست داروهای قبول شده این فرد
                var medList = string.Join(" | ", group.Select(m => $"{m.productName} ({m.Qty} عدد)"));

                // ذخیره در جدول آرشیو دائمی جهت خروجی اکسل و بسته‌بندی در هر زمانی
                foreach (var item in group)
                {
                    var archive = new MedicationRemittanceArchive
                    {
                        RemittanceBatchId = batchId,
                        RemittanceNumber = remittanceNo,
                        SnapshotId = item.SnapshotId,
                        WorkFlowHdrId = item.WorkFlowHdrId,
                        PersonId = item.PersonId,
                        PersonelCode = personelCode,
                        PersonFullName = fullName,
                        DeliveryLocation = deliveryLoc,
                        MedicationsSummary = medList,
                        ProductId = item.ProductId,
                        ProductName = item.productName,
                        Qty = item.Qty,
                        IssuedDate = now,
                        IssuedBy = userName,
                        CreatedDate = now
                    };
                    _context.MedicationRemittanceArchives.Add(archive);
                }

                // فراخوانی پروسجر جهت الحاق فرآیند و ارسال نوتیفیکیشن
                var fullText = $"[حواله انبار {remittanceNo} در تاریخ {now:yyyy/MM/dd HH:mm} توسط {userName}]: {medList}";
                SaveWorkflowAndNotify(group.Key.WorkFlowHdrId, group.Key.PersonId, issueWFFieldId, fullText, "ISSUE", userName);
            }

            _context.SaveChanges();
        }

        public List<MedicationRequest> GetIssuedRequests()
        {
            return _context.MedicationRequests
                .Where(x => x.Status == (int)MedicationStatus.Issued)
                .OrderByDescending(x => x.IssuedDate)
                .ToList();
        }

        /// <summary>
        /// واکشی داروهای حواله شده به همراه اطلاعات Person از طریق Include مستقیم EF
        /// </summary>
        public List<MedicationRequest> GetIssuedRequestsWithPerson(int? personelCode = null, string fullName = null, string deliveryLocation = null)
        {
            var query = _context.MedicationRequests
                .Include(x => x.Person)
                .Where(x => x.Status == (int)MedicationStatus.Issued);

            if (personelCode.HasValue)
            {
                query = query.Where(x => x.Person != null && x.Person.PersonelCode == personelCode.Value);
            }

            if (!string.IsNullOrWhiteSpace(fullName))
            {
                query = query.Where(x => x.Person != null && (x.Person.FirstName + " " + x.Person.LastName).Contains(fullName.Trim()));
            }

            if (!string.IsNullOrWhiteSpace(deliveryLocation))
            {
                query = query.Where(x => x.DeliveryLocation != null && x.DeliveryLocation.Equals(deliveryLocation.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            return query.OrderByDescending(x => x.IssuedDate).ToList();
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

        public List<MedicationRemittanceArchive> GetRemittanceArchives(Guid? batchId = null)
        {
            IQueryable<MedicationRemittanceArchive> query = _context.MedicationRemittanceArchives.AsNoTracking();

            if (batchId.HasValue && batchId.Value != Guid.Empty)
            {
                query = query.Where(x => x.RemittanceBatchId == batchId.Value);
            }

            return query.OrderByDescending(x => x.IssuedDate).ThenBy(x => x.PersonFullName).ToList();
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
