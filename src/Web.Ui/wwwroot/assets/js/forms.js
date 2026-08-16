"use strict";
    /* ── Enter = رفتن به اینپوت بعدی (ویژهٔ سرعت حسابدار) ── */
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
            e.preventDefault(); /* جلوگیری از سابمیت نمایشی فرم */
        }
    });
    /* انتخاب کل متن فیلدهای مبلغ هنگام فوکوس */
    document.addEventListener("focusin", e => {
        if (e.target.matches(".amount .in")) e.target.select();
    });
    /* ── تب‌های گام‌دار (فرم کارمند و مانند آن) ── */
    $$(".step[data-tab]").forEach(btn => btn.addEventListener("click", () => activateTab(btn)));

    function activateTab(btn) {
        const group = btn.closest("[data-tabgroup]").dataset.tabgroup;
        $$("[data-tabgroup=" + group + "] .step").forEach(s => s.classList.toggle("active", s === btn));
        $$(".tabpane").forEach(p => {
            if (p.id.startsWith("tp-" + group + "-")) p.classList.toggle("show", p.id === "tp-" + btn.dataset.tab);
        });
    }

    /* دکمه‌های گام قبل/بعد */
    $$("[data-stepnav]").forEach(btn => btn.addEventListener("click", () => {
        const group = btn.closest("form").querySelector("[data-tabgroup]");
        if (!group) return;
        const steps = [...group.querySelectorAll(".step")];
        const i = steps.findIndex(s => s.classList.contains("active"));
        const next = steps[Math.max(0, Math.min(steps.length - 1, i + (+btn.dataset.stepnav)))];
        activateTab(next);
        window.scrollTo({top: 0, behavior: "smooth"});
    }));
    /* ── وابستگی «نحوهٔ اعمال پایه سنوات» ← «روش محاسبه» (مطابق سند دامنه) ── */
    const senmode = $("#p-senmode");
    if (senmode) senmode.addEventListener("change", () => {
        const auto = senmode.value === "auto";
        const method = $("#p-senmethod"), fld = $("#fld-senmethod");
        method.disabled = !auto;
        fld.classList.toggle("dis", !auto);
        if (!auto) method.value = "";
    });
    /* ── نمایش/مخفی‌سازی گذرواژه ── */
    $$(".pw-eye").forEach(btn => btn.addEventListener("click", () => {
        const inp = document.getElementById(btn.dataset.for);
        const show = inp.type === "password";
        inp.type = show ? "text" : "password";
        btn.querySelector(".e-on").style.display = show ? "none" : "";
        btn.querySelector(".e-off").style.display = show ? "" : "none";
    }));