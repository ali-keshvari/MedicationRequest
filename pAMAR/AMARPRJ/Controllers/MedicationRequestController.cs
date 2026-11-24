using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AMAR.Application.Contract.HR.DTO;
using AMAR.Application.Contract.HR.IService;
using AMAR.Application.HR.Service;
using AMAR.DataAccess.EF.Repository.HR;

namespace AMARPRJ.Controllers
{
    // کلاس کمکی فرضی genClass در صورت عدم وجود در پروژه اصلی
    public static class genClass
    {
        public static bool CheckAccess(string userName, string accessName)
        {
            // در سیستم اصلی شما این متد از بانک اطلاعاتی دسترسی‌ها را بررسی می‌کند
            return true;
        }
    }

    [Authorize]
    public class MedicationRequestController : Controller
    {
        private readonly IMedicationRequestService _medicationService;

        // پیش‌فرض شناسه فیلدهای فرآیند جهت درج/الحاق اطلاعات
        private const int DEFAULT_ISSUE_WF_FIELD_ID = 101;     // شناسه فیلد فرآیند برای حواله انبار
        private const int DEFAULT_DELIVERY_WF_FIELD_ID = 102;  // شناسه فیلد فرآیند برای تحویل دبیرخانه

        // دسترسی‌های سیستم
        public const string ACCESS_MAIN = "MedicationRequest";
        public const string ACCESS_WAREHOUSE = "MedicationRequest_Warehouse";
        public const string ACCESS_SECRETARIAT = "MedicationRequest_Secretariat";

        public MedicationRequestController()
        {
            var repo = new MedicationRequestRepository();
            _medicationService = new MedicationRequestService(repo);
        }

        public MedicationRequestController(IMedicationRequestService medicationService)
        {
            _medicationService = medicationService;
        }

        // =========================================================================
        // متدهای بررسی دسترسی متمرکز (Access Control Helpers)
        // =========================================================================
        /// <summary>
        /// بررسی متمرکز دسترسی کاربر به بخش‌های مختلف ماژول دارو
        /// </summary>
        public static bool CheckUserAccess(string accessName, string userName = null)
        {
            if (string.IsNullOrEmpty(userName))
            {
                if (System.Web.HttpContext.Current != null &&
                    System.Web.HttpContext.Current.User != null &&
                    System.Web.HttpContext.Current.User.Identity != null)
                {
                    userName = System.Web.HttpContext.Current.User.Identity.Name;
                }
            }

            if (string.IsNullOrEmpty(userName))
                return false;

            return genClass.CheckAccess(userName, accessName);
        }

        /// <summary>
        /// دسترسی به کارتابل انباردار و آرشیو حواله
        /// </summary>
        public static bool CanAccessWarehouse(string userName = null)
        {
            return CheckUserAccess(ACCESS_WAREHOUSE, userName);
        }

        /// <summary>
        /// دسترسی به کارتابل دبیرخانه
        /// </summary>
        public static bool CanAccessSecretariat(string userName = null)
        {
            return CheckUserAccess(ACCESS_SECRETARIAT, userName);
        }

        // =========================================================================
        // صفحه اصلی / داشبورد و هدایت بر اساس دسترسی
        // =========================================================================
        public ActionResult Index()
        {
            bool hasWarehouse = CheckUserAccess( ACCESS_WAREHOUSE);
            bool hasSecretariat = CheckUserAccess( ACCESS_SECRETARIAT);
            bool hasMain = CheckUserAccess( ACCESS_MAIN);

            if (!hasMain && !hasWarehouse && !hasSecretariat)
            {
                return RedirectToAction("AccessDenied", "AccessPolicy", new { accessName = ACCESS_MAIN });
            }

            // اگر کاربر همزمان به هر دو کارتابل دسترسی دارد، داشبورد انتخاب کارتابل نمایش داده شود
            if (hasWarehouse && hasSecretariat)
            {
                ViewBag.HasWarehouse = true;
                ViewBag.HasSecretariat = true;
                return View();
            }

            // اگر کاربر صرفاً دسترسی انباردار دارد، مستقیماً هدایت شود
            if (hasWarehouse)
            {
                return RedirectToAction("Warehouse");
            }

            // اگر کاربر صرفاً دسترسی دبیرخانه دارد، مستقیماً هدایت شود
            if (hasSecretariat)
            {
                return RedirectToAction("Secretariat");
            }

            return View();
        }

        // =========================================================================
        // بخش انباردار (Warehouse)
        // =========================================================================
        public ActionResult Warehouse(Guid? snapshotId = null)
        {
            if (!CheckUserAccess( ACCESS_WAREHOUSE))
            {
                return RedirectToAction("AccessDenied", "AccessPolicy", new { accessName = ACCESS_WAREHOUSE });
            }

            // دریافت لیست تجمیعی به ازای هر دارو (چند درخواست وجود دارد)
            var summary = _medicationService.GetPendingMedicationSummary();
            ViewBag.MedicationSummary = summary;

            // دریافت لیست رکوردهای اسنپ‌شات شده برای تیک زدن و صدور حواله
            var snapshottedItems = _medicationService.GetSnapshottedRequests(snapshotId);
            ViewBag.ActiveSnapshotId = snapshotId ?? (snapshottedItems.Any() ? snapshottedItems.FirstOrDefault().SnapshotId : (Guid?)null);
            ViewBag.DefaultWFFieldId = DEFAULT_ISSUE_WF_FIELD_ID;

            return View(snapshottedItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TakeSnapshot(List<int> selectedProductIds)
        {
            if (!CheckUserAccess(ACCESS_WAREHOUSE))
            {
                return RedirectToAction("AccessDenied", "AccessPolicy", new { accessName = ACCESS_WAREHOUSE });
            }

            if (selectedProductIds == null || !selectedProductIds.Any())
            {
                TempData["WarningMessage"] = "لطفاً حداقل یک قلم دارو را برای ثبت اسنپ‌شات انتخاب نمایید.";
                return RedirectToAction("Warehouse");
            }

            try
            {
                var snapshotId = _medicationService.TakeWarehouseSnapshot(User.Identity.Name, selectedProductIds);
                if (snapshotId == Guid.Empty)
                {
                    TempData["WarningMessage"] = "هیچ درخواستی در وضعیت در انتظار برای داروهای انتخابی یافت نشد.";
                    return RedirectToAction("Warehouse");
                }

                TempData["SuccessMessage"] = $"اسنپ‌شات لحظه‌ای برای {selectedProductIds.Count} قلم داروی انتخابی با موفقیت ثبت شد و آماده صدور حواله می‌باشد.";
                return RedirectToAction("Warehouse", new { snapshotId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "خطا در ثبت اسنپ‌شات: " + ex.Message;
                return RedirectToAction("Warehouse");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IssueRemittance(List<int> selectedProductIds, List<int> selectedUniqIds, Guid? activeSnapshotId, int? wfFieldId)
        {
            if (!CheckUserAccess(ACCESS_WAREHOUSE))
            {
                return RedirectToAction("AccessDenied", "AccessPolicy", new { accessName = ACCESS_WAREHOUSE });
            }

            bool hasSelectedProducts = selectedProductIds != null && selectedProductIds.Any();
            bool hasSelectedUniqs = selectedUniqIds != null && selectedUniqIds.Any();
            int targetFieldId = wfFieldId ?? DEFAULT_ISSUE_WF_FIELD_ID;

            try
            {
                if (!hasSelectedProducts && !hasSelectedUniqs)
                {
                    _medicationService.IssueWarehouseRemittance(null, null, activeSnapshotId, User.Identity.Name, targetFieldId);
                    TempData["WarningMessage"] = "چون هیچ دارویی انتخاب نشد، تمام اقلام این اسنپ‌شات لغو شده و به صف انتظار (Status = 0) بازگردانده شدند.";
                    return RedirectToAction("Warehouse");
                }

                _medicationService.IssueWarehouseRemittance(selectedUniqIds, selectedProductIds, activeSnapshotId, User.Identity.Name, targetFieldId);

                TempData["SuccessMessage"] = "حواله انبار برای داروهای انتخابی با موفقیت صادر شد و سایر اقلام تیک‌نخورده به صف انتظار (Status = 0) بازگشتند.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "خطا در صدور حواله انبار: " + ex.Message;
            }

            return RedirectToAction("Warehouse");
        }

        // =========================================================================
        // بخش آرشیو حواله‌ها، لیست بسته‌بندی دارو و خروجی اکسل
        // =========================================================================
        public ActionResult RemittanceArchive(Guid? batchId = null)
        {
            if (!CheckUserAccess( ACCESS_WAREHOUSE) && !CheckUserAccess( ACCESS_SECRETARIAT))
            {
                return RedirectToAction("AccessDenied", "AccessPolicy", new { accessName = ACCESS_WAREHOUSE });
            }

            var batches = _medicationService.GetRemittanceBatches();
            ViewBag.Batches = batches;

            var targetBatchId = batchId ?? batches.FirstOrDefault()?.RemittanceBatchId;
            ViewBag.SelectedBatchId = targetBatchId;

            var packagingSlips = _medicationService.GetPackagingSlip(targetBatchId);
            return View(packagingSlips);
        }

        public ActionResult ExportPackagingToExcel(Guid? batchId = null)
        {
            if (!CheckUserAccess( ACCESS_WAREHOUSE) && !CheckUserAccess( ACCESS_SECRETARIAT))
            {
                return RedirectToAction("AccessDenied", "AccessPolicy", new { accessName = ACCESS_WAREHOUSE });
            }

            var slips = _medicationService.GetPackagingSlip(batchId);
            var batchNo = slips.FirstOrDefault()?.RemittanceNumber ?? "All";

            var sb = new System.Text.StringBuilder();
            sb.Append("<html xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:x='urn:schemas-microsoft-com:office:excel' xmlns='http://www.w3.org/TR/REC-html40'>");
            sb.Append("<head><meta http-equiv='Content-Type' content='text/html; charset=utf-8' />");
            sb.Append("<!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>PackagingSlip</x:Name><x:WorksheetOptions><x:DisplayRightToLeft/></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]-->");
            sb.Append("<style>th { background-color: #2563eb; color: #ffffff; font-weight: bold; } td, th { border: 1px solid #cbd5e1; padding: 6px 12px; font-family: Tahoma, Arial; font-size: 11pt; text-align: center; } .text-right { text-align: right; } </style>");
            sb.Append("</head><body dir='rtl'>");
            sb.Append("<h3 style='text-align:center;'>لیست تفکیکی بسته‌بندی و تحویل داروی پرسنل (حواله: " + batchNo + ")</h3>");
            sb.Append("<table border='1' cellspacing='0' cellpadding='4' dir='rtl'>");
            sb.Append("<thead><tr>");
            sb.Append("<th>ردیف</th>");
            sb.Append("<th>کد پرسنلی</th>");
            sb.Append("<th>نام و نام خانوادگی</th>");
            sb.Append("<th>شماره فرآیند</th>");
            sb.Append("<th>محل تحویل (DeliveryLocation)</th>");
            sb.Append("<th>داروهای تجمیعی جهت بسته‌بندی</th>");
            sb.Append("<th>تعداد کل اقلام</th>");
            sb.Append("<th>شماره حواله</th>");
            sb.Append("<th>تاریخ حواله</th>");
            sb.Append("<th>صادرکننده</th>");
            sb.Append("</tr></thead><tbody>");

            int index = 1;
            foreach (var item in slips)
            {
                sb.Append("<tr>");
                sb.Append($"<td>{index++}</td>");
                sb.Append($"<td>{item.PersonelCode}</td>");
                sb.Append($"<td class='text-right'><strong>{item.PersonFullName}</strong></td>");
                sb.Append($"<td>{item.WorkFlowHdrId}</td>");
                sb.Append($"<td><strong>{item.DeliveryLocation}</strong></td>");
                sb.Append($"<td class='text-right'>{item.MedicationsSummary}</td>");
                sb.Append($"<td>{item.TotalItemsCount}</td>");
                sb.Append($"<td>{item.RemittanceNumber}</td>");
                sb.Append($"<td>{item.IssuedDateShamsi}</td>");
                sb.Append($"<td>{item.IssuedBy}</td>");
                sb.Append("</tr>");
            }

            sb.Append("</tbody></table></body></html>");

            var fileName = $"MedicationPackaging_{batchNo}_{DateTime.Now:yyyyMMdd}.xls";
            return File(System.Text.Encoding.UTF8.GetBytes(sb.ToString()), "application/vnd.ms-excel", fileName);
        }

        public ActionResult PrintPackagingLabels(Guid? batchId = null)
        {
            if (!CheckUserAccess( ACCESS_WAREHOUSE) && !CheckUserAccess( ACCESS_SECRETARIAT))
            {
                return RedirectToAction("AccessDenied", "AccessPolicy", new { accessName = ACCESS_WAREHOUSE });
            }

            var slips = _medicationService.GetPackagingSlip(batchId);
            return View(slips);
        }

        // =========================================================================
        // بخش دبیرخانه (Secretariat)
        // =========================================================================
        public ActionResult Secretariat(SecretariatSearchFilterDTO filter)
        {
            if (!CheckUserAccess( ACCESS_SECRETARIAT))
            {
                return RedirectToAction("AccessDenied", "AccessPolicy", new { accessName = ACCESS_SECRETARIAT });
            }

            var issuedList = _medicationService.GetSecretariatRequests(filter);

            ViewBag.Locations = _medicationService.GetDeliveryLocations();
            ViewBag.Filter = filter ?? new SecretariatSearchFilterDTO();
            ViewBag.DefaultWFFieldId = DEFAULT_DELIVERY_WF_FIELD_ID;

            return View(issuedList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeliverMedications(List<int> selectedMedicationRequestIds, List<int> selectedUniqIds, int? wfFieldId)
        {
            if (!CheckUserAccess( ACCESS_SECRETARIAT))
            {
                return RedirectToAction("AccessDenied", "AccessPolicy", new { accessName = ACCESS_SECRETARIAT });
            }

            if (selectedUniqIds == null || !selectedUniqIds.Any())
            {
                TempData["ErrorMessage"] = "لطفاً حداقل یک قلم دارو برای تحویل انتخاب نمایید.";
                return RedirectToAction("Secretariat");
            }

            try
            {
                int targetFieldId = wfFieldId ?? DEFAULT_DELIVERY_WF_FIELD_ID;
                _medicationService.DeliverMedications(selectedUniqIds, User.Identity.Name, targetFieldId);

                TempData["SuccessMessage"] = $"داروهای انتخابی به پرسنل تحویل داده شدند و فرآیند تکمیل گردید.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "خطا در ثبت تحویل دارو: " + ex.Message;
            }

            return RedirectToAction("Secretariat");
        }
    }
}
