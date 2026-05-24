# 📋 ViewModels Directory Guide
### 📂 Folder Location: `f:\VisualStudio\CattleFarm\CattleFarm\ViewModels`

The **ViewModels** folder contains the application's Data Transfer Objects (DTOs) and view-specific models. These classes shape data formatted for Razor Views, ensuring database entities are decoupled from user interfaces.

---

## 🎯 Goal
To isolate the database layer from the user interface, prevent security vulnerabilities (such as over-posting attacks), and enforce input validations before data is passed to the database.

---

## 📅 Roadmap & Milestones
ViewModels are introduced alongside UI forms across the project lifecycle:
- **Week 1 (Authentication Forms)**: [LoginViewModel.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/ViewModels/LoginViewModel.cs) and [RegisterViewModel.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/ViewModels/RegisterViewModel.cs) map credentials.
- **Week 2-7 (Domain Inputs)**: [DomainViewModels.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/ViewModels/DomainViewModels.cs) groups models for Livestock, Farms, Breeding logs, Logistics dispatches, and Orders.
- **HR & Management**: [AttendanceViewModel.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/ViewModels/AttendanceViewModel.cs) and [EmployeePayrollViewModels.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/ViewModels/EmployeePayrollViewModels.cs) manage employee records.
- **System Settings**: [ChangeRoleViewModel.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/ViewModels/ChangeRoleViewModel.cs) handles role modifications.

---

## 📋 Tasks & Deliverables
1. **Form Binding Models**: Create view-specific input properties matching HTML form inputs.
2. **Declarative Input Validation**: Set annotations like `[Required]`, `[EmailAddress]`, `[Compare]`, and `[Range]` to trigger automatic ModelState validations in controllers.
3. **Compound View Models**: Structure complex view models that combine list data, pagination details, and dropdown choices to feed UI rendering requirements.

---

## 🗃️ ViewModels List

| File Name | Purpose | Target Action View | Link |
| :--- | :--- | :--- | :--- |
| `LoginViewModel.cs` | Binds Login inputs (Email, Password, RememberMe). | `Account/Login` | [LoginViewModel.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/ViewModels/LoginViewModel.cs) |
| `RegisterViewModel.cs` | Binds Register fields, ensuring password confirmations match. | `Account/Register` | [RegisterViewModel.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/ViewModels/RegisterViewModel.cs) |
| `DomainViewModels.cs` | Consolidates models for Farms, Cattle, Breeding, Milk production, Doctors, and Orders. | Multiple CRUD Views | [DomainViewModels.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/ViewModels/DomainViewModels.cs) |
| `AttendanceViewModel.cs` | Records daily check-in times and attendance logs. | `Attendance/` | [AttendanceViewModel.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/ViewModels/AttendanceViewModel.cs) |
| `EmployeePayrollViewModels.cs`| Binds payroll calculations, wages, and working hours. | `Payroll/` | [EmployeePayrollViewModels.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/ViewModels/EmployeePayrollViewModels.cs) |
| `ChangeRoleViewModel.cs` | Manages role changes for users. | `UserManagement/` | [ChangeRoleViewModel.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/ChangeRoleViewModel.cs) |

---

## 🏆 Milestone Contribution
This folder contributes to the **Secure Model Binding Milestone**. It acts as a security filter that shields the EF Core models from direct user input.
