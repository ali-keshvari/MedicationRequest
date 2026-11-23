# سامانه مدیریت، صدور حواله و تحویل داروی سازمان (MedicationRequest)

یک سیستم جامع و لایه‌ای توسعه‌یافته بر بستر **ASP.NET Framework 4.8**، **Entity Framework 6.2.0** و **SQL Server** جهت مدیریت درخواست‌های داروی پرسنل سازمان پس از تایید پزشک طب کار، کارتابل انباردار و کارتابل دبیرخانه.

---

## 🏗 ساختار معماری و پروژه‌ها (Solution Architecture)

سولوشن `oAMAR.sln` شامل پروژه‌های لایه‌ای زیر است:

```
oAMAR/
├── Application/
│   ├── AMAR.Application/
│   │   └── HR/
│   │       ├── Mapper/MedicationRequestMapper.cs
│   │       └── Service/MedicationRequestService.cs
│   └── AMAR.Application.Contract/
│       └── HR/
│           ├── DTO/MedicationRequestDTO.cs
│           └── IService/IMedicationRequestService.cs
├── Domain/
│   └── AMAR.Domain/
│       └── HR/
│           ├── IRepository/IMedicationRequestRepository.cs
│           └── Model/MedicationRequest.cs
└── Infrastructure/
    └── AMAR.DataAccess.EF/
        ├── AmarDbContext.cs
        ├── Mapping/
        │   └── HR/MedicationRequestMapping.cs
        └── Repository/
            └── HR/MedicationRequestRepository.cs
pAMAR/
└── AMARPRJ/
    ├── Controllers/MedicationRequestController.cs
    └── Views/
        ├── Shared/_Layout.cshtml
        └── MedicationRequest/
            ├── Index.cshtml
            ├── Warehouse.cshtml
            └── Secretariat.cshtml
DatabaseScripts/
├── 01_CreateSchemaAndTables.sql
└── 02_StoredProcedures.sql
```

---

## 🔒 کلیدهای دسترسی (Security & Permissions)

کنترل دسترسی‌ها از طریق متد استاندارد سیستم به صورت زیر بررسی می‌شود:
```csharp
if (!genClass.CheckAccess(User.Identity.Name, "MedicationRequest"))
{
    return RedirectToAction("AccessDenied", "AccessPolicy", new { accessName = "MedicationRequest" });
}
```

* **`MedicationRequest`**: دسترسی پایه و ورود به ماژول دارو
* **`MedicationRequest_Warehouse`**: کارتابل انباردار
* **`MedicationRequest_Secretariat`**: کارتابل دبیرخانه

---

## ⚙️ گردش کار سیستم (Business Process)

1. **انباردار (Warehouse):**
   - مشاهده تعداد درخواست‌های موجود به تفکیک هر دارو و مجموع اقلام.
   - ثبت **Snapshot لحظه‌ای** برای فریز کردن تقاضای فعلی.
   - انتخاب و تیک زدن داروهایی که حواله آن‌ها صادر می‌شود.
   - تغییر وضعیت به «حواله شده» و ثبت تاریخ و نام انباردار.
   - گروه‌بندی بر اساس `(WorkFlowHdrId, PersonId)` و الحاق (Append) یا درج (Insert) لیست داروها در فرم فرآیند بر اساس `(WorkFlowHdrId, WFFieldId)` و ارسال پیامک و ایمیل اطلاع‌رسانی.

2. **دبیرخانه (Secretariat):**
   - مشاهده اقلام حواله شده با **تاریخ شمسی صدور حواله**.
   - اتصال (`JOIN`) به جدول `Person` جهت نمایش کد پرسنلی و نام کامل.
   - فیلتر پیشرفته بر اساس کد پرسنلی، نام و محل تحویل (`DeliveryLocation`).
   - تیک زدن اقلام و ثبت تحویل نهایی به همکار + الحاق وضعیت تحویل در فیلد دیگر فرم فرآیند.

---

## 🎨 ویژگی‌های رابط کاربری (UI/UX)
- استفاده از فونت مدرن فارسی **وزیرمتن (Vazirmatn)**
- طراحی کاملاً راست‌چین (**RTL**) با Bootstrap 5 و FontAwesome 6
- کارت‌های خلاصه آماری داینامیک
- اعتبارسنجی و پنجره‌های تایید قبل از صدور حواله یا ثبت تحویل
- سازگار با نمایشگرهای دسکتاپ و تبلت.
