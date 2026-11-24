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

        public Guid TakeWarehouseSnapshot(string userName, List<int> productIds = null)
        {
            return _repository.CreateSnapshot(userName, productIds);
        }

        public List<MedicationRequestDTO> GetSnapshottedRequests(Guid? snapshotId = null)
        {
            var list = _repository.GetSnapshottedRequests(snapshotId);
            return list.Select(MedicationRequestMapper.ToDTO).ToList();
        }

        public void IssueWarehouseRemittance(List<int> selectedMedicationRequestIds, string userName, int issueWFFieldId)
        {
            IssueWarehouseRemittance(selectedMedicationRequestIds, null, null, userName, issueWFFieldId);
        }

        public void IssueWarehouseRemittance(List<int> selectedMedicationRequestIds, List<int> selectedProductIds, Guid? snapshotId, string userName, int issueWFFieldId)
        {
            _repository.IssueRemittance(selectedMedicationRequestIds, selectedProductIds, snapshotId, userName, issueWFFieldId);
        }

        public List<RemittanceBatchSummaryDTO> GetRemittanceBatches()
        {
            var archives = _repository.GetRemittanceArchives();

            var batches = archives
                .GroupBy(x => new { x.RemittanceBatchId, x.RemittanceNumber, x.IssuedDate, x.IssuedBy })
                .Select(g => new RemittanceBatchSummaryDTO
                {
                    RemittanceBatchId = g.Key.RemittanceBatchId,
                    RemittanceNumber = g.Key.RemittanceNumber,
                    IssuedDate = g.Key.IssuedDate,
                    IssuedDateShamsi = ((DateTime?)g.Key.IssuedDate).ToShamsi(),
                    IssuedBy = g.Key.IssuedBy,
                    TotalPersonsCount = g.Select(x => x.PersonId).Distinct().Count(),
                    TotalItemsCount = g.Sum(x => x.Qty)
                })
                .OrderByDescending(x => x.IssuedDate)
                .ToList();

            return batches;
        }

        public List<MedicationPackagingSlipDTO> GetPackagingSlip(Guid? batchId = null)
        {
            var archives = _repository.GetRemittanceArchives(batchId);

            var slips = archives
                .GroupBy(x => new
                {
                    x.RemittanceBatchId,
                    x.RemittanceNumber,
                    x.WorkFlowHdrId,
                    x.PersonId,
                    x.PersonelCode,
                    x.PersonFullName,
                    x.DeliveryLocation,
                    x.MedicationsSummary,
                    x.IssuedDate,
                    x.IssuedBy
                })
                .Select(g => new MedicationPackagingSlipDTO
                {
                    RemittanceBatchId = g.Key.RemittanceBatchId,
                    RemittanceNumber = g.Key.RemittanceNumber,
                    WorkFlowHdrId = g.Key.WorkFlowHdrId,
                    PersonId = g.Key.PersonId,
                    PersonelCode = g.Key.PersonelCode,
                    PersonFullName = g.Key.PersonFullName,
                    DeliveryLocation = string.IsNullOrWhiteSpace(g.Key.DeliveryLocation) ? "دبیرخانه مرکزی" : g.Key.DeliveryLocation,
                    MedicationsSummary = g.Key.MedicationsSummary,
                    TotalItemsCount = g.Sum(x => x.Qty),
                    IssuedDate = g.Key.IssuedDate,
                    IssuedDateShamsi = ((DateTime?)g.Key.IssuedDate).ToShamsi(),
                    IssuedBy = g.Key.IssuedBy,
                    Items = g.Select(i => new PackagingItemLineDTO
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        Qty = i.Qty
                    }).ToList()
                })
                .OrderBy(x => x.DeliveryLocation)
                .ThenBy(x => x.PersonFullName)
                .ToList();

            return slips;
        }

        // ======================= بخش دبیرخانه =======================

        public List<MedicationRequestDTO> GetSecretariatRequests(SecretariatSearchFilterDTO filter)
        {
            var list = _repository.GetIssuedRequestsWithPerson(
                filter?.PersonelCode,
                filter?.FullName,
                filter?.DeliveryLocation
            );

            return list.Select(MedicationRequestMapper.ToDTO).ToList();
        }

        public List<MedicationRequestDTO> GetDeliveredRequests(SecretariatSearchFilterDTO filter)
        {
            var list = _repository.GetDeliveredRequestsWithPerson(
                filter?.PersonelCode,
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
