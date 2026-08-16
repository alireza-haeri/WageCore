"use strict";

document.addEventListener("DOMContentLoaded", () => {

  // ---------- استایل‌های مورد نیاز ----------
  if (!document.querySelector("#searchable-select-styles")) {
    const style = document.createElement("style");
    style.id = "searchable-select-styles";
    style.textContent = `
      .ss-wrapper{position:relative;display:block;width:100%}
      .ss-input{width:100%;height:43px;border:1.5px solid var(--line,#dcd3b8);background:#fff;border-radius:10px;padding:0 14px;font:500 13.5px var(--ff,'Vazirmatn',Tahoma,sans-serif);color:var(--ink,#222c25);transition:border-color .15s,box-shadow .15s;cursor:pointer;direction:rtl;text-align:right}
      .ss-input:focus{outline:none;border-color:var(--grn,#175349);box-shadow:0 0 0 3.5px rgba(23,83,73,.13)}
      .ss-input::placeholder{color:#bdb394;font-weight:400}
      .ss-dropdown{position:absolute;top:calc(100% + 4px);right:0;left:0;max-height:220px;overflow-y:auto;background:var(--card,#fffdf7);border:1px solid var(--line,#dcd3b8);border-radius:10px;box-shadow:var(--sh,0 1px 2px rgba(30,40,34,.05));list-style:none;padding:4px 0;margin:0;z-index:1050;display:none;direction:rtl}
      .ss-dropdown.show{display:block}
      .ss-dropdown li{padding:8px 14px;font-size:13px;cursor:pointer;transition:background .12s;border-bottom:1px solid var(--line2,#e9e2cf);white-space:nowrap;overflow:hidden;text-overflow:ellipsis}
      .ss-dropdown li:last-child{border-bottom:none}
      .ss-dropdown li:hover,.ss-dropdown li.active{background:var(--grn-s,#e2eee7);color:var(--grn-d,#0e3830)}
      .ss-dropdown li.hidden{display:none}
      .ss-dropdown li .highlight{background:var(--gold-s,#f6ecd2);color:var(--gold,#a97609);font-weight:700;padding:0 2px;border-radius:2px}
      .ss-original{position:absolute!important;width:1px!important;height:1px!important;padding:0!important;margin:-1px!important;overflow:hidden!important;clip:rect(0,0,0,0)!important;border:0!important}
    `;
    document.head.appendChild(style);
  }

  // ---------- تبدیل select به کامپوننت جست‌وجو‌دار ----------
  function makeSearchable(select) {
    if (select.dataset.searchable === "false" || select.closest(".ss-wrapper")) return;

    const wrapper = document.createElement("div");
    wrapper.className = "ss-wrapper";

    select.classList.add("ss-original");
    select.parentNode.insertBefore(wrapper, select);
    wrapper.appendChild(select);

    const input = document.createElement("input");
    input.type = "text";
    input.className = "ss-input";
    input.placeholder = select.dataset.placeholder || "جست‌وجو یا انتخاب کنید...";
    input.autocomplete = "off";
    wrapper.appendChild(input);

    const ul = document.createElement("ul");
    ul.className = "ss-dropdown";
    wrapper.appendChild(ul);

    wrapper._select = select;
    wrapper._dropdown = ul;
    wrapper._input = input;

    let lastValidValue = select.value; // مقدار معتبر قبلی

    function buildOptions(filter = "") {
      ul.innerHTML = "";
      const options = [...select.options];
      let hasVisible = false;
      const filterLower = filter.trim().toLowerCase();
      options.forEach(opt => {
        if (opt.disabled) return;
        const li = document.createElement("li");
        li.textContent = opt.text;
        li.dataset.value = opt.value;
        if (filterLower) {
          const idx = opt.text.toLowerCase().indexOf(filterLower);
          if (idx > -1) {
            const before = opt.text.slice(0, idx);
            const match = opt.text.slice(idx, idx + filter.length);
            const after = opt.text.slice(idx + filter.length);
            li.innerHTML = before + `<span class="highlight">${match}</span>` + after;
          } else {
            li.classList.add("hidden");
          }
        }
        if (!li.classList.contains("hidden")) hasVisible = true;
        li.addEventListener("mousedown", (e) => e.preventDefault());
        li.addEventListener("click", () => {
          select.value = li.dataset.value;
          lastValidValue = select.value;
          select.dispatchEvent(new Event("change", { bubbles: true }));
          updateInputValue();
          closeDropdown();
          input.focus();
        });
        ul.appendChild(li);
      });
      if (!hasVisible) {
        const li = document.createElement("li");
        li.textContent = "❌ موردی یافت نشد";
        li.style.color = "var(--muted,#6e7a70)";
        li.style.cursor = "default";
        li.style.textAlign = "center";
        ul.appendChild(li);
      }
    }

    function updateInputValue() {
      const selected = select.options[select.selectedIndex];
      input.value = selected ? selected.text : "";
    }

    function isInputValid() {
      const val = input.value.trim();
      if (val === "") return false; // خالی را نامعتبر در نظر می‌گیریم (می‌توانید تغییر دهید)
      for (let opt of select.options) {
        if (opt.text.trim() === val) return true;
      }
      return false;
    }

    function closeDropdown() {
      ul.classList.remove("show");
      // اگر مقدار input نامعتبر است، به مقدار قبلی برگردان
      if (!isInputValid()) {
        select.value = lastValidValue;
        select.dispatchEvent(new Event("change", { bubbles: true }));
      }
      updateInputValue();
    }

    function openDropdown() {
      lastValidValue = select.value; // ذخیره مقدار فعلی
      ul.classList.add("show");
      buildOptions("");
      const val = select.value;
      if (val) {
        const items = ul.querySelectorAll("li");
        items.forEach(li => {
          if (li.dataset.value === val) li.classList.add("active");
        });
      }
    }

    function toggleDropdown() {
      if (ul.classList.contains("show")) closeDropdown();
      else openDropdown();
    }

    // رویدادها
    input.addEventListener("focus", () => {
      openDropdown();
      updateInputValue();
    });

    input.addEventListener("input", () => {
      const filter = input.value;
      buildOptions(filter);
      const firstVisible = ul.querySelector("li:not(.hidden)");
      if (firstVisible) {
        ul.querySelectorAll("li").forEach(li => li.classList.remove("active"));
        firstVisible.classList.add("active");
      }
    });

    // بستن با کلیک بیرون
    document.addEventListener("click", (e) => {
      if (!wrapper.contains(e.target)) {
        closeDropdown();
      }
    });

    // کلیدها
    input.addEventListener("keydown", (e) => {
      const items = [...ul.querySelectorAll("li:not(.hidden)")];
      const activeIndex = items.findIndex(li => li.classList.contains("active"));

      if (e.key === "ArrowDown") {
        e.preventDefault();
        if (items.length) {
          const next = activeIndex < items.length - 1 ? activeIndex + 1 : 0;
          items.forEach(li => li.classList.remove("active"));
          items[next].classList.add("active");
          items[next].scrollIntoView({ block: "nearest" });
        }
      } else if (e.key === "ArrowUp") {
        e.preventDefault();
        if (items.length) {
          const prev = activeIndex > 0 ? activeIndex - 1 : items.length - 1;
          items.forEach(li => li.classList.remove("active"));
          items[prev].classList.add("active");
          items[prev].scrollIntoView({ block: "nearest" });
        }
      } else if (e.key === "Enter") {
        e.preventDefault();
        if (items.length && activeIndex > -1) {
          const li = items[activeIndex];
          select.value = li.dataset.value;
          lastValidValue = select.value;
          select.dispatchEvent(new Event("change", { bubbles: true }));
          updateInputValue();
          closeDropdown();
        } else if (items.length) {
          const first = items[0];
          select.value = first.dataset.value;
          lastValidValue = select.value;
          select.dispatchEvent(new Event("change", { bubbles: true }));
          updateInputValue();
          closeDropdown();
        }
      } else if (e.key === "Escape") {
        closeDropdown();
        input.blur();
      } else if (e.key === "Tab") {
        closeDropdown();
        // اجازه بده tab به فیلد بعدی برود
        return;
      }
    });

    // وقتی select اصلی تغییر کرد (توسط کد دیگر)
    select.addEventListener("change", () => {
      lastValidValue = select.value;
      updateInputValue();
    });

    updateInputValue();
    if (select.dataset.placeholder) {
      input.placeholder = select.dataset.placeholder;
    }
  }

  // ---------- مقداردهی اولیه ----------
  function initSearchableSelects() {
    const selects = document.querySelectorAll("select:not([data-searchable='false'])");
    selects.forEach(select => {
      if (!select.closest(".ss-wrapper")) {
        makeSearchable(select);
      }
    });
  }

  initSearchableSelects();

  // مشاهده تغییرات DOM برای selectهای جدید
  const observer = new MutationObserver(() => {
    const newSelects = document.querySelectorAll("select:not([data-searchable='false']):not(.ss-original)");
    newSelects.forEach(select => {
      if (!select.closest(".ss-wrapper")) {
        makeSearchable(select);
      }
    });
  });
  observer.observe(document.body, { childList: true, subtree: true });

});