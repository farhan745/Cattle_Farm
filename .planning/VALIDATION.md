---
phase: 3
slug: ui-ux-modernization
status: completed
nyquist_compliant: true
wave_0_complete: true
created: 2026-06-01
---

# Phase 3 — UI/UX Modernization & Grid Standardisation — Validation Strategy

> Per-phase validation contract for feedback sampling during execution and verification gates.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | Dotnet Compiler (Static Code Verification) & Manual Visual Responsive Audit |
| **Config file** | `CattleFarm.csproj` |
| **Quick run command** | `dotnet build CattleFarm/CattleFarm.csproj --no-incremental -v minimal` |
| **Full suite command** | `dotnet build CattleFarm/CattleFarm.csproj --no-incremental -v minimal` |
| **Estimated runtime** | ~5 seconds |

---

## Sampling Rate

- **After every task commit:** Run static markup and compilation checks.
- **After every plan wave:** Full visual regression check on breakpoints.
- **Before final sign-off:** Ensure `dotnet build` executes cleanly with zero compilation errors.

---

## Per-Task Verification Map

| Task ID | Component | Requirement | Secure / Expected Behavior | Test Type | Verification Method | Status |
|---------|-----------|-------------|----------------------------|-----------|---------------------|--------|
| UI-01-01 | site.css | Centralize `.chart-grid`, `.financial-grid`, and `.form-grid-3` | Fluid layout grid with no horizontal overflows on viewports under 768px | manual | Static media query inspect | ✅ green |
| UI-01-02 | site.css | Glassmorphism card variables and active shadows | Backdrops must display smooth transparency gradients and modern active flows | manual | Visual check | ✅ green |
| UI-01-03 | site.css | Custom toggles, outline focus animations, custom checkable tiles | Dynamic highlight animations during focus events with clear touch targets | manual | Tab-index focus checks | ✅ green |
| UI-01-04 | site.css | Standardize typography and `.data-table` hover states | High-contrast font pairings and smooth background alternating rows | manual | Visual check | ✅ green |
| UI-02-01 | _Layout.cshtml | Modernize vertical sidebar drawer and collapse | Smooth transitions on collapse/expand with no content shifting | manual | Layout resizing check | ✅ green |
| UI-02-02 | _Layout.cshtml | Header search, notify count, profile alignment | Centered layout alignment for all elements in top-header bar | manual | Breakpoint audit | ✅ green |
| UI-02-03 | _Layout.cshtml | Mobile navigation overlay and z-index bounds | Sidebar overlay sits perfectly atop standard viewports on mobile | manual | Layout resizing check | ✅ green |
| UI-03-01 | Owner.cshtml | Refactor inline styles into `.chart-grid` and `.herd-legend` | KPI blocks and SVG/Canvas charts scale seamlessly with container bounds | manual | Breakpoint audit | ✅ green |
| UI-03-02 | AssignTrip.cshtml| Form grid audit for responsive fields | Grid elements align into neat columns and fall back to single column on mobile | manual | Visual check | ✅ green |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

Existing infrastructure (Razor syntax parser and Dotnet compiler) fully covers all phase requirements. No additional package installs needed.

---

## Manual-Only Verifications

| Behavior / Requirement | Source File | Why Manual | Test Instructions |
|-----------------------|-------------|------------|-------------------|
| Responsive Grid Breakpoints | [site.css](file:///f:/VisualStudio/CattleFarm/CattleFarm/wwwroot/css/site.css) | Layout rendering relies on client-side viewport width | Resize viewport to 1024px (tablet), 768px (small tablet), and 480px (mobile) to verify that grids pack and stack cleanly with zero horizontal scrollbars. |
| Top Header Elements Alignment | [_Layout.cshtml](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Shared/_Layout.cshtml) | Visual checks for pixel-perfect vertical alignment | Access dashboard as Owner, inspect notifications count badge and search inputs to ensure they align vertically with the user profile image. |
| KPI Card Scaling | [Owner.cshtml](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Dashboard/Owner.cshtml) | Chart.js and layout bounds require browser-resize checks | Resize browser dynamically. Chart canvases must shrink/grow within their `.chart-grid` boxes, and the `.herd-legend` dots must remain neatly spaced. |
| Form Input Standardisation | [AssignTrip.cshtml](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Transport/AssignTrip.cshtml) | Requires interaction tests for checkboxes and radios | Select custom vehicles and drivers. Ensure the suggested chips apply correctly and the layout doesn't overlap on 320px screens. |

---

## Validation Sign-Off

- [x] All tasks have manual/automated verification methods mapped
- [x] No inline styles remain on dashboard charts or layouts
- [x] Tested with multiple viewport dimensions to ensure fluid responsiveness
- [x] Clean compilation verified (`dotnet build` succeeds with 0 errors)
- [x] `nyquist_compliant: true` set in frontmatter

**Approval:** Approved 2026-06-01
