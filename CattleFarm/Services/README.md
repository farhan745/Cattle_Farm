# ⚙️ Services Directory Guide
### 📂 Folder Location: `f:\VisualStudio\CattleFarm\CattleFarm\Services`

The **Services** folder contains the application's business logic layer. To decouple controllers and data layers, the service layer is structured into:
1. 📁 **[Interfaces](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces)**: Defines contract protocols for each service.
2. 📁 **[Implementations](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations)**: Concrete classes executing the business computations, validation checks, and API calls.

---

## 🎯 Goal
To consolidate and execute the core business operations (e.g. computing pregnancy dates, compiling dashboards, connecting to secure payment webhooks, checking worker attendance, and generating reports) while maintaining transaction isolation.

---

## 📅 Roadmap & Milestones
Services are registered inside `Program.cs` as scoped or singleton resources and map directly to weekly deliverables:
- **Week 1**: [IAuthService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces/IAuthService.cs) / [AuthService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/AuthService.cs) handle password encryption and cookie claims.
- **Week 2**: [IFarmService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces/IFarmService.cs) / [FarmService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/FarmService.cs), [ICattleService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces/ICattleService.cs) / [CattleService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/CattleService.cs), and [IImageService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces/IImageService.cs) / [ImageService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/ImageService.cs) resize and save uploaded cattle photos.
- **Week 3**: [IMilkService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces/IMilkService.cs) / [MilkService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/MilkService.cs) computes milking sessions and yield volumes.
- **Week 4**: [IDoctorService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces/IDoctorService.cs) / [DoctorService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/DoctorService.cs), [IAppointmentService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces/IAppointmentService.cs) / [AppointmentService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/AppointmentService.cs), [IHealthService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces/IHealthService.cs) / [HealthService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/HealthService.cs), and [IVaccinationService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces/IVaccinationService.cs) / [VaccinationService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/VaccinationService.cs) handle clinical tasks.
- **Week 5**: [INotificationService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces/INotificationService.cs) / [NotificationService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/NotificationService.cs) pushes pregnancy reminders.
- **Week 6**: [IProductService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces/IProductService.cs) / [ProductService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/ProductService.cs) and [IOrderService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces/IOrderService.cs) / [OrderService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/OrderService.cs) handle ecommerce logic.
- **Week 7**: [IPaymentGatewayService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces/IPaymentGatewayService.cs) / [SslCommerzService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/SslCommerzService.cs) handles online payments, and [ITransportService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces/ITransportService.cs) / [TransportService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/TransportService.cs) maps driver dispatches.
- **Week 8**: [IDashboardService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces/IDashboardService.cs) / [DashboardService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/DashboardService.cs) aggregates analytics, [IFinancialService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces/IFinancialService.cs) / [FinancialService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/FinancialService.cs) processes cash flows, and [IAuditService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces/IAuditService.cs) / [AuditService](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/AuditService.cs) writes operational trails.

---

## 📋 Tasks & Deliverables
1. **Interface Definition**: Define abstract contracts inside the `Interfaces` directory.
2. **Business Process Implementation**: Code the logical actions, calculations, validation constraints, and database modifications inside the `Implementations` directory.
3. **External Gateway Connectivity**: Leverage HttpClient integration in `SslCommerzService` for validating online payments, and MailKit in `EmailService` for sending alerts.
4. **Photo Processing Logic**: Handle local folder media uploads using the ImageSharp library in `ImageService`.

---

## 🗃️ Services Map (Interfaces & Implementations)

| Service Area | Contract Interface | Concrete Implementation | Primary Focus |
| :--- | :--- | :--- | :--- |
| **Authentication** | [IAuthService.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces/IAuthService.cs) | [AuthService.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/AuthService.cs) | Validates credentials, issues secure cookie identities, handles hashing. |
| **Cattle Inventory** | [ICattleService.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces/ICattleService.cs) | [CattleService.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/CattleService.cs) | Implements validation rules for cattle weight, status, and tag assignments. |
| **Logistics** | [ITransportService.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces/ITransportService.cs) | [TransportService.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/TransportService.cs) | Dispatches drivers, calculates fuel usage, and coordinates delivery dispatches. |
| **Commerce** | [IOrderService.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces/IOrderService.cs) | [OrderService.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/OrderService.cs) | Audits shopping carts, checks inventory stock, and creates payment requests. |
| **Payments** | [IPaymentGatewayService.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces/IPaymentGatewayService.cs) | [SslCommerzService.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/SslCommerzService.cs) | Integrates SSLCommerz payment options, sandbox webhooks, and status checks. |
| **Health** | [IHealthService.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces/IHealthService.cs) | [HealthService.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/HealthService.cs) | Tracks clinical visits, medication regimens, and vaccine scheduling. |
| **HR & Payroll** | [IPayrollService.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces/IPayrollService.cs) | [PayrollService.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/PayrollService.cs) | Computes employee wages, attendance statistics, and monthly payrolls. |
| **Analytics** | [IDashboardService.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces/IDashboardService.cs) | [DashboardService.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/DashboardService.cs) | Pulls metrics for livestock counts, milk yields, and cash flows. |
| **Images** | [IImageService.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces/IImageService.cs) | [ImageService.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/ImageService.cs) | Saves and optimizes profile images. |
| **Audit Trails** | [IAuditService.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces/IAuditService.cs) | [AuditService.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/AuditService.cs) | Captures database updates for security logging. |

---

## 🏆 Milestone Contribution
This folder implements the **Business Logic Milestone**. It houses the validation algorithms, calculations, and integrations that drive the application's core functionality.
