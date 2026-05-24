# 🤝 Unit of Work Directory Guide
### 📂 Folder Location: `f:\VisualStudio\CattleFarm\CattleFarm\UnitOfWork`

The **UnitOfWork** folder implements the Unit of Work design pattern. It aggregates all database repositories, ensuring they share a single, unified Entity Framework `DbContext` instance.

---

## 🎯 Goal
To coordinate transactions across multiple repositories. When a business request modifies several entities (e.g., submitting an order, reducing store inventory, and creating a shipping request), the Unit of Work guarantees that all updates are committed or rolled back together as a single transaction.

---

## 📅 Roadmap & Milestones
- **Week 1 Setup**: The [IUnitOfWork](file:///f:/VisualStudio/CattleFarm/CattleFarm/UnitOfWork/IUnitOfWork.cs) interface and [UnitOfWork](file:///f:/VisualStudio/CattleFarm/CattleFarm/UnitOfWork/UnitOfWork.cs) implementation are established during Week 1 and registered as scoped dependencies inside `Program.cs`.
- **Throughout Development**: New repository properties are registered in the Unit of Work whenever new tables and models are introduced.

---

## 📋 Tasks & Deliverables
1. **Aggregating Repositories**: Define and implement readonly properties on `UnitOfWork` representing each repository interface (e.g., `ICattleRepository`, `IUserRepository`, `IOrderRepository`).
2. **Coordinating Transactions**: Provide a single `SaveChangesAsync()` method that executes a database transaction commit for all pending entity updates.
3. **Resource Disposal**: Implement `IDisposable` to ensure the underlying Entity Framework DbContext is disposed at the end of the HTTP request lifecycle.

---

## 🗃️ Files & Modules

| File Name | Purpose | Link |
| :--- | :--- | :--- |
| `IUnitOfWork.cs` | Interface listing all repository properties and transaction commit actions. | [IUnitOfWork.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/UnitOfWork/IUnitOfWork.cs) |
| `UnitOfWork.cs` | Concrete class that instantiates repositories, shares the `CattleFarmDbContext`, and saves changes. | [UnitOfWork.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/UnitOfWork/UnitOfWork.cs) |

---

## 🏆 Milestone Contribution
This folder implements the **Transactional Integrity Milestone**. It ensures that complex multi-table updates are completed successfully together, preventing partial database writes.
