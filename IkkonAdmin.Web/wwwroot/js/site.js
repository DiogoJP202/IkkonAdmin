// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(() => {
  const body = document.body;
  const sidebar = document.getElementById("adminSidebar");

  if (!body?.classList.contains("admin-body") || !sidebar) {
    return;
  }

  const storageKey = "ikkon.admin.sidebar.collapsed";
  const mobileQuery = window.matchMedia("(max-width: 768px)");
  const toggles = document.querySelectorAll("[data-admin-sidebar-toggle]");
  const closers = document.querySelectorAll("[data-admin-sidebar-close]");

  const isMobile = () => mobileQuery.matches;
  const getStoredCollapsed = () => {
    try {
      return window.localStorage?.getItem(storageKey) === "true";
    } catch {
      return false;
    }
  };

  const setStoredCollapsed = (collapsed) => {
    try {
      window.localStorage?.setItem(storageKey, collapsed ? "true" : "false");
    } catch {
      // Ignore storage restrictions; the sidebar still works for the current page.
    }
  };

  const syncToggleState = () => {
    const expanded = isMobile()
      ? body.classList.contains("admin-sidebar-open")
      : !body.classList.contains("admin-sidebar-collapsed");

    toggles.forEach((toggle) => {
      toggle.setAttribute("aria-expanded", expanded ? "true" : "false");
    });
  };

  const applyStoredState = () => {
    if (isMobile()) {
      body.classList.remove("admin-sidebar-open");
      syncToggleState();
      return;
    }

    const collapsed = getStoredCollapsed();
    body.classList.toggle("admin-sidebar-collapsed", collapsed);
    syncToggleState();
  };

  toggles.forEach((toggle) => {
    toggle.addEventListener("click", () => {
      if (isMobile()) {
        body.classList.toggle("admin-sidebar-open");
      } else {
        const collapsed = !body.classList.contains("admin-sidebar-collapsed");
        body.classList.toggle("admin-sidebar-collapsed", collapsed);
        setStoredCollapsed(collapsed);
      }

      syncToggleState();
    });
  });

  closers.forEach((closer) => {
    closer.addEventListener("click", () => {
      body.classList.remove("admin-sidebar-open");
      syncToggleState();
    });
  });

  document.addEventListener("keydown", (event) => {
    if (event.key === "Escape") {
      body.classList.remove("admin-sidebar-open");
      syncToggleState();
    }
  });

  if (typeof mobileQuery.addEventListener === "function") {
    mobileQuery.addEventListener("change", applyStoredState);
  } else if (typeof mobileQuery.addListener === "function") {
    mobileQuery.addListener(applyStoredState);
  }

  applyStoredState();
})();
