"use strict";
const $ = s => document.querySelector(s);
const $$ = s => [...document.querySelectorAll(s)];

const AUTH_VIEWS = ["home", "login", "register"];

/* مسیریابی نمایشی بین صفحهها */
function go(view) {
    const el = document.getElementById("v-" + view);
    if (!el) return;
    const isAuth = AUTH_VIEWS.includes(view);
    $("#authLayer").style.display = isAuth ? "" : "none";
    $("#appLayer").style.display = isAuth ? "none" : "flex";
    $$(".view").forEach(v => v.hidden = v.id !== "v-" + view);
    el.classList.remove("anim"); void el.offsetWidth; el.classList.add("anim");
    $$(".sb-item").forEach(a => a.classList.toggle("active", a.dataset.go === view));
    if (!isAuth) {
        $("#tbTitle").textContent = el.dataset.title || "";
        $("#tbSub").textContent = el.dataset.sub || "";
    }
    const pb = $("#pbar");
    pb.style.transition = "none"; pb.style.width = "0";
    requestAnimationFrame(() => { pb.style.transition = "width .5s ease"; pb.style.width = "100%"; });
    setTimeout(() => { pb.style.transition = "none"; pb.style.width = "0"; }, 650);
    location.hash = view;
    window.scrollTo({ top: 0 });
    const af = el.querySelector("[data-autofocus]");
    if (af) setTimeout(() => af.focus(), 380);
    closeSide();
}

document.addEventListener("click", e => {
    const t = e.target.closest("[data-go]");
    if (t) { e.preventDefault(); go(t.dataset.go); }
});
window.addEventListener("hashchange", () => { const v = location.hash.slice(1); if (v) go(v); });
document.addEventListener("DOMContentLoaded", () => {
    go(location.hash.slice(1) || "home");
    const io = new IntersectionObserver(es => es.forEach(x => {
        if (x.isIntersecting) { x.target.classList.add("in"); io.unobserve(x.target); }
    }), { threshold: .08 });
    $$(".rv").forEach(el => io.observe(el));
});

/* سایدبار موبایل + کلیک خارج */
function closeSide() {
    $("#sideEl").classList.remove("open");
    $("#sbBack").classList.remove("show");
}

document.addEventListener("DOMContentLoaded", () => {
    $("#btnBurger").addEventListener("click", () => {
        $("#sideEl").classList.toggle("open");
        $("#sbBack").classList.toggle("show");
    });
    $("#sbBack").addEventListener("click", closeSide);
    document.addEventListener("click", e => {
        if (!$("#sideEl").contains(e.target) && !$("#btnBurger").contains(e.target)) {
            closeSide();
        }
    });
});

/* مودالها، توست، فرم‌ها و بقیه اسکریپتهای اصلی بدون تغییر */