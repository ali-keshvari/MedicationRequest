using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using AMAR.Application.Contract.HR.DTO;
using AMAR.Application.Contract.HR.IService;
using AMAR.Application.HR.Mapper;
using AMAR.DataAccess.EF;
using AMAR.Domain.HR.IRepository;
using AMAR.Domain.HR.Model;

namespace AMAR.Application.HR.Service
{
    public class MedicationRequestService : IMedicationRequestService
    {
        private readonly IMedicationRequestRepository _repository;
        private readonly AmarDbContext _context;

        public MedicationRequestService(
            IMedicationRequestRepository repository,
            AmarDbContext context = null)
        {
            _repository = repository;
            _context = context ?? new AmarDbContext();
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
                    ISNULL(p.PersonCode, '') AS PersonCode,
                    ISNULL(p.FirstName + ' ' + p.LastName, N'نامشخص') AS FullName,
                    ISNULL(p.NationalCode, '') AS NationalCode,
                    ISNULL(p.Mobile, '') AS Mobile
                FROM [gen].[MedicationRequest] req
                LEFT JOIN [dbo].[Person] p ON req.PersonId = p.PersonId
                WHERE req.Status = @status
            ";

            var rawResults = _context.Database.SqlQuery<SecretariatRawQueryResult>(
                sql, 
                new SqlParameter("@status", (int)MedicationStatus.Issued)
            ).ToList();

            IEnumerable<SecretariatRawQueryResult> filtered = rawResults;

            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.PersonCode))
                {
                    filtered = filtered.Where(x => x.PersonCode != null && x.PersonCode.Contains(filter.PersonCode.Trim()));
                }

                if (!string.IsNullOrWhiteSpace(filter.FullName))
                {
                    filtered = filtered.Where(x => x.FullName != null && x.FullName.Contains(filter.FullName.Trim()));
                }

                if (!string.IsNullOrWhiteSpace(filter.DeliveryLocation))
                {
                    filtered = filtered.Where(x => x.DeliveryLocation != null && x.DeliveryLocation.Equals(filter.DeliveryLocation.Trim(), StringComparison.OrdinalIgnoreCase));
                }
            }

            var dtos = filtered.Select(x => new MedicationRequestDTO
            {
                UniqId = x.UniqId,
                WorkFlowHdrId = x.WorkFlowHdrId,
                PersonId = x.PersonId,
                ProductId = x.ProductId,
                productName = x.productName,
                Qty = x.Qty,
                DeliveryLocation = x.DeliveryLocation,
                ShYear = x.ShYear,
                ShSeason = x.ShSeason,
                Status = x.Status,
                StatusTitle = MedicationRequestMapper.GetStatusTitle(x.Status),
                SnapshotId = x.SnapshotId,
                SnapshotDate = x.SnapshotDate,
                SnapshotDateShamsi = x.SnapshotDate.ToShamsi(),
                IssuedDate = x.IssuedDate,
                IssuedDateShamsi = x.IssuedDate.ToShamsi(),
                IssuedBy = x.IssuedBy,
                DeliveredDate = x.DeliveredDate,
                DeliveredDateShamsi = x.DeliveredDate.ToShamsi(),
                DeliveredBy = x.DeliveredBy,
                PersonCode = x.PersonCode,
                FullName = x.FullName,
                NationalCode = x.NationalCode,
                Mobile = x.Mobile
            }).OrderByDescending(x => x.IssuedDate).ToList();

            return dtos;
        }

        public void DeliverMedications(List<int> selectedUniqIds, string userName, int deliveryWFFieldId)
        {
            if (selectedUniqIds == null || !selectedUniqIds.Any())
                throw new ArgumentException("هیچ دارویی برای تحویل انتخاب نشده است.");

            _repository.DeliverMedications(selectedUniqIds, userName, deliveryWFFieldId);
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
    }
}
