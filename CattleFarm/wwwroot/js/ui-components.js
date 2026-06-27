// ═════════════════════════════════════════════════════════════════════════════
// UI COMPONENTS
// Professional light-theme component interactions for CattleFarm
// ═════════════════════════════════════════════════════════════════════════════

// ─────────────────────────────────────────────────────────────────────────────
// MODAL DIALOG
// ─────────────────────────────────────────────────────────────────────────────

class Modal {
    constructor(selector) {
        this.overlay = document.querySelector(selector);
        this.dialog = this.overlay?.querySelector('.modal-dialog');
        this.closeBtn = this.overlay?.querySelector('.modal-close');

        if (this.overlay && this.closeBtn) {
            this.closeBtn.addEventListener('click', () => this.close());
            this.overlay.addEventListener('click', (e) => {
                if (e.target === this.overlay) this.close();
            });
        }
    }

    open() {
        if (this.overlay) {
            this.overlay.classList.add('active');
            document.body.style.overflow = 'hidden';
        }
    }

    close() {
        if (this.overlay) {
            this.overlay.classList.remove('active');
            document.body.style.overflow = '';
        }
    }

    toggle() {
        if (this.overlay?.classList.contains('active')) {
            this.close();
        } else {
            this.open();
        }
    }
}

// Initialize all modals
function initModals() {
    const modals = document.querySelectorAll('.modal-overlay');
    modals.forEach((modal) => {
        const modalInstance = new Modal(`.modal-overlay[data-modal="${modal.dataset.modal}"]`);
    });

    // Handle modal trigger buttons
    document.addEventListener('click', (e) => {
        const trigger = e.target.closest('[data-modal-trigger]');
        if (trigger) {
            const modalId = trigger.dataset.modalTrigger;
            const modal = document.querySelector(`.modal-overlay[data-modal="${modalId}"]`);
            if (modal) {
                const modalInstance = new Modal(`.modal-overlay[data-modal="${modalId}"]`);
                modalInstance.open();
            }
        }
    });
}

// ─────────────────────────────────────────────────────────────────────────────
// ACCORDION
// ─────────────────────────────────────────────────────────────────────────────

class Accordion {
    constructor(selector) {
        this.container = document.querySelector(selector);
        this.headers = this.container?.querySelectorAll('.accordion-header');

        if (this.headers) {
            this.headers.forEach((header) => {
                header.addEventListener('click', () => this.toggle(header));
            });
        }
    }

    toggle(header) {
        const isActive = header.classList.contains('active');
        const body = header.nextElementSibling;

        if (isActive) {
            header.classList.remove('active');
            body?.classList.remove('active');
        } else {
            // Close other items
            this.headers.forEach((h) => {
                h.classList.remove('active');
                h.nextElementSibling?.classList.remove('active');
            });

            // Open clicked item
            header.classList.add('active');
            body?.classList.add('active');
        }
    }
}

// Initialize all accordions
function initAccordions() {
    const accordions = document.querySelectorAll('.accordion');
    accordions.forEach((accordion) => {
        new Accordion(`.accordion[data-accordion="${accordion.dataset.accordion}"]`);
    });
}

// ─────────────────────────────────────────────────────────────────────────────
// TABS
// ─────────────────────────────────────────────────────────────────────────────

class Tabs {
    constructor(selector) {
        this.container = document.querySelector(selector);
        this.buttons = this.container?.querySelectorAll('.tab-button');
        this.panes = this.container?.querySelectorAll('.tab-pane');

        if (this.buttons) {
            this.buttons.forEach((button, index) => {
                button.addEventListener('click', () => this.activate(index));
            });

            // Activate first tab by default
            if (this.buttons.length > 0) {
                this.activate(0);
            }
        }
    }

    activate(index) {
        // Deactivate all
        this.buttons.forEach((btn) => btn.classList.remove('active'));
        this.panes.forEach((pane) => pane.classList.remove('active'));

        // Activate selected
        if (this.buttons[index]) {
            this.buttons[index].classList.add('active');
        }
        if (this.panes[index]) {
            this.panes[index].classList.add('active');
        }
    }
}

// Initialize all tabs
function initTabs() {
    const tabsContainers = document.querySelectorAll('.tabs-container');
    tabsContainers.forEach((container) => {
        new Tabs(`.tabs-container[data-tabs="${container.dataset.tabs}"]`);
    });
}

// ─────────────────────────────────────────────────────────────────────────────
// TOGGLE SWITCH
// ─────────────────────────────────────────────────────────────────────────────

function initToggleSwitches() {
    const switches = document.querySelectorAll('.toggle-switch input');

    switches.forEach((toggle) => {
        toggle.addEventListener('change', (e) => {
            const event = new CustomEvent('toggle-change', {
                detail: { checked: e.target.checked },
            });
            e.target.dispatchEvent(event);
        });
    });
}

// ─────────────────────────────────────────────────────────────────────────────
// RANGE SLIDER
// ─────────────────────────────────────────────────────────────────────────────

function initRangeSliders() {
    const sliders = document.querySelectorAll('.range-slider');

    sliders.forEach((slider) => {
        slider.addEventListener('input', (e) => {
            const value = e.target.value;
            const event = new CustomEvent('range-change', {
                detail: { value },
            });
            e.target.dispatchEvent(event);
        });
    });
}

// ─────────────────────────────────────────────────────────────────────────────
// CUSTOM CHECKBOX
// ─────────────────────────────────────────────────────────────────────────────

function initCustomCheckboxes() {
    const checkboxes = document.querySelectorAll('.checkbox-custom input');

    checkboxes.forEach((checkbox) => {
        checkbox.addEventListener('change', (e) => {
            const event = new CustomEvent('checkbox-change', {
                detail: { checked: e.target.checked },
            });
            e.target.dispatchEvent(event);
        });
    });
}

// ─────────────────────────────────────────────────────────────────────────────
// CUSTOM RADIO BUTTON
// ─────────────────────────────────────────────────────────────────────────────

function initCustomRadios() {
    const radios = document.querySelectorAll('.radio-custom input');

    radios.forEach((radio) => {
        radio.addEventListener('change', (e) => {
            const event = new CustomEvent('radio-change', {
                detail: { value: e.target.value },
            });
            e.target.dispatchEvent(event);
        });
    });
}

// ─────────────────────────────────────────────────────────────────────────────
// TOOLTIPS
// ─────────────────────────────────────────────────────────────────────────────

function initTooltips() {
    const tooltips = document.querySelectorAll('[data-tooltip]');

    tooltips.forEach((element) => {
        const tooltipText = element.dataset.tooltip;
        const tooltipContainer = document.createElement('div');
        tooltipContainer.className = 'tooltip-container';

        const tooltip = document.createElement('div');
        tooltip.className = 'tooltip-text';
        tooltip.textContent = tooltipText;

        tooltipContainer.appendChild(tooltip);
        element.parentNode.insertBefore(tooltipContainer, element);
        tooltipContainer.appendChild(element);
    });
}

// ─────────────────────────────────────────────────────────────────────────────
// CARD GRID LAZY LOADING
// ─────────────────────────────────────────────────────────────────────────────

function initCardGridLazyLoad() {
    const cardGrids = document.querySelectorAll('.card-grid');

    cardGrids.forEach((grid) => {
        const items = grid.querySelectorAll('.card-grid-item');

        const observer = new IntersectionObserver((entries) => {
            entries.forEach((entry) => {
                if (entry.isIntersecting) {
                    entry.target.style.animation = 'fadeIn 0.6s ease-out forwards';
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.1 });

        items.forEach((item) => {
            item.style.opacity = '0';
            observer.observe(item);
        });
    });
}

// ─────────────────────────────────────────────────────────────────────────────
// MODAL HELPER FUNCTIONS
// ─────────────────────────────────────────────────────────────────────────────

/**
 * Open a modal by ID
 * @param {string} modalId - The modal ID
 */
function openModal(modalId) {
    const modal = document.querySelector(`.modal-overlay[data-modal="${modalId}"]`);
    if (modal) {
        const modalInstance = new Modal(`.modal-overlay[data-modal="${modalId}"]`);
        modalInstance.open();
    }
}

/**
 * Close a modal by ID
 * @param {string} modalId - The modal ID
 */
function closeModal(modalId) {
    const modal = document.querySelector(`.modal-overlay[data-modal="${modalId}"]`);
    if (modal) {
        const modalInstance = new Modal(`.modal-overlay[data-modal="${modalId}"]`);
        modalInstance.close();
    }
}

/**
 * Create and show a confirmation modal
 * @param {string} title - Modal title
 * @param {string} message - Modal message
 * @param {Function} onConfirm - Callback on confirm
 * @param {Function} onCancel - Callback on cancel
 */
function showConfirmModal(title, message, onConfirm, onCancel) {
    const overlay = document.createElement('div');
    overlay.className = 'modal-overlay active';
    overlay.innerHTML = `
        <div class="modal-dialog">
            <div class="modal-header">
                <h2 class="modal-title">${title}</h2>
                <button class="modal-close">✕</button>
            </div>
            <div class="modal-body">
                <p>${message}</p>
            </div>
            <div class="modal-footer">
                <button class="btn btn-outline cancel-btn">Cancel</button>
                <button class="btn btn-primary confirm-btn">Confirm</button>
            </div>
        </div>
    `;

    document.body.appendChild(overlay);
    document.body.style.overflow = 'hidden';

    const confirmBtn = overlay.querySelector('.confirm-btn');
    const cancelBtn = overlay.querySelector('.cancel-btn');
    const closeBtn = overlay.querySelector('.modal-close');

    const cleanup = () => {
        overlay.remove();
        document.body.style.overflow = '';
    };

    confirmBtn.addEventListener('click', () => {
        if (onConfirm) onConfirm();
        cleanup();
    });

    cancelBtn.addEventListener('click', () => {
        if (onCancel) onCancel();
        cleanup();
    });

    closeBtn.addEventListener('click', cleanup);

    overlay.addEventListener('click', (e) => {
        if (e.target === overlay) cleanup();
    });
}

// ─────────────────────────────────────────────────────────────────────────────
// INITIALIZE ALL UI COMPONENTS ON DOM READY
// ─────────────────────────────────────────────────────────────────────────────

document.addEventListener('DOMContentLoaded', () => {
    initModals();
    initAccordions();
    initTabs();
    initToggleSwitches();
    initRangeSliders();
    initCustomCheckboxes();
    initCustomRadios();
    initTooltips();
    initCardGridLazyLoad();
});

// ─────────────────────────────────────────────────────────────────────────────
// EXPORT FUNCTIONS FOR EXTERNAL USE
// ─────────────────────────────────────────────────────────────────────────────

if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        Modal,
        Accordion,
        Tabs,
        openModal,
        closeModal,
        showConfirmModal,
    };
}
