// Smart Cattle Farm — professional light UI JavaScript

// ── Theme Toggle ─────────────────────────────────────────────────────────────
function toggleTheme() {
    const html  = document.documentElement;
    const icon  = document.getElementById('themeIcon');
    html.setAttribute('data-theme', 'light');
    if (icon) icon.className = 'bi bi-brightness-high';
    localStorage.setItem('theme', 'light');
}

// Always use light mode for client-facing consistency.
(function () {
    localStorage.setItem('theme', 'light');
    document.documentElement.setAttribute('data-theme', 'light');
    const icon = document.getElementById('themeIcon');
    if (icon) icon.className = 'bi bi-brightness-high';
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
    toast.style.setProperty('--duration', `${duration}ms`);
    toast.innerHTML = `<i class="bi ${icons[type] || icons.info}" style="color:${colors[type]};font-size:18px;flex-shrink:0"></i><div class="toast-body"><div class="toast-title">${title}</div><div style="font-size:12px;color:var(--text-secondary)">${message}</div></div><button class="toast-dismiss" onclick="this.parentElement.remove()" style="background:none;border:none;cursor:pointer;color:var(--text-muted);font-size:16px;flex-shrink:0;align-self:flex-start" aria-label="Close notification">✕</button>`;
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
    const els = document.querySelectorAll('.animate-in, .slide-in-left, .slide-in-right, .zoom-in, .fade-in-up');
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

// ── Premium dashboard reveal support ─────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    const revealItems = document.querySelectorAll('.animate-in:not(.visible), .slide-in-left:not(.visible), .slide-in-right:not(.visible), .zoom-in:not(.visible), .fade-in-up:not(.visible)');
    if (!revealItems.length) return;

    if (!('IntersectionObserver' in window)) {
        revealItems.forEach(item => item.classList.add('visible'));
        return;
    }

    const revealObserver = new IntersectionObserver((entries) => {
        entries.forEach((entry) => {
            if (!entry.isIntersecting) return;
            entry.target.classList.add('visible');
            revealObserver.unobserve(entry.target);
        });
    }, { threshold: 0.12, rootMargin: '0px 0px -32px 0px' });

    revealItems.forEach(item => revealObserver.observe(item));
});

// ── Public landing page interactions ─────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    const landingNav = document.querySelector('[data-landing-nav]');
    if (landingNav) {
        const updateNav = () => landingNav.classList.toggle('is-scrolled', window.scrollY > 24);
        updateNav();
        window.addEventListener('scroll', updateNav, { passive: true });
    }

    const counters = document.querySelectorAll('.landing-count-up[data-target], .count-up[data-target]');
    if (!counters.length) return;

    const easeOutQuart = value => 1 - Math.pow(1 - value, 4);
    const formatCounter = (value, decimals) => Number(value).toLocaleString(undefined, {
        minimumFractionDigits: decimals,
        maximumFractionDigits: decimals
    });

    const runCounter = (element) => {
        if (element.dataset.counted === 'true') return;
        element.dataset.counted = 'true';

        const target = Number(element.dataset.target || 0);
        const suffix = element.dataset.suffix || '';
        const decimals = String(element.dataset.target || '').includes('.') ? 1 : 0;
        const start = performance.now();
        const duration = 1600;

        const tick = (now) => {
            const progress = Math.min((now - start) / duration, 1);
            const current = target * easeOutQuart(progress);
            element.textContent = `${formatCounter(current, progress === 1 ? decimals : 0)}${suffix}`;
            if (progress < 1) requestAnimationFrame(tick);
        };

        requestAnimationFrame(tick);
    };

    if (!('IntersectionObserver' in window)) {
        counters.forEach(runCounter);
        return;
    }

    const counterObserver = new IntersectionObserver((entries) => {
        entries.forEach((entry) => {
            if (!entry.isIntersecting) return;
            runCounter(entry.target);
            counterObserver.unobserve(entry.target);
        });
    }, { threshold: 0.35 });

    counters.forEach(counter => counterObserver.observe(counter));
});

// ── Carousel ──────────────────────────────────────────────────────────────────
function initCarousels() {
    const carousels = document.querySelectorAll('.carousel-wrapper');
    carousels.forEach(carousel => {
        const track = carousel.querySelector('.carousel-track');
        const slides = carousel.querySelectorAll('.carousel-slide');
        const dots = carousel.querySelectorAll('.carousel-dot, .carousel-dots button');
        if (!track || slides.length === 0) return;

        let currentIndex = 0;
        let autoplayTimer = null;

        const updateActiveDot = (index) => {
            dots.forEach((dot, idx) => {
                dot.classList.toggle('active', idx === index);
            });
            currentIndex = index;
        };

        const scrollToSlide = (index) => {
            const slideWidth = slides[0].offsetWidth;
            const gap = parseInt(window.getComputedStyle(track).gap) || 0;
            track.scrollTo({
                left: index * (slideWidth + gap),
                behavior: 'smooth'
            });
            updateActiveDot(index);
        };

        const startAutoplay = () => {
            stopAutoplay();
            autoplayTimer = setInterval(() => {
                let nextIndex = (currentIndex + 1) % slides.length;
                scrollToSlide(nextIndex);
            }, 5000);
        };

        const stopAutoplay = () => {
            if (autoplayTimer) {
                clearInterval(autoplayTimer);
                autoplayTimer = null;
            }
        };

        // Dot clicking
        dots.forEach((dot, index) => {
            dot.addEventListener('click', () => {
                scrollToSlide(index);
                startAutoplay(); // Restart timer on click
            });
        });

        // Scroll listener to update dots on manual swipe/scroll
        let scrollTimeout;
        track.addEventListener('scroll', () => {
            clearTimeout(scrollTimeout);
            scrollTimeout = setTimeout(() => {
                const slideWidth = slides[0].offsetWidth;
                const gap = parseInt(window.getComputedStyle(track).gap) || 0;
                const newIndex = Math.round(track.scrollLeft / (slideWidth + gap));
                if (newIndex !== currentIndex && newIndex >= 0 && newIndex < slides.length) {
                    updateActiveDot(newIndex);
                }
            }, 100);
        }, { passive: true });

        // Start autoplay initially
        startAutoplay();
    });
}

// ── Microinteractions: Ripple Effect ──────────────────────────────────────────
function initRippleEffect() {
    document.addEventListener('click', (e) => {
        const btn = e.target.closest('.btn-ripple');
        if (!btn) return;
        
        const rect = btn.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;
        
        const span = document.createElement('span');
        span.className = 'ripple-span';
        span.style.left = `${x}px`;
        span.style.top = `${y}px`;
        
        btn.appendChild(span);
        
        setTimeout(() => span.remove(), 650);
    });
}

// ── Initialize Task 3 Features ────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    initCarousels();
    initRippleEffect();
});
