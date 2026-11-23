using System;
using System.Collections.Generic;
using System.Linq;
using AMAR.Application.Contract.HR.DTO;
using AMAR.Application.Contract.HR.IService;
using AMAR.Application.HR.Mapper;
using AMAR.Domain.HR.IRepository;
using AMAR.Domain.HR.Model;

namespace AMAR.Application.HR.Service
{
    public class MedicationRequestService : IMedicationRequestService
    {
        private readonly IMedicationRequestRepository _repository;

        public MedicationRequestService(IMedicationRequestRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        // ======================= بخش انباردار =======================

        public List<MedicationSummaryDTO> GetPendingMedicationSummary()
        {
            var pendingRequests = _repository.GetPendingRequests();

            var summary = pendingRequests
                .GroupBy(x => new { x.ProductId, x.productName })
                .Select(g => new MedicationSummaryDTO
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.productName,
                    TotalRequests = g.Count(),
                    TotalQty = g.Sum(x => x.Qty)
                })
                .OrderBy(x => x.ProductName)
                .ToList();

            return summary;
        }

        public List<MedicationRequestDTO> GetPendingRequests()
        {
            var list = _repository.GetPendingRequests();
            return list.Select(MedicationRequestMapper.ToDTO).ToList();
        }

        public Guid TakeWarehouseSnapshot(string userName)
        {
            return _repository.CreateSnapshot(userName);
        }

        public List<MedicationRequestDTO> GetSnapshottedRequests(Guid? snapshotId = null)
        {
            var list = _repository.GetSnapshottedRequests(snapshotId);
            return list.Select(MedicationRequestMapper.ToDTO).ToList();
        }

        public void IssueWarehouseRemittance(List<int> selectedUniqIds, string userName, int issueWFFieldId)
        {
            if (selectedUniqIds == null || !selectedUniqIds.Any())
                throw new ArgumentException("هیچ دارویی برای صدور حواله انتخاب نشده است.");

            _repository.IssueRemittance(selectedUniqIds, userName, issueWFFieldId);
        }

        // ======================= بخش دبیرخانه =======================

        public List<MedicationRequestDTO> GetSecretariatRequests(SecretariatSearchFilterDTO filter)
        {
            // فراخوانی ریپازیتوری که Person را به صورت Include واکشی کرده است
            var list = _repository.GetIssuedRequestsWithPerson(
                filter?.PersonCode,
                filter?.FullName,
                filter?.DeliveryLocation
            );

            return list.Select(MedicationRequestMapper.ToDTO).ToList();
        }

        public void DeliverMedications(List<int> selectedUniqIds, string userName, int deliveryWFFieldId)
        {
            if (selectedUniqIds == null || !selectedUniqIds.Any())
                throw new ArgumentException("هیچ دارویی برای تحویل انتخاب نشده است.");

            _repository.DeliverMedications(selectedUniqIds, userName, deliveryWFFieldId);
        }

        public List<string> GetDeliveryLocations()
        {
            return _repository.GetDeliveryLocations();
        }
    }
}
