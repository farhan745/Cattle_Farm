# 🌐 Static Files & Client Assets Directory Guide
### 📂 Folder Location: `f:\VisualStudio\CattleFarm\CattleFarm\wwwroot`

The **wwwroot** folder holds the application's client-side assets and files. These files are served directly to the user's browser, bypassing the MVC routing pipeline.

---

## 🎯 Goal
To manage design stylesheets, interactive JavaScript libraries, third-party frameworks (Bootstrap, jQuery), and physical storage directories for user-uploaded media assets.

---

## 📅 Roadmap & Milestones
- **Week 1-2**: Initialize global CSS theme assets, register icons, and establish upload directories for profile pictures and cattle images.
- **Week 3-8**: Deploy Client libraries (such as Chart.js for financial analytics and scheduling calendars) and implement target script folders.

---

## 📋 Tasks & Deliverables
1. **Design Theme & Style**: Maintain [css/site.css](file:///f:/VisualStudio/CattleFarm/CattleFarm/wwwroot/css) to define colors, typography, responsiveness, and dark-mode styling.
2. **Client Scripting Interactivity**: Implement custom scripts under [js/](file:///f:/VisualStudio/CattleFarm/CattleFarm/wwwroot/js) for interactive forms, validation helpers, and AJAX calls.
3. **Local Media Upload Storage**: Maintain subdirectories under [uploads/](file:///f:/VisualStudio/CattleFarm/CattleFarm/wwwroot/uploads) to store images for cattle, farms, products, vets, and employee avatars.
4. **Third-Party Libraries**: Manage package installations inside [lib/](file:///f:/VisualStudio/CattleFarm/CattleFarm/wwwroot/lib) (e.g., jQuery, Bootstrap, Chart.js, FontAwesome).

---

## 🗃️ Assets & Media Directory Structure

| Folder Name | Asset Category | Description | Link |
| :--- | :--- | :--- | :--- |
| `css/` | Style Sheets | Custom CSS files including themes and styling. | [wwwroot/css](file:///f:/VisualStudio/CattleFarm/CattleFarm/wwwroot/css) |
| `js/` | JavaScript Modules | Client-side scripting for validations and chart loaders. | [wwwroot/js](file:///f:/VisualStudio/CattleFarm/CattleFarm/wwwroot/js) |
| `lib/` | Dependency Libraries | Third-party UI components (Bootstrap, jQuery, validation plugins). | [wwwroot/lib](file:///f:/VisualStudio/CattleFarm/CattleFarm/wwwroot/lib) |
| `uploads/cattle/` | Media Asset (Inventory) | Resized catalog photos for cattle profiles. | [uploads/cattle](file:///f:/VisualStudio/CattleFarm/CattleFarm/wwwroot/uploads/cattle) |
| `uploads/products/`| Media Asset (Storefront)| Product catalog images for the storefront. | [uploads/products](file:///f:/VisualStudio/CattleFarm/CattleFarm/wwwroot/uploads/products) |
| `uploads/avatars/` | Media Asset (Security)  | Uploaded user profile avatars. | [uploads/avatars](file:///f:/VisualStudio/CattleFarm/CattleFarm/wwwroot/uploads/avatars) |
| `uploads/doctors/` | Media Asset (Medical)   | Vet profile images. | [uploads/doctors](file:///f:/VisualStudio/CattleFarm/CattleFarm/wwwroot/uploads/doctors) |
| `uploads/workers/` | Media Asset (HR)        | Employee profile images. | [uploads/workers](file:///f:/VisualStudio/CattleFarm/CattleFarm/wwwroot/uploads/workers) |
| `uploads/farms/`   | Media Asset (Inventory) | Physical farm facility photos. | [uploads/farms](file:///f:/VisualStudio/CattleFarm/CattleFarm/wwwroot/uploads/farms) |

---

## 🏆 Milestone Contribution
This folder implements the **Client Assets & Media Storage Milestone**. It provides the CSS styles, interactive scripts, and file upload storage needed to build the user interface.
