// Smart Cattle Farm — Premium Ranch Heritage JavaScript

// ── Theme Toggle ─────────────────────────────────────────────────────────────
function toggleTheme() {
    const html  = document.documentElement;
    const icon  = document.getElementById('themeIcon');
    const isDark = html.getAttribute('data-theme') === 'dark';
    html.setAttribute('data-theme', isDark ? 'light' : 'dark');
    if (icon) icon.className = isDark ? 'bi bi-moon-stars' : 'bi bi-sun';
    localStorage.setItem('theme', isDark ? 'light' : 'dark');
}

// Apply saved theme on load
(function () {
    const saved = localStorage.getItem('theme') || 'light';
    document.documentElement.setAttribute('data-theme', saved);
    const icon = document.getElementById('themeIcon');
    if (icon) icon.className = saved === 'dark' ? 'bi bi-sun' : 'bi bi-moon-stars';
})();

// ── Sidebar ───────────────────────────────────────────────────────────────────
function toggleSidebar() {
    const sidebar  = document.getElementById('sidebar');
    const overlay  = document.getElementById('sidebarOverlay');
    sidebar?.classList.toggle('open');
    overlay?.classList.toggle('open');
}
function closeSidebar() {
    document.getElementById('sidebar')?.classList.remove('open');
    document.getElementById('sidebarOverlay')?.classList.remove('open');
}

// ── Toast Notifications ───────────────────────────────────────────────────────
function showToast(title, message, type = 'success', duration = 4000) {
    const container = document.getElementById('toastContainer');
    if (!container) return;
    const icons = { success: 'bi-check-circle-fill', error: 'bi-exclamation-triangle-fill', warning: 'bi-exclamation-circle-fill', info: 'bi-info-circle-fill' };
    const colors = { success: '#27ae60', error: '#c0392b', warning: '#e67e22', info: '#2980b9' };
    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.innerHTML = `<i class="bi ${icons[type] || icons.info}" style="color:${colors[type]};font-size:18px;flex-shrink:0"></i><div class="toast-body"><div class="toast-title">${title}</div><div style="font-size:12px;color:var(--text-secondary)">${message}</div></div><button onclick="this.parentElement.remove()" style="background:none;border:none;cursor:pointer;color:var(--text-muted);font-size:16px;flex-shrink:0;align-self:flex-start" aria-label="Close notification">✕</button>`;
    container.appendChild(toast);
    setTimeout(() => toast.remove(), duration);
}

// ── Notification Badge ────────────────────────────────────────────────────────
async function loadNotifCount() {
    try {
        const res = await fetch('/Notification/UnreadCount');
        if (!res.ok) return;
        const data = await res.json();
        const count = data.count;
        ['notif-count', 'header-notif-count'].forEach(id => {
            const el = document.getElementById(id);
            if (el) { el.textContent = count; el.style.display = count > 0 ? '' : 'none'; }
        });
    } catch {}
}

// ── Confirm Dialog ────────────────────────────────────────────────────────────
function confirmDelete(formOrUrl, message = 'Are you sure you want to delete this record? This action cannot be undone.') {
    if (confirm(message)) {
        if (typeof formOrUrl === 'string') {
            const f = document.createElement('form');
            f.method = 'POST'; f.action = formOrUrl;
            const t = document.createElement('input');
            t.type = 'hidden'; t.name = '__RequestVerificationToken';
            const tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
            t.value = tokenEl ? tokenEl.value : '';
            f.appendChild(t); document.body.appendChild(f); f.submit();
        } else { formOrUrl.submit(); }
    }
    return false;
}

// ── Image Preview ─────────────────────────────────────────────────────────────
function previewImage(input, previewId) {
    const preview = document.getElementById(previewId);
    if (!preview || !input.files?.[0]) return;
    const reader = new FileReader();
    reader.onload = e => { preview.src = e.target.result; preview.style.display = 'block'; };
    reader.readAsDataURL(input.files[0]);
}

// ── Debounce ──────────────────────────────────────────────────────────────────
function debounce(fn, delay = 300) {
    let timer;
    return (...args) => { clearTimeout(timer); timer = setTimeout(() => fn(...args), delay); };
}

// ── Format currency ───────────────────────────────────────────────────────────
function formatCurrency(amount, currency = '৳') {
    return `${currency} ${Number(amount).toLocaleString('en-BD', { minimumFractionDigits: 0 })}`;
}

// ── Scroll-triggered animations ───────────────────────────────────────────────
function initScrollAnimations() {
    const els = document.querySelectorAll('.animate-in');
    if (!els.length) return;
    const observer = new IntersectionObserver((entries) => {
        entries.forEach((entry, i) => {
            if (entry.isIntersecting) {
                setTimeout(() => entry.target.classList.add('visible'), i * 60);
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.1, rootMargin: '0px 0px -40px 0px' });
    els.forEach(el => observer.observe(el));
}

// ── Auto-dismiss Alerts ───────────────────────────────────────────────────────
document.querySelectorAll('.alert').forEach(a => setTimeout(() => a.remove(), 6000));

// ── DOMContentLoaded ──────────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    // Notification polling
    const isAuth = document.getElementById('notif-count') !== null;
    if (isAuth) { loadNotifCount(); setInterval(loadNotifCount, 60000); }

    // Scroll animations
    initScrollAnimations();

    // Page load stagger for main content
    const pageContent = document.querySelector('.page-content');
    if (pageContent) pageContent.classList.add('page-load-anim');
});
