# 📦 Repositories Directory Guide
### 📂 Folder Location: `f:\VisualStudio\CattleFarm\CattleFarm\Repositories`

The **Repositories** folder implements the Repository pattern to abstract database operations. By encapsulating Entity Framework Core (EF Core) DB queries, it separates the business logic layer (Services) from data access.
The directory is split into:
1. 📁 **[Interfaces](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/Interfaces)**: Defines contract methods for database queries.
2. 📁 **[Implementations](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/Implementations)**: Concrete classes executing queries against the `CattleFarmDbContext`.

---

## 🎯 Goal
To provide a clean, standardized data access layer. It isolates database query logic, simplifies unit testing via in-memory database mocks, and prevents direct DB connections from leaking into services or controllers.

---

## 📅 Roadmap & Milestones
Repository implementations scale with each weekly phase of the database structure:
- **Week 1**: User authentication queries via [IUserRepository](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/Interfaces/IUserRepository.cs) / [UserRepository](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/Implementations/UserRepository.cs).
- **Week 2**: Cattle query operations via [ICattleRepository](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/Interfaces/ICattleRepository.cs) / [CattleRepository](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/Implementations/CattleRepository.cs) and [IFarmRepository](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/Interfaces/IFarmRepository.cs) / [FarmRepository](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/Implementations/FarmRepository.cs).
- **Week 3**: Yield tracking queries via [IMilkProductionRepository](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/Interfaces/IMilkProductionRepository.cs) / [MilkProductionRepository](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/Implementations/MilkProductionRepository.cs) and [IFeedRepository](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/Interfaces/IFeedRepository.cs) / [FeedRepository](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/Implementations/FeedRepository.cs).
- **Week 4-8**: Specific repositories created to support appointments, breeding calendars, orders, logistics dispatches, and financial tracking.

---

## 📋 Tasks & Deliverables
1. **Generic Repository Interface**: Build [IRepository.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/Interfaces/IRepository.cs) containing general operations: `GetByIdAsync`, `GetAllAsync`, `FindAsync`, `AddAsync`, `Update`, and `Delete`.
2. **Generic Repository Implementation**: Write [Repository.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/Implementations/Repository.cs) inheriting `IRepository<T>` to handle standard EF Core `DbSet<T>` interactions.
3. **Custom Entity Repositories**: Implement custom query methods (e.g. `CattleRepository.GetCattleWithFarmDetailsAsync`) to handle SQL JOINs, filtering, and eager loading of related entities (using `.Include()`).
4. **Performance Tuning**: Apply `.AsNoTracking()` on read-only queries to bypass EF Core tracking overhead.

---

## 🗃️ Repository Layer Map

| Entity Type | Interface Contract | Implementation Class | Query Focus |
| :--- | :--- | :--- | :--- |
| **Generic Entity** | [IRepository.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/Interfaces/IRepository.cs) | [Repository.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/Implementations/Repository.cs) | Core CRUD commands for all database tables. |
| **User Accounts** | [IUserRepository.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/Interfaces/IUserRepository.cs) | [UserRepository.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/Implementations/UserRepository.cs) | Locates users by Email, loading roles. |
| **Cattle Inventory** | [ICattleRepository.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/Interfaces/ICattleRepository.cs) | [CattleRepository.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/Implementations/CattleRepository.cs) | Queries active cattle with associated farm records. |
| **Breeding Logs** | [IBreedingRepository.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/Interfaces/IBreedingRepository.cs) | [BreedingRepository.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/Implementations/BreedingRepository.cs) | Queries pregnant cows and estimated calving windows. |
| **Daily Milk** | [IMilkProductionRepository.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/Interfaces/IMilkProductionRepository.cs) | [MilkProductionRepository.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/Implementations/MilkProductionRepository.cs) | Compiles total yields per cow or farm. |
| **Order Pipeline** | [IOrderRepository.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/Interfaces/IOrderRepository.cs) | [OrderRepository.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/Implementations/OrderRepository.cs) | Tracks orders containing customer details and order items. |
| **Trip Logistics** | [ITripRepository.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/Interfaces/ITripRepository.cs) | [TripRepository.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/Implementations/TripRepository.cs) | Queries logistics schedules, drivers, and vehicles. |

---

## 🏆 Milestone Contribution
This folder contributes to the **Data Abstraction Milestone**. It ensures that DB connection details and SQL-specific query logic remain decoupled from the service layers.
