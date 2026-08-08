
"use strict";
const $  = s => document.querySelector(s);
const $$ = s => [...document.querySelectorAll(s)];
const AUTH_VIEWS = ["home","login","register"];

/* مسیریابی نمایشی بین صفحه‌ها (هر view یک section با id برابر v-نام است) */
function go(view){
  const el = document.getElementById("v-" + view);
  if (!el) return;
  const isAuth = AUTH_VIEWS.includes(view);
  $("#authLayer").style.display = isAuth ? "" : "none";
  $("#appLayer").style.display  = isAuth ? "none" : "flex";
  $$(".view").forEach(v => v.hidden = v.id !== "v-" + view);
  el.classList.remove("anim"); void el.offsetWidth; el.classList.add("anim");
  $$(".sb-item").forEach(a => a.classList.toggle("active", a.dataset.go === view));
  if (!isAuth){
    $("#tbTitle").textContent = el.dataset.title || "";
    $("#tbSub").textContent   = el.dataset.sub   || "";
  }
  /* نوار پیشروی بالای صفحه */
  const pb = $("#pbar");
  pb.style.transition = "none"; pb.style.width = "0";
  requestAnimationFrame(() => { pb.style.transition = "width .5s ease"; pb.style.width = "100%"; });
  setTimeout(() => { pb.style.transition = "none"; pb.style.width = "0"; }, 650);
  location.hash = view;
  window.scrollTo({ top: 0 });
  /* فوکوس خودکار اولین فیلد فرم‌ها — سرعت حسابدار */
  const af = el.querySelector("[data-autofocus]");
  if (af) setTimeout(() => af.focus(), 380);
  closeSide();
}
document.addEventListener("click", e => {
  const t = e.target.closest("[data-go]");
  if (t){ e.preventDefault(); go(t.dataset.go); }
});
window.addEventListener("hashchange", () => { const v = location.hash.slice(1); if (v) go(v); });
document.addEventListener("DOMContentLoaded", () => {
  go(location.hash.slice(1) || "home");
  /* راویل عناصر با اسکرول */
  const io = new IntersectionObserver(es => es.forEach(x => {
    if (x.isIntersecting){ x.target.classList.add("in"); io.unobserve(x.target); }
  }), { threshold:.08 });
  $$(".rv").forEach(el => io.observe(el));
});

/* سایدبار موبایل */
function closeSide(){ $("#sideEl").classList.remove("open"); $("#sbBack").classList.remove("show"); }
document.addEventListener("DOMContentLoaded", () => {
  $("#btnBurger").addEventListener("click", () => { $("#sideEl").classList.add("open"); $("#sbBack").classList.add("show"); });
  $("#sbBack").addEventListener("click", closeSide);
});

/* مودال‌ها */
function openM(id){ document.getElementById(id).classList.add("open"); document.body.style.overflow = "hidden"; }
function closeM(id){ document.getElementById(id).classList.remove("open"); document.body.style.overflow = ""; }
document.addEventListener("click", e => {
  const c = e.target.closest("[data-close]");
  if (c) closeM(c.closest(".mback").id);
  if (e.target.classList.contains("mback")) closeM(e.target.id);
  const cf = e.target.closest("[data-confirm]");
  if (cf){ $("#confirmText").textContent = cf.dataset.confirm + " — این عملیات در قالب نمایشی است."; openM("mConfirm"); }
  const sl = e.target.closest("[data-slip]");
  if (sl) openM("mSlip");
  const pr = e.target.closest("[data-print]");
  if (pr) window.print();
});
document.addEventListener("keydown", e => {
  if (e.key === "Escape") $$(".mback.open").forEach(m => closeM(m.id));
});

/* توست نمایشی */
function toast(msg){
  const z = $("#toastZone"), t = document.createElement("div");
  t.className = "app-toast";
  t.innerHTML = '<svg class="ic"><use href="#i-info"/></svg><div>' + msg + '</div>';
  z.appendChild(t);
  setTimeout(() => { t.classList.add("out"); setTimeout(() => t.remove(), 320); }, 3300);
}
document.addEventListener("click", e => {
  const d = e.target.closest("[data-demo]");
  if (d) toast(d.dataset.demo || "این بخش در قالب نمایشی است و با پیاده‌سازی اصلی تکمیل می‌شود.");
});

