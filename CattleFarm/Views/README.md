# 🖥️ Views Directory Guide
### 📂 Folder Location: `f:\VisualStudio\CattleFarm\CattleFarm\Views`

The **Views** folder contains the server-side Razor view pages (`.cshtml`). These files mix HTML structure with inline C# code to dynamically compile and serve user interfaces to the browser.

---

## 🎯 Goal
To render responsive, modern user interfaces that dynamically adjust based on user roles and database states.

---

## 📅 Roadmap & Milestones
Razor Views are organized into subfolders matching their respective Controllers and align with the project roadmap:
- **Week 1 (Security Screens)**: [Views/Account](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Account) houses Login and Registration views.
- **Week 2 (Inventory Grids)**: [Views/Farm](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Farm) and [Views/Cattle](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Cattle) list physical assets with picture upload forms.
- **Week 3 (Barn Data-Entry)**: [Views/Feed](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Feed) and [Views/MilkProduction](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/MilkProduction) provide mobile-friendly forms for logging production.
- **Week 4 (Veterinary Schedules)**: [Views/Doctor](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Doctor), [Views/Appointment](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Appointment), and [Views/Health](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Health) list medical histories.
- **Week 5 (Reproduction Calendars)**: [Views/Breeding](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Breeding) and [Views/Notification](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Notification) display alert notifications.
- **Week 6 (E-Shop Catalog)**: [Views/Product](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Product) and [Views/Order](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Order) show shopping carts and checkout forms.
- **Week 7 (Logistics Monitoring)**: [Views/Transport](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Transport) displays vehicle dispatch screens.
- **Week 8 (Analytics Panels)**: [Views/Dashboard](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Dashboard), [Views/Financial](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Financial), and [Views/Reports](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Reports) present Chart.js dashboards.

---

## 📋 Tasks & Deliverables
1. **Global Configuration Setup**: Define namespaces and import MVC Tag Helpers in [\_ViewImports.cshtml](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/_ViewImports.cshtml).
2. **Global View Layout Initialization**: Define default layouts in [\_ViewStart.cshtml](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/_ViewStart.cshtml).
3. **Master Layout Design**: Construct the shared master wrapper in `Shared/_Layout.cshtml` with navigation panels customized for user roles (`Admin`, `Owner`, `Manager`, `Worker`, `Customer`).
4. **Interactive Component Design**: Integrate Chart.js canvas containers, ClosedXML file downloads, and modal forms.

---

## 🗃️ View Folder Index

| View Folder | Associated Controller | Key Interfaces | Link |
| :--- | :--- | :--- | :--- |
| `Account/` | `AccountController` | Login, signups, and access denied screens. | [Views/Account](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Account) |
| `Cattle/` | `CattleController` | Inventory tables and cattle details. | [Views/Cattle](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Cattle) |
| `Farm/` | `FarmController` | Farm creation cards and profiles. | [Views/Farm](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Farm) |
| `MilkProduction/` | `MilkProductionController` | Daily milking session log forms. | [Views/MilkProduction](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/MilkProduction) |
| `Feed/` | `FeedController` | Nutrition logging cards. | [Views/Feed](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Feed) |
| `Doctor/` | `DoctorController` | Visiting veterinarian schedules. | [Views/Doctor](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Doctor) |
| `Appointment/` | `AppointmentController` | Animal medical visit schedulers. | [Views/Appointment](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Appointment) |
| `Health/` | `HealthController` | Diagnosis records, treatments, and prescriptions. | [Views/Health](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Health) |
| `Breeding/` | `BreedingController` | Gestation timelines and calving schedules. | [Views/Breeding](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Breeding) |
| `Product/` | `ProductController` | Storefront catalog and cart lists. | [Views/Product](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Product) |
| `Order/` | `OrderController` | Checkout forms and billing records. | [Views/Order](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Order) |
| `Transport/` | `TransportController` | Vehicle dispatches and trip progress tables. | [Views/Transport](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Transport) |
| `Dashboard/` | `DashboardController` | Graphical metrics and role summaries. | [Views/Dashboard](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Dashboard) |
| `Financial/` | `FinancialController` | Income statement inputs and reports. | [Views/Financial](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Financial) |
| `Reports/` | `ReportsController` | Excel export tools. | [Views/Reports](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Reports) |
| `Shared/` | *None* | Layout master framework (`_Layout.cshtml`). | [Views/Shared](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Shared) |

---

## 🏆 Milestone Contribution
This folder implements the **User Interface & Presentation Milestone**. By blending HTML structures, CSS style libraries, and Javascript scripts, it builds the interactive pages that users navigate.
