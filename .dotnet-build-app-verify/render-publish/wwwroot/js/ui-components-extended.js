/* ═════════════════════════════════════════════════════════════════════════════
   EXTENDED UI COMPONENTS INTERACTIONS
   JavaScript for CattleFarm UI components
   ═════════════════════════════════════════════════════════════════════════════ */

// ── Sticky Navbar Scroll Detection ────────────────────────────────────────────
function initStickyNavbar() {
    const navbar = document.querySelector('.sticky-navbar');
    if (!navbar) return;

    let lastScrollTop = 0;
    
    window.addEventListener('scroll', () => {
        const scrollTop = window.scrollY;
        
        if (scrollTop > 50) {
            navbar.classList.add('scrolled');
        } else {
            navbar.classList.remove('scrolled');
        }
        
        lastScrollTop = scrollTop;
    }, { passive: true });
    
    // Active link detection
    const links = navbar.querySelectorAll('.sticky-navbar-links a');
    const sections = document.querySelectorAll('[data-section]');
    
    if (links.length && sections.length) {
        const updateActiveLink = () => {
            let current = '';
            
            sections.forEach(section => {
                const sectionTop = section.offsetTop;
                const sectionHeight = section.clientHeight;
                
                if (scrollTop >= sectionTop - 100) {
                    current = section.getAttribute('data-section');
                }
            });
            
            links.forEach(link => {
                link.classList.remove('active');
                if (link.getAttribute('href') === `#${current}`) {
                    link.classList.add('active');
                }
            });
        };
        
        window.addEventListener('scroll', updateActiveLink, { passive: true });
    }
}

// ── Hamburger Menu Toggle ─────────────────────────────────────────────────────
function initHamburgerMenu() {
    const toggle = document.querySelector('.navbar-toggle');
    const menu = document.querySelector('.sticky-navbar-links');
    
    if (!toggle || !menu) return;
    
    toggle.addEventListener('click', () => {
        menu.classList.toggle('active');
        toggle.classList.toggle('active');
    });
    
    // Close menu on link click
    menu.querySelectorAll('a').forEach(link => {
        link.addEventListener('click', () => {
            menu.classList.remove('active');
            toggle.classList.remove('active');
        });
    });
    
    // Close menu on outside click
    document.addEventListener('click', (e) => {
        if (!e.target.closest('.sticky-navbar')) {
            menu.classList.remove('active');
            toggle.classList.remove('active');
        }
    });
}

// ── Pagination ────────────────────────────────────────────────────────────────
function initPagination() {
    const paginationLinks = document.querySelectorAll('.page-link');
    
    paginationLinks.forEach(link => {
        link.addEventListener('click', (e) => {
            if (link.classList.contains('disabled')) {
                e.preventDefault();
                return;
            }
            
            // Remove active from all
            paginationLinks.forEach(l => l.classList.remove('active'));
            
            // Add active to clicked
            if (!link.classList.contains('prev') && !link.classList.contains('next')) {
                link.classList.add('active');
            }
        });
    });
}

// ── Data Table Sorting ────────────────────────────────────────────────────────
function initDataTableSorting() {
    const tables = document.querySelectorAll('.data-table');
    
    tables.forEach(table => {
        const headers = table.querySelectorAll('th.sortable');
        
        headers.forEach(header => {
            header.addEventListener('click', () => {
                const column = header.cellIndex;
                const tbody = table.querySelector('tbody');
                const rows = Array.from(tbody.querySelectorAll('tr'));
                const isAsc = header.classList.contains('sorted-asc');
                
                // Remove sorting classes from all headers
                headers.forEach(h => {
                    h.classList.remove('sorted-asc', 'sorted-desc');
                });
                
                // Add sorting class to current header
                header.classList.add(isAsc ? 'sorted-desc' : 'sorted-asc');
                
                // Sort rows
                rows.sort((a, b) => {
                    const aValue = a.cells[column].textContent.trim();
                    const bValue = b.cells[column].textContent.trim();
                    
                    // Try numeric sort first
                    const aNum = parseFloat(aValue);
                    const bNum = parseFloat(bValue);
                    
                    if (!isNaN(aNum) && !isNaN(bNum)) {
                        return isAsc ? bNum - aNum : aNum - bNum;
                    }
                    
                    // Fall back to string sort
                    return isAsc ? bValue.localeCompare(aValue) : aValue.localeCompare(bValue);
                });
                
                // Re-append sorted rows
                rows.forEach(row => tbody.appendChild(row));
            });
        });
    });
}

// ── Timeline Item Animations ──────────────────────────────────────────────────
function initTimelineAnimations() {
    const timelineItems = document.querySelectorAll('.timeline-item');
    
    if (!timelineItems.length) return;
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach((entry, index) => {
            if (entry.isIntersecting) {
                setTimeout(() => {
                    entry.target.style.opacity = '1';
                    entry.target.style.transform = 'translateX(0)';
                }, index * 100);
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.1 });
    
    timelineItems.forEach(item => {
        item.style.opacity = '0';
        item.style.transform = 'translateX(-20px)';
        item.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
        observer.observe(item);
    });
}

// ── Chat Bubble Animations ────────────────────────────────────────────────────
function initChatBubbles() {
    const bubbles = document.querySelectorAll('.chat-bubble');
    
    bubbles.forEach((bubble, index) => {
        bubble.style.animationDelay = `${index * 0.1}s`;
    });
}

// ── Device Mockup Responsive ──────────────────────────────────────────────────
function initDeviceMockups() {
    const mockups = document.querySelectorAll('.device-mockup');
    
    mockups.forEach(mockup => {
        const screen = mockup.querySelector('.device-screen');
        if (!screen) return;
        
        // Add hover effect
        mockup.addEventListener('mouseenter', () => {
            mockup.style.transform = 'scale(1.02)';
            mockup.style.boxShadow = '0 30px 80px rgba(0, 0, 0, 0.4)';
        });
        
        mockup.addEventListener('mouseleave', () => {
            mockup.style.transform = 'scale(1)';
            mockup.style.boxShadow = '0 20px 60px rgba(0, 0, 0, 0.3)';
        });
    });
}

// ── Spotlight Hover Effect ────────────────────────────────────────────────────
function initSpotlightHover() {
    const spotlightElements = document.querySelectorAll('.spotlight-hover');
    
    spotlightElements.forEach(element => {
        element.addEventListener('mousemove', (e) => {
            const rect = element.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;
            
            element.style.setProperty('--x', `${x}px`);
            element.style.setProperty('--y', `${y}px`);
        });
        
        element.addEventListener('mouseleave', () => {
            element.style.setProperty('--x', '50%');
            element.style.setProperty('--y', '50%');
        });
    });
}

// ── Blur-up Image Loading ─────────────────────────────────────────────────────
function initBlurUpImages() {
    const blurImages = document.querySelectorAll('.blur-up img.blur');
    
    blurImages.forEach(img => {
        if (img.complete) {
            img.classList.remove('blur');
            img.classList.add('loaded');
        } else {
            img.addEventListener('load', () => {
                img.classList.remove('blur');
                img.classList.add('loaded');
            });
        }
    });
}

// ── Gradient Text Animation ───────────────────────────────────────────────────
function initGradientText() {
    const gradientTexts = document.querySelectorAll('.gradient-text.rainbow');
    
    gradientTexts.forEach(text => {
        const style = document.createElement('style');
        style.textContent = `
            .gradient-text.rainbow {
                background-size: 200% 100%;
                animation: gradientFlow 6s ease infinite;
            }
        `;
        document.head.appendChild(style);
    });
}

// ── Floating Card Depth Effect ────────────────────────────────────────────────
function initFloatingCards() {
    const floatingCards = document.querySelectorAll('.floating-card');
    
    floatingCards.forEach(card => {
        card.addEventListener('mousemove', (e) => {
            const rect = card.getBoundingClientRect();
            const x = (e.clientX - rect.left) / rect.width;
            const y = (e.clientY - rect.top) / rect.height;
            
            const rotateX = (y - 0.5) * 10;
            const rotateY = (x - 0.5) * -10;
            
            card.style.transform = `perspective(1000px) rotateX(${rotateX}deg) rotateY(${rotateY}deg)`;
        });
        
        card.addEventListener('mouseleave', () => {
            card.style.transform = 'perspective(1000px) rotateX(0) rotateY(0)';
        });
    });
}

// ── Bento Grid Lazy Loading ───────────────────────────────────────────────────
function initBentoGrid() {
    const bentoItems = document.querySelectorAll('.bento-item');
    
    if (!bentoItems.length) return;
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach((entry, index) => {
            if (entry.isIntersecting) {
                setTimeout(() => {
                    entry.target.style.opacity = '1';
                    entry.target.style.transform = 'scale(1)';
                }, index * 50);
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.1 });
    
    bentoItems.forEach(item => {
        item.style.opacity = '0';
        item.style.transform = 'scale(0.95)';
        item.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
        observer.observe(item);
    });
}

// ── Glass Panel Hover Effect ──────────────────────────────────────────────────
function initGlassPanels() {
    const glassPanels = document.querySelectorAll('.glass-panel');
    
    glassPanels.forEach(panel => {
        panel.addEventListener('mousemove', (e) => {
            const rect = panel.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;
            
            panel.style.setProperty('--x', `${x}px`);
            panel.style.setProperty('--y', `${y}px`);
        });
    });
}

// ── Progress Bar Animation ────────────────────────────────────────────────────
function animateProgressBar(element, targetPercent, duration = 1500) {
    const fill = element.querySelector('.progress-fill');
    if (!fill) return;
    
    const startPercent = parseFloat(fill.style.width) || 0;
    const startTime = performance.now();
    
    const animate = (currentTime) => {
        const elapsed = currentTime - startTime;
        const progress = Math.min(elapsed / duration, 1);
        
        const easeOutQuad = 1 - Math.pow(1 - progress, 2);
        const currentPercent = startPercent + (targetPercent - startPercent) * easeOutQuad;
        
        fill.style.width = `${currentPercent}%`;
        
        if (progress < 1) {
            requestAnimationFrame(animate);
        }
    };
    
    requestAnimationFrame(animate);
}

// ── Initialize All Components ─────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    initStickyNavbar();
    initHamburgerMenu();
    initPagination();
    initDataTableSorting();
    initTimelineAnimations();
    initChatBubbles();
    initDeviceMockups();
    initSpotlightHover();
    initBlurUpImages();
    initGradientText();
    initFloatingCards();
    initBentoGrid();
    initGlassPanels();
    
    // Animate progress bars on scroll
    const progressBars = document.querySelectorAll('.progress-container');
    if (progressBars.length) {
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const fill = entry.target.querySelector('.progress-fill');
                    const targetPercent = parseFloat(fill.getAttribute('data-percent') || 0);
                    animateProgressBar(entry.target, targetPercent);
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.5 });
        
        progressBars.forEach(bar => observer.observe(bar));
    }
});

// ── Export functions for external use ─────────────────────────────────────────
window.animateProgressBar = animateProgressBar;
