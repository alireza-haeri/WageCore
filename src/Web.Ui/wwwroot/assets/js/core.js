"use strict";

const $ = s => document.querySelector(s);
const $$ = s => [...document.querySelectorAll(s)];

const AUTH_VIEWS = ["home", "login", "register"];

function go(view) {
    const el = document.getElementById("v-" + view);
    if (!el) return;

    const authLayer = $("#authLayer");
    const appLayer = $("#appLayer");
    const tbTitle = $("#tbTitle");
    const tbSub = $("#tbSub");
    const pbar = $("#pbar");

    const isAuth = AUTH_VIEWS.includes(view);

    if (authLayer) authLayer.style.display = isAuth ? "" : "none";
    if (appLayer) appLayer.style.display = isAuth ? "none" : "flex";

    $$(".view").forEach(v => v.hidden = v.id !== "v-" + view);

    el.classList.remove("anim");
    void el.offsetWidth;
    el.classList.add("anim");

    $$(".sb-item").forEach(a => a.classList.toggle("active", a.dataset.go === view));

    if (!isAuth) {
        if (tbTitle) tbTitle.textContent = el.dataset.title || "";
        if (tbSub) tbSub.textContent = el.dataset.sub || "";
    }

    if (pbar) {
        pbar.style.transition = "none";
        pbar.style.width = "0";
        requestAnimationFrame(() => {
            pbar.style.transition = "width .5s ease";
            pbar.style.width = "100%";
        });
        setTimeout(() => {
            pbar.style.transition = "none";
            pbar.style.width = "0";
        }, 650);
    }

    location.hash = view;
    window.scrollTo({ top: 0 });

    const af = el.querySelector("[data-autofocus]");
    if (af) setTimeout(() => af.focus(), 380);

    closeSide();
}

/* ===== سایدبار ===== */
function closeSide() {
    const sideEl = $("#sideEl");
    const sbBack = $("#sbBack");
    if (sideEl) sideEl.classList.remove("open");
    if (sbBack) sbBack.classList.remove("show");
}

// برای استفاده در onclick توی HTML (دسترسی سراسری)
window.closeSide = closeSide;

/* ===== مودال‌ها ===== */
function openM(id) {
    const el = document.getElementById(id);
    if (el) {
        el.classList.add("open");
        document.body.style.overflow = "hidden";
    }
}

function closeM(id) {
    const el = document.getElementById(id);
    if (el) {
        el.classList.remove("open");
        document.body.style.overflow = "";
    }
}

/* ===== توست ===== */
function toast(msg) {
    const z = $("#toastZone");
    if (!z) return;

    const t = document.createElement("div");
    t.className = "app-toast";
    t.innerHTML = `<svg class="ic"><use href="#i-info"/></svg><div>${msg}</div>`;
    z.appendChild(t);

    setTimeout(() => {
        t.classList.add("out");
        setTimeout(() => t.remove(), 320);
    }, 3300);
}

/* ===== راه‌اندازی ===== */
function runApp() {
    // ===== کلیک روی دکمه‌های دارای data-go =====
    document.addEventListener("click", e => {
        const t = e.target.closest("[data-go]");
        if (t) {
            e.preventDefault();
            go(t.dataset.go);
        }
    });

    // ===== تغییر هش =====
    window.addEventListener("hashchange", () => {
        const v = location.hash.slice(1);
        if (v) go(v);
    });

    // ===== دکمه‌ی برگر (سایدبار موبایل) =====
    const btnBurger = $("#btnBurger");
    const sideEl = $("#sideEl");
    const sbBack = $("#sbBack");

    if (btnBurger && sideEl && sbBack) {
        // حذف کلاس open از sideEl در صورت وجود (برای جلوگیری از باز بودن پیش‌فرض)
        sideEl.classList.remove("open");
        sbBack.classList.remove("show");

        btnBurger.addEventListener("click", (e) => {
            e.stopPropagation();
            sideEl.classList.toggle("open");
            sbBack.classList.toggle("show");
        });

        sbBack.addEventListener("click", closeSide);

        // بستن سایدبار با کلیک بیرون
        document.addEventListener("click", e => {
            if (sideEl.classList.contains("open")) {
                if (!sideEl.contains(e.target) && !btnBurger.contains(e.target)) {
                    closeSide();
                }
            }
        });
    }

    // ===== مودال‌ها =====
    document.addEventListener("click", e => {
        const closeBtn = e.target.closest("[data-close]");
        if (closeBtn) {
            const modal = closeBtn.closest(".mback");
            if (modal) closeM(modal.id);
        }

        if (e.target.classList.contains("mback")) {
            closeM(e.target.id);
        }

        const confirmBtn = e.target.closest("[data-confirm]");
        if (confirmBtn) {
            const msg = confirmBtn.dataset.confirm || "تأیید";
            const confirmText = $("#confirmText");
            if (confirmText) {
                confirmText.textContent = msg + " — این عملیات در قالب نمایشی است.";
            }
            openM("mConfirm");
        }

        const slipBtn = e.target.closest("[data-slip]");
        if (slipBtn) openM("mSlip");

        const printBtn = e.target.closest("[data-print]");
        if (printBtn) window.print();
    });

    // ===== بستن مودال با دکمه‌ی Escape =====
    document.addEventListener("keydown", e => {
        if (e.key === "Escape") {
            $$(".mback.open").forEach(m => closeM(m.id));
        }
    });

    // ===== دمو کلیک (نمایش توست) =====
    document.addEventListener("click", e => {
        const demo = e.target.closest("[data-demo]");
        if (demo) {
            toast(demo.dataset.demo || "این بخش در قالب نمایشی است و با پیاده‌سازی اصلی تکمیل می‌شود.");
        }
    });

    // ===== اسکرول انیمیشن (IntersectionObserver) =====
    const io = new IntersectionObserver(es => {
        es.forEach(x => {
            if (x.isIntersecting) {
                x.target.classList.add("in");
                io.unobserve(x.target);
            }
        });
    }, { threshold: 0.08 });

    $$(".rv").forEach(el => io.observe(el));

    // ===== نمایش view اولیه =====
    const initialView = location.hash.slice(1) || "home";
    go(initialView);
}

/* ===== صبر برای آماده‌شدن کامل DOM ===== */
let attempts = 0;
const MAX_ATTEMPTS = 60;

function waitForDomAndRun() {
    attempts++;

    // وجود حداقل یکی از المان‌های کلیدی برای شروع
    const hasAuthLayer = document.getElementById("authLayer") !== null;
    const hasAppLayer = document.getElementById("appLayer") !== null;
    const hasSidebar = document.getElementById("sideEl") !== null;

    // اگر المان‌های لاگین وجود دارن یا سایدبار وجود داره یا تعداد تلاش به حد نهایی رسیده
    if ((hasAuthLayer && hasAppLayer) || hasSidebar || attempts >= MAX_ATTEMPTS) {
        runApp();
        return;
    }

    setTimeout(waitForDomAndRun, 100);
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', waitForDomAndRun);
} else {
    waitForDomAndRun();
}