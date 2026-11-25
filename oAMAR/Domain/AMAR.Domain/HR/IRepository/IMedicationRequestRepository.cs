using System;
using System.Collections.Generic;
using System.Linq;
using AMAR.Domain.HR.Model;

namespace AMAR.Domain.HR.IRepository
{
    public interface IMedicationRequestRepository
    {
        IQueryable<MedicationRequest> GetAll();
        MedicationRequest GetById(int medicationRequestId);
        
        // متدهای انباردار
        List<MedicationRequest> GetPendingRequests();
        Guid CreateSnapshot(string userName, List<int> productIds = null);
        List<MedicationRequest> GetSnapshottedRequests(Guid? snapshotId = null);
        void IssueRemittance(List<int> selectedMedicationRequestIds, string userName, int issueWFFieldId);
        void IssueRemittance(List<int> selectedMedicationRequestIds, List<int> selectedProductIds, Guid? snapshotId, string userName, int issueWFFieldId);
        void RevertSnapshot(Guid? snapshotId, List<int> productIds = null);

        // متدهای دبیرخانه (اطلاعات شخص با Include لود می‌شود)
        List<MedicationRequest> GetIssuedRequests();
        List<MedicationRequest> GetIssuedRequestsWithPerson(int? personelCode = null, string fullName = null, string deliveryLocation = null);
        List<MedicationRequest> GetDeliveredRequestsWithPerson(int? personelCode = null, string fullName = null, string deliveryLocation = null);
        void DeliverMedications(List<int> selectedMedicationRequestIds, string userName, int deliveryWFFieldId);
        List<string> GetDeliveryLocations();

        // متدهای آرشیو حواله و لیست بسته‌بندی انبار جهت خروجی اکسل در هر زمان
        List<MedicationRemittanceArchive> GetRemittanceArchives(Guid? batchId = null);

        // اجرای پراسیجر الحاق فرآیند و ارسال پیامک/ایمیل
        void SaveWorkflowAndNotify(int workFlowHdrId, int personId, int wfFieldId, string medicationListText, string actionType, string userName);
    }
}
