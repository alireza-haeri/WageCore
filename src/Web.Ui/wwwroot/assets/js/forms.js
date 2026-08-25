"use strict";

// جلوگیری از اجرای مجدد اسکریپت وقتی Blazor تگ <script> رو دوباره insert می‌کنه
// (باعث خطای "Identifier '$$' has already been declared" می‌شد)
if (window.__formsJsLoaded) {
    // اسکریپت قبلاً یک‌بار اجرا شده؛ از اجرای مجدد و ری‌دیکلر متغیرها صرف‌نظر کن
} else {
    window.__formsJsLoaded = true;

// ===== تعریف $$ برای انتخابگرها (مثل نسخه اصلی) =====
    const $$ = s => [...document.querySelectorAll(s)];

// ===== قابلیت سرچ در selectها با دراپ‌داون واقعی (custom combobox) =====
// رفتار:
// - کلیک روی اینپوت، لیست گزینه‌ها رو زیرش باز می‌کنه (مثل یه select عادی).
// - تایپ کردن، لیست رو فیلتر می‌کنه.
// - با کلیک یا کیبورد (بالا/پایین/Enter) میشه گزینه رو انتخاب کرد.
// - به‌صورت پیش‌فرض هیچ گزینه‌ای انتخاب‌شده نیست (مگر select از قبل مقدار داشته باشه).
// - اگه کاربر از فیلد خارج بشه (blur) بدون اینکه یه گزینه‌ی معتبر رو انتخاب کرده باشه
//   (چیزی وارد نکرده، یا متنی که با هیچ گزینه‌ای مطابقت نداره)، اینپوت و select خالی می‌شن.
// - برای اینکه مقدار دلخواه (خارج از لیست) هم مجاز باشه، روی <select> بذار: data-allow-custom="true"
    function enableSearchableSelects(container = document) {
        container.querySelectorAll('select:not(.no-search):not([data-searchable])').forEach(select => {
            const id = select.id || 'select-' + Math.random().toString(36).substr(2, 9);
            if (!select.id) select.id = id;

            const allowCustom = select.dataset.allowCustom === 'true';

            // ----- ساخت wrapper برای موقعیت‌دهی دراپ‌داون -----
            const wrapper = document.createElement('div');
            wrapper.className = 'searchable-select-wrapper';
            wrapper.style.position = 'relative';
            wrapper.style.width = '100%';
            select.parentNode.insertBefore(wrapper, select);

            const input = document.createElement('input');
            input.type = 'text';
            input.className = select.className;
            input.id = 'search-' + id;
            input.placeholder = 'انتخاب کنید...';
            input.autocomplete = 'off';

            // کپی استایل ظاهریِ select به input (بدون تغییر ظاهر)
            const computedStyle = window.getComputedStyle(select);
            Object.assign(input.style, {
                cssText: computedStyle.cssText,
                width: '100%',
                boxSizing: 'border-box'
            });

            const dropdown = document.createElement('div');
            dropdown.className = 'searchable-select-dropdown';
            Object.assign(dropdown.style, {
                position: 'absolute',
                top: '100%',
                insetInlineStart: '0',
                insetInlineEnd: '0',
                zIndex: '1000',
                maxHeight: '240px',
                overflowY: 'auto',
                background: '#fff',
                border: '1px solid #ccc',
                borderRadius: '6px',
                boxShadow: '0 4px 12px rgba(0,0,0,0.12)',
                marginTop: '2px',
                display: 'none'
            });

            wrapper.appendChild(input);
            wrapper.appendChild(dropdown);

            select.style.display = 'none';
            wrapper.appendChild(select);
            select.dataset.searchable = 'true';

            // ----- توابع کمکی -----
            function getOptions() {
                return [...select.options].filter(o => o.value !== '' || o.textContent.trim() !== '');
            }

            function currentSelectedText() {
                const opt = select.options[select.selectedIndex];
                return (opt && opt.value !== '') ? opt.textContent : '';
            }

            // مقدار اولیه: فقط اگه select از قبل واقعاً مقداری داشته باشه
            input.value = currentSelectedText();
            let lastValidText = input.value;
            let highlightedIndex = -1;
            let filteredOptions = [];

            function closeDropdown() {
                dropdown.style.display = 'none';
                dropdown.innerHTML = '';
                highlightedIndex = -1;
            }

            function selectOption(opt) {
                select.value = opt.value;
                input.value = opt.textContent;
                lastValidText = opt.textContent;
                select.dispatchEvent(new Event('change', { bubbles: true }));
                closeDropdown();
            }

            function renderDropdown(filterText) {
                const q = (filterText || '').trim().toLowerCase();
                filteredOptions = getOptions().filter(o => o.textContent.toLowerCase().includes(q));

                dropdown.innerHTML = '';
                if (filteredOptions.length === 0) {
                    const empty = document.createElement('div');
                    empty.textContent = 'موردی یافت نشد';
                    Object.assign(empty.style, { padding: '8px 12px', color: '#999', fontSize: '0.9em' });
                    dropdown.appendChild(empty);
                } else {
                    filteredOptions.forEach((opt, idx) => {
                        const item = document.createElement('div');
                        item.textContent = opt.textContent;
                        item.dataset.idx = idx;
                        Object.assign(item.style, {
                            padding: '8px 12px',
                            cursor: 'pointer'
                        });
                        if (opt.value === select.value) {
                            item.style.background = '#eef4ff';
                        }
                        item.addEventListener('mouseenter', () => setHighlighted(idx));
                        // mousedown نه click، تا زودتر از blur اینپوت اجرا بشه
                        item.addEventListener('mousedown', (e) => {
                            e.preventDefault();
                            selectOption(opt);
                        });
                        dropdown.appendChild(item);
                    });
                }
                dropdown.style.display = 'block';
                highlightedIndex = -1;
            }

            function setHighlighted(idx) {
                const items = [...dropdown.children];
                items.forEach(el => el.style.background = '');
                highlightedIndex = idx;
                if (items[idx]) {
                    items[idx].style.background = '#e0ebff';
                    items[idx].scrollIntoView({ block: 'nearest' });
                }
            }

            function openDropdown() {
                renderDropdown(input.value === lastValidText ? '' : input.value);
            }

            // ----- رویدادها -----
            input.addEventListener('focus', openDropdown);
            input.addEventListener('click', openDropdown);

            input.addEventListener('input', () => {
                renderDropdown(input.value);
            });

            input.addEventListener('keydown', (e) => {
                if (dropdown.style.display === 'none' && (e.key === 'ArrowDown' || e.key === 'ArrowUp')) {
                    openDropdown();
                    return;
                }
                if (e.key === 'ArrowDown') {
                    e.preventDefault();
                    if (filteredOptions.length) setHighlighted(Math.min(highlightedIndex + 1, filteredOptions.length - 1));
                } else if (e.key === 'ArrowUp') {
                    e.preventDefault();
                    if (filteredOptions.length) setHighlighted(Math.max(highlightedIndex - 1, 0));
                } else if (e.key === 'Enter') {
                    if (dropdown.style.display !== 'none' && highlightedIndex > -1 && filteredOptions[highlightedIndex]) {
                        e.preventDefault();
                        selectOption(filteredOptions[highlightedIndex]);
                    }
                } else if (e.key === 'Escape') {
                    closeDropdown();
                }
            });

            // موقع خروج از فیلد: اگه گزینه‌ی معتبری انتخاب نشده، خالی کن
            input.addEventListener('blur', () => {
                // یه تاخیر کوچیک تا mousedown روی آیتم دراپ‌داون زودتر پردازش بشه
                setTimeout(() => {
                    const opt = getOptions().find(o => o.textContent === input.value);
                    if (opt) {
                        if (opt.value !== select.value) {
                            select.value = opt.value;
                            select.dispatchEvent(new Event('change', { bubbles: true }));
                        }
                        lastValidText = opt.textContent;
                    } else if (allowCustom && input.value.trim() !== '') {
                        lastValidText = input.value;
                    } else {
                        // چیزی وارد نکرده یا مقدار نامعتبره => خالی کن
                        input.value = '';
                        lastValidText = '';
                        if (select.value !== '') {
                            select.value = '';
                            select.dispatchEvent(new Event('change', { bubbles: true }));
                        }
                    }
                    closeDropdown();
                }, 150);
            });

            // بستن دراپ‌داون با کلیک بیرون
            document.addEventListener('click', (e) => {
                if (!wrapper.contains(e.target)) closeDropdown();
            });

            // اگر select به‌روز شد (مثلاً از طریق جاوااسکریپت دیگر)
            const observer = new MutationObserver(() => {
                const text = currentSelectedText();
                input.value = text;
                lastValidText = text;
            });
            observer.observe(select, { attributes: true, attributeFilter: ['value'] });
        });
    }

// ===== تابع فعال‌سازی تب‌ها =====
    function activateTab(btn) {
        const group = btn.closest("[data-tabgroup]")?.dataset.tabgroup;
        if (!group) return;
        $$(`[data-tabgroup="${group}"] .step`).forEach(s => s.classList.toggle("active", s === btn));
        $$(".tabpane").forEach(p => {
            if (p.id.startsWith("tp-" + group + "-")) {
                p.classList.toggle("show", p.id === "tp-" + btn.dataset.tab);
            }
        });
    }

// ===== تابع مقداردهی اولیه (برای عناصر موجود در DOM) =====
    function initElements(container = document) {
        // تب‌های گام‌دار
        container.querySelectorAll(".step[data-tab]").forEach(btn => {
            // جلوگیری از اتصال مجدد
            if (btn.dataset.listener) return;
            btn.addEventListener("click", () => activateTab(btn));
            btn.dataset.listener = "true";
        });

        // دکمه‌های گام قبل/بعد
        container.querySelectorAll("[data-stepnav]").forEach(btn => {
            if (btn.dataset.listener) return;
            btn.addEventListener("click", () => {
                const group = btn.closest("form")?.querySelector("[data-tabgroup]");
                if (!group) return;
                const steps = [...group.querySelectorAll(".step")];
                const i = steps.findIndex(s => s.classList.contains("active"));
                const next = steps[Math.max(0, Math.min(steps.length - 1, i + (+btn.dataset.stepnav)))];
                if (next) activateTab(next);
                window.scrollTo({ top: 0, behavior: "smooth" });
            });
            btn.dataset.listener = "true";
        });

        // نمایش/مخفی‌سازی گذرواژه
        container.querySelectorAll(".pw-eye").forEach(btn => {
            if (btn.dataset.listener) return;
            btn.addEventListener("click", () => {
                const inp = document.getElementById(btn.dataset.for);
                if (!inp) return;
                const show = inp.type === "password";
                inp.type = show ? "text" : "password";
                const on = btn.querySelector(".e-on");
                const off = btn.querySelector(".e-off");
                if (on) on.style.display = show ? "none" : "";
                if (off) off.style.display = show ? "" : "none";
            });
            btn.dataset.listener = "true";
        });

        // قابلیت سرچ در selectها
        enableSearchableSelects(container);

        // وابستگی «نحوهٔ اعمال پایه سنوات»
        const senmode = container.querySelector("#p-senmode");
        if (senmode && !senmode.dataset.listener) {
            senmode.addEventListener("change", () => {
                const auto = senmode.value === "auto";
                const method = document.getElementById("p-senmethod");
                const fld = document.getElementById("fld-senmethod");
                if (method) method.disabled = !auto;
                if (fld) fld.classList.toggle("dis", !auto);
                if (!auto && method) method.value = "";
            });
            senmode.dataset.listener = "true";
        }
    }

// ===== رویدادهای سراسری (با event delegation) =====
    document.addEventListener("keydown", e => {
        if (e.key !== "Enter") return;
        const t = e.target;
        if (!t.matches("input:not([type=checkbox]):not([type=radio]):not([type=hidden]), select")) return;
        const scope = t.closest("form") || document;
        const els = [...scope.querySelectorAll("input:not([type=hidden]):not([disabled]), select:not([disabled])")]
            .filter(el => el.offsetParent !== null);
        const i = els.indexOf(t);
        if (i > -1 && i < els.length - 1) {
            e.preventDefault();
            els[i + 1].focus();
            if (els[i + 1].select) els[i + 1].select();
        } else {
            e.preventDefault();
        }
    });

    document.addEventListener("focusin", e => {
        if (e.target.matches(".amount .in")) e.target.select();
    });

// ===== مشاهده‌گر تغییرات DOM (برای Blazor) =====
    const observer = new MutationObserver(mutations => {
        mutations.forEach(mutation => {
            mutation.addedNodes.forEach(node => {
                if (node.nodeType === 1) { // عنصر DOM
                    // اگر خود عنصر یا فرزندانش نیاز به مقداردهی دارند
                    if (node.matches && node.matches('select, .step, .pw-eye, [data-stepnav], #p-senmode')) {
                        initElements(node.parentNode);
                    } else {
                        // بررسی فرزندان
                        const container = node.querySelector ? node : null;
                        if (container) {
                            // بررسی وجود عناصر مورد نیاز درون container
                            const hasTargets = container.querySelector('select, .step, .pw-eye, [data-stepnav], #p-senmode');
                            if (hasTargets) initElements(container);
                        }
                    }
                }
            });
        });
    });

// ===== راه‌اندازی اولیه =====
    document.addEventListener("DOMContentLoaded", () => {
        initElements();
        observer.observe(document.body, { childList: true, subtree: true });
    });

// در صورتی که DOM از قبل بارگذاری شده باشد
    if (document.readyState !== 'loading') {
        initElements();
        observer.observe(document.body, { childList: true, subtree: true });
    }

// در دسترس قرار دادن initElements برای اجراهای بعدی همین اسکریپت (بدون ری‌دیکلر)
    window.__formsJsInit = initElements;

} // پایان گارد window.__formsJsLoaded

// این خط بیرون از گارد، هر بار که اسکریپت (دوباره) اجرا بشه کار می‌کنه:
// اگه اسکریپت قبلاً لود شده بود، فقط عناصر جدید رو مقداردهی کن، بدون ری‌دیکلر متغیرها
if (window.__formsJsLoaded && window.__formsJsInit) {
    window.__formsJsInit();
}