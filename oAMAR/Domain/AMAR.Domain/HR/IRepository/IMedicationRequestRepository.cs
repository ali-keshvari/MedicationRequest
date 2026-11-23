using System;
using System.Collections.Generic;
using System.Linq;
using AMAR.Domain.HR.Model;

namespace AMAR.Domain.HR.IRepository
{
    public interface IMedicationRequestRepository
    {
        IQueryable<MedicationRequest> GetAll();
        MedicationRequest GetById(int uniqId);
        
        // متدهای انباردار
        List<MedicationRequest> GetPendingRequests();
        Guid CreateSnapshot(string userName);
        List<MedicationRequest> GetSnapshottedRequests(Guid? snapshotId = null);
        void IssueRemittance(List<int> uniqIds, string userName, int issueWFFieldId);

        // متدهای دبیرخانه
        List<MedicationRequest> GetIssuedRequests();
        List<MedicationRequestWithPerson> GetIssuedRequestsWithPersonInfo(string personCode = null, string fullName = null, string deliveryLocation = null);
        void DeliverMedications(List<int> uniqIds, string userName, int deliveryWFFieldId);
        List<string> GetDeliveryLocations();

        // اجرای پراسیجر الحاق فرآیند و ارسال پیامک/ایمیل
        void SaveWorkflowAndNotify(int workFlowHdrId, int personId, int wfFieldId, string medicationListText, string actionType, string userName);
    }
}
