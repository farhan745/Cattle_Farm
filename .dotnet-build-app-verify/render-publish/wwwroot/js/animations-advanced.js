// ═════════════════════════════════════════════════════════════════════════════
// ADVANCED ANIMATIONS & PREMIUM EFFECTS
// Professional light-theme JavaScript for CattleFarm
// ═════════════════════════════════════════════════════════════════════════════

// ─────────────────────────────────────────────────────────────────────────────
// CUSTOM CURSOR DOT
// ─────────────────────────────────────────────────────────────────────────────

function initCursorDot() {
    const cursorDot = document.createElement('div');
    cursorDot.className = 'cursor-dot';
    document.body.appendChild(cursorDot);

    let mouseX = 0;
    let mouseY = 0;
    let dotX = 0;
    let dotY = 0;

    document.addEventListener('mousemove', (e) => {
        mouseX = e.clientX;
        mouseY = e.clientY;
    });

    function animateDot() {
        dotX += (mouseX - dotX) * 0.2;
        dotY += (mouseY - dotY) * 0.2;

        cursorDot.style.left = dotX + 'px';
        cursorDot.style.top = dotY + 'px';

        requestAnimationFrame(animateDot);
    }

    animateDot();

    // Hide cursor dot when leaving window
    document.addEventListener('mouseleave', () => {
        cursorDot.classList.add('hidden');
    });

    document.addEventListener('mouseenter', () => {
        cursorDot.classList.remove('hidden');
    });
}

// ─────────────────────────────────────────────────────────────────────────────
// PARALLAX TILT EFFECT
// ─────────────────────────────────────────────────────────────────────────────

function initParallaxTilt() {
    const tiltCards = document.querySelectorAll('.tilt-card');

    tiltCards.forEach((card) => {
        card.addEventListener('mousemove', (e) => {
            const rect = card.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;

            const centerX = rect.width / 2;
            const centerY = rect.height / 2;

            const rotateX = (y - centerY) / 10;
            const rotateY = (centerX - x) / 10;

            card.style.transform = `perspective(1000px) rotateX(${rotateX}deg) rotateY(${rotateY}deg) scale(1.02)`;
        });

        card.addEventListener('mouseleave', () => {
            card.style.transform = 'perspective(1000px) rotateX(0deg) rotateY(0deg) scale(1)';
        });
    });
}

// ─────────────────────────────────────────────────────────────────────────────
// SPOTLIGHT EFFECT FOLLOWING CURSOR
// ─────────────────────────────────────────────────────────────────────────────

function initSpotlight() {
    const spotlightContainers = document.querySelectorAll('.spotlight-container');

    spotlightContainers.forEach((container) => {
        const spotlight = container.querySelector('.spotlight');
        if (!spotlight) return;

        container.addEventListener('mousemove', (e) => {
            const rect = container.getBoundingClientRect();
            const x = e.clientX - rect.left - 200; // Center the spotlight
            const y = e.clientY - rect.top - 200;

            spotlight.style.left = x + 'px';
            spotlight.style.top = y + 'px';
        });

        container.addEventListener('mouseleave', () => {
            spotlight.style.opacity = '0';
        });
    });
}

// ─────────────────────────────────────────────────────────────────────────────
// TYPEWRITER EFFECT
// ─────────────────────────────────────────────────────────────────────────────

function initTypewriter() {
    const typewriterElements = document.querySelectorAll('.typewriter');

    typewriterElements.forEach((element) => {
        const text = element.textContent;
        element.textContent = '';
        element.style.minHeight = '1em';

        let index = 0;
        const speed = 50; // ms per character

        function type() {
            if (index < text.length) {
                element.textContent += text.charAt(index);
                index++;
                setTimeout(type, speed);
            }
        }

        // Start typing when element is in view
        const observer = new IntersectionObserver((entries) => {
            entries.forEach((entry) => {
                if (entry.isIntersecting && index === 0) {
                    type();
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.5 });

        observer.observe(element);
    });
}

// ─────────────────────────────────────────────────────────────────────────────
// WORD-BY-WORD FADE-IN
// ─────────────────────────────────────────────────────────────────────────────

function initWordFadeIn() {
    const wordFadeElements = document.querySelectorAll('.word-fade-in');

    wordFadeElements.forEach((element) => {
        const text = element.textContent;
        const words = text.split(' ');
        element.innerHTML = words
            .map((word) => `<span>${word}&nbsp;</span>`)
            .join('');

        const observer = new IntersectionObserver((entries) => {
            entries.forEach((entry) => {
                if (entry.isIntersecting) {
                    const spans = entry.target.querySelectorAll('span');
                    spans.forEach((span) => {
                        span.style.animation = 'wordFadeIn 0.6s ease-out forwards';
                    });
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.5 });

        observer.observe(element);
    });
}

// ─────────────────────────────────────────────────────────────────────────────
// PAGE TRANSITIONS
// ─────────────────────────────────────────────────────────────────────────────

function initPageTransitions() {
    // Add transition class on page load
    const pageContent = document.querySelector('.page-content') || document.body;
    if (pageContent && !pageContent.classList.contains('page-transition')) {
        pageContent.classList.add('page-transition');
    }

    // Handle link clicks for smooth transitions
    document.addEventListener('click', (e) => {
        const link = e.target.closest('a');
        if (!link || link.target === '_blank' || link.href.includes('#')) return;

        // Check if it's an internal link
        if (link.href.startsWith(window.location.origin)) {
            e.preventDefault();

            const pageContent = document.querySelector('.page-content') || document.body;
            pageContent.classList.add('page-transition-out');

            setTimeout(() => {
                window.location.href = link.href;
            }, 400);
        }
    });
}

// ─────────────────────────────────────────────────────────────────────────────
// FORM SHAKE ON VALIDATION ERROR
// ─────────────────────────────────────────────────────────────────────────────

function initFormShake() {
    const forms = document.querySelectorAll('form');

    forms.forEach((form) => {
        form.addEventListener('submit', (e) => {
            const invalidFields = form.querySelectorAll(':invalid');

            if (invalidFields.length > 0) {
                form.classList.add('form-shake');

                setTimeout(() => {
                    form.classList.remove('form-shake');
                }, 500);

                // Also shake individual invalid fields
                invalidFields.forEach((field) => {
                    field.classList.add('form-shake');
                    setTimeout(() => {
                        field.classList.remove('form-shake');
                    }, 500);
                });
            }
        });
    });
}

// ─────────────────────────────────────────────────────────────────────────────
// CHECKBOX BOUNCE ON CHECK
// ─────────────────────────────────────────────────────────────────────────────

function initCheckboxBounce() {
    const checkboxes = document.querySelectorAll('.checkbox-bounce input[type="checkbox"]');

    checkboxes.forEach((checkbox) => {
        checkbox.addEventListener('change', () => {
            const label = checkbox.nextElementSibling;
            if (label && label.tagName === 'LABEL') {
                label.style.animation = 'none';
                // Trigger reflow to restart animation
                void label.offsetWidth;
                label.style.animation = 'checkboxBounce 0.6s cubic-bezier(0.68, -0.55, 0.265, 1.55)';
            }
        });
    });
}

// ─────────────────────────────────────────────────────────────────────────────
// SUCCESS CHECKMARK DRAW ANIMATION
// ─────────────────────────────────────────────────────────────────────────────

function showSuccessCheckmark(container) {
    const svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
    svg.setAttribute('viewBox', '0 0 52 52');
    svg.setAttribute('width', '50');
    svg.setAttribute('height', '50');

    const circle = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
    circle.setAttribute('cx', '26');
    circle.setAttribute('cy', '26');
    circle.setAttribute('r', '25');
    circle.setAttribute('fill', 'none');
    circle.setAttribute('stroke', 'var(--primary)');
    circle.setAttribute('stroke-width', '2');
    circle.classList.add('checkmark-circle');

    const checkmark = document.createElementNS('http://www.w3.org/2000/svg', 'polyline');
    checkmark.setAttribute('points', '16,26 24,34 36,16');
    checkmark.setAttribute('fill', 'none');
    checkmark.setAttribute('stroke', 'var(--primary)');
    checkmark.setAttribute('stroke-width', '2');
    checkmark.setAttribute('stroke-linecap', 'round');
    checkmark.setAttribute('stroke-linejoin', 'round');
    checkmark.classList.add('checkmark-check');

    svg.appendChild(circle);
    svg.appendChild(checkmark);

    const wrapper = document.createElement('div');
    wrapper.className = 'checkmark-draw';
    wrapper.appendChild(svg);

    container.innerHTML = '';
    container.appendChild(wrapper);
}

// ─────────────────────────────────────────────────────────────────────────────
// FLOATING PARTICLES (Canvas-based)
// ─────────────────────────────────────────────────────────────────────────────

function initFloatingParticles() {
    const particleContainers = document.querySelectorAll('.particle-container');

    particleContainers.forEach((container) => {
        const canvas = document.createElement('canvas');
        canvas.width = container.offsetWidth;
        canvas.height = container.offsetHeight;
        canvas.style.position = 'absolute';
        canvas.style.top = '0';
        canvas.style.left = '0';
        canvas.style.pointerEvents = 'none';

        container.style.position = 'relative';
        container.appendChild(canvas);

        const ctx = canvas.getContext('2d');
        const particles = [];

        // Create particles
        for (let i = 0; i < 30; i++) {
            particles.push({
                x: Math.random() * canvas.width,
                y: Math.random() * canvas.height,
                vx: (Math.random() - 0.5) * 0.5,
                vy: (Math.random() - 0.5) * 0.5,
                radius: Math.random() * 2 + 1,
                opacity: Math.random() * 0.5 + 0.2,
            });
        }

        function animate() {
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            ctx.fillStyle = `rgba(45, 106, 79, 0.3)`;

            particles.forEach((particle) => {
                particle.x += particle.vx;
                particle.y += particle.vy;

                // Bounce off edges
                if (particle.x - particle.radius < 0 || particle.x + particle.radius > canvas.width) {
                    particle.vx *= -1;
                }
                if (particle.y - particle.radius < 0 || particle.y + particle.radius > canvas.height) {
                    particle.vy *= -1;
                }

                ctx.globalAlpha = particle.opacity;
                ctx.beginPath();
                ctx.arc(particle.x, particle.y, particle.radius, 0, Math.PI * 2);
                ctx.fill();
            });

            ctx.globalAlpha = 1;
            requestAnimationFrame(animate);
        }

        animate();

        // Handle window resize
        window.addEventListener('resize', () => {
            canvas.width = container.offsetWidth;
            canvas.height = container.offsetHeight;
        });
    });
}

// ─────────────────────────────────────────────────────────────────────────────
// SVG BLOB MORPHING
// ─────────────────────────────────────────────────────────────────────────────

function initBlobMorphing() {
    const blobs = document.querySelectorAll('.blob');

    blobs.forEach((blob) => {
        if (blob.tagName === 'svg') {
            // SVG blob morphing is handled by CSS animation
            // This function ensures the blob is properly initialized
            blob.style.animation = 'blobMorph 8s ease-in-out infinite';
        }
    });
}

// ─────────────────────────────────────────────────────────────────────────────
// SHIMMER HIGHLIGHT
// ─────────────────────────────────────────────────────────────────────────────

function initShimmerHighlight() {
    const shimmerElements = document.querySelectorAll('.shimmer-highlight');

    shimmerElements.forEach((element) => {
        element.style.animation = 'shimmer 2s infinite';
    });
}

// ─────────────────────────────────────────────────────────────────────────────
// GLOW PULSE EFFECT
// ─────────────────────────────────────────────────────────────────────────────

function initGlowPulse() {
    const glowElements = document.querySelectorAll('.glow-pulse');

    glowElements.forEach((element) => {
        element.style.animation = 'glowPulse 2s ease-in-out infinite';
    });
}

// ─────────────────────────────────────────────────────────────────────────────
// ENTRANCE ANIMATIONS ON SCROLL
// ─────────────────────────────────────────────────────────────────────────────

function initEntranceAnimations() {
    const animatedElements = document.querySelectorAll('.bounce-in, .flip-in, .rotate-in');

    const observer = new IntersectionObserver((entries) => {
        entries.forEach((entry) => {
            if (entry.isIntersecting) {
                // Element is already visible with animation class
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.1 });

    animatedElements.forEach((element) => {
        observer.observe(element);
    });
}

// ─────────────────────────────────────────────────────────────────────────────
// INITIALIZE ALL ANIMATIONS ON DOM READY
// ─────────────────────────────────────────────────────────────────────────────

document.addEventListener('DOMContentLoaded', () => {
    // Initialize all animation features
    // initCursorDot();
    initParallaxTilt();
    initSpotlight();
    initTypewriter();
    initWordFadeIn();
    initPageTransitions();
    initFormShake();
    initCheckboxBounce();
    initFloatingParticles();
    initBlobMorphing();
    initShimmerHighlight();
    initGlowPulse();
    initEntranceAnimations();
});

// ─────────────────────────────────────────────────────────────────────────────
// UTILITY FUNCTIONS
// ─────────────────────────────────────────────────────────────────────────────

/**
 * Add ripple effect to a button
 * @param {HTMLElement} button - The button element
 */
function addRippleEffect(button) {
    if (!button.classList.contains('btn-ripple')) {
        button.classList.add('btn-ripple');
    }
}

/**
 * Trigger form shake animation
 * @param {HTMLElement} form - The form element
 */
function triggerFormShake(form) {
    form.classList.add('form-shake');
    setTimeout(() => {
        form.classList.remove('form-shake');
    }, 500);
}

/**
 * Show success animation in a container
 * @param {HTMLElement} container - The container element
 * @param {string} message - Optional success message
 */
function showSuccessAnimation(container, message = 'Success!') {
    showSuccessCheckmark(container);

    if (message) {
        const messageEl = document.createElement('p');
        messageEl.textContent = message;
        messageEl.style.marginTop = '12px';
        messageEl.style.color = 'var(--primary)';
        messageEl.style.fontWeight = '600';
        container.appendChild(messageEl);
    }
}

/**
 * Create a spinner element
 * @returns {HTMLElement} - The spinner element
 */
function createSpinner() {
    const spinner = document.createElement('div');
    spinner.className = 'spinner';
    return spinner;
}

/**
 * Add pulse animation to an element
 * @param {HTMLElement} element - The element to pulse
 */
function addPulseAnimation(element) {
    element.classList.add('pulse');
}

/**
 * Remove pulse animation from an element
 * @param {HTMLElement} element - The element to stop pulsing
 */
function removePulseAnimation(element) {
    element.classList.remove('pulse');
}

// Export for use in other scripts
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        addRippleEffect,
        triggerFormShake,
        showSuccessAnimation,
        createSpinner,
        addPulseAnimation,
        removePulseAnimation,
    };
}
