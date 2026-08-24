(() => {
  const mobileQuery = window.matchMedia("(max-width: 767px)");
  const body = document.body;
  const sidebar = document.getElementById("alunoPortalSidebar");
  const toggle = document.querySelector("[data-aluno-menu-toggle]");
  const closeTargets = document.querySelectorAll("[data-aluno-menu-close]");

  if (!sidebar || !toggle) {
    return;
  }

  const focusableSelector = [
    "a[href]",
    "button:not([disabled])",
    "input:not([disabled])",
    "select:not([disabled])",
    "textarea:not([disabled])",
    "[tabindex]:not([tabindex='-1'])"
  ].join(",");

  const setOpen = (open, restoreFocus = true) => {
    const isMobile = mobileQuery.matches;
    const shouldOpen = isMobile && open;

    if (!shouldOpen && restoreFocus && sidebar.contains(document.activeElement)) {
      toggle.focus();
    }

    body.classList.toggle("aluno-menu-open", shouldOpen);
    toggle.setAttribute("aria-expanded", String(shouldOpen));
    toggle.setAttribute(
      "aria-label",
      shouldOpen ? toggle.dataset.closeLabel : toggle.dataset.openLabel
    );
    sidebar.setAttribute("aria-hidden", isMobile ? String(!shouldOpen) : "false");
    sidebar.inert = isMobile && !shouldOpen;

    if (shouldOpen) {
      const firstFocusable = sidebar.querySelector(focusableSelector);
      window.requestAnimationFrame(() => firstFocusable?.focus());
    }
  };

  toggle.addEventListener("click", () => {
    setOpen(!body.classList.contains("aluno-menu-open"));
  });

  closeTargets.forEach(target => {
    target.addEventListener("click", () => setOpen(false));
  });

  sidebar.querySelectorAll("a").forEach(link => {
    link.addEventListener("click", () => {
      if (mobileQuery.matches) {
        setOpen(false, false);
      }
    });
  });

  document.addEventListener("keydown", event => {
    if (!mobileQuery.matches || !body.classList.contains("aluno-menu-open")) {
      return;
    }

    if (event.key === "Escape") {
      event.preventDefault();
      setOpen(false);
      return;
    }

    if (event.key !== "Tab") {
      return;
    }

    const focusable = Array.from(sidebar.querySelectorAll(focusableSelector));
    if (focusable.length === 0) {
      return;
    }

    const first = focusable[0];
    const last = focusable[focusable.length - 1];
    if (event.shiftKey && document.activeElement === first) {
      event.preventDefault();
      last.focus();
    } else if (!event.shiftKey && document.activeElement === last) {
      event.preventDefault();
      first.focus();
    }
  });

  /* Minimizar o menu no desktop. Mesmo contrato do painel administrativo
     (ver site.js): a preferência fica no localStorage e vale entre páginas. */
  const collapseToggle = document.querySelector("[data-aluno-menu-collapse]");
  const collapseKey = "ikkon.aluno.sidebar.collapsed";

  const lerColapsado = () => {
    try {
      return window.localStorage?.getItem(collapseKey) === "true";
    } catch {
      return false;
    }
  };

  const gravarColapsado = collapsed => {
    try {
      window.localStorage?.setItem(collapseKey, collapsed ? "true" : "false");
    } catch {
      // Sem storage o menu continua funcionando, só não lembra da escolha.
    }
  };

  /* Minimizado, o rótulo sai do fluxo e sobra o número do item. O `title`
     devolve o nome no hover; sem ele o mouse ficaria sem pista nenhuma. */
  const sincronizarTitles = collapsed => {
    sidebar
      .querySelectorAll(".aluno-portal-nav a, .aluno-portal-sidebar-footer a, .aluno-portal-sidebar-footer button")
      .forEach(item => {
        const rotulo = item.querySelector(".aluno-portal-nav-text, .aluno-portal-footer-text");
        if (!rotulo) {
          return;
        }

        if (collapsed) {
          item.setAttribute("title", rotulo.textContent.trim());
        } else {
          item.removeAttribute("title");
        }
      });
  };

  const aplicarColapso = (collapsed, persistir = true) => {
    body.classList.toggle("aluno-sidebar-collapsed", collapsed);
    sincronizarTitles(collapsed);

    if (collapseToggle) {
      collapseToggle.setAttribute("aria-expanded", String(!collapsed));
      collapseToggle.setAttribute(
        "aria-label",
        collapsed ? collapseToggle.dataset.expandLabel : collapseToggle.dataset.collapseLabel
      );
    }

    if (persistir) {
      gravarColapsado(collapsed);
    }
  };

  const restaurarColapso = () => {
    if (mobileQuery.matches) {
      // Na gaveta o menu abre inteiro; o estado minimizado não se aplica.
      aplicarColapso(false, false);
      return;
    }

    aplicarColapso(lerColapsado(), false);
  };

  collapseToggle?.addEventListener("click", () => {
    aplicarColapso(!body.classList.contains("aluno-sidebar-collapsed"));
  });

  mobileQuery.addEventListener("change", event => {
    restaurarColapso();

    if (event.matches) {
      setOpen(false, false);
    } else {
      body.classList.remove("aluno-menu-open");
      toggle.setAttribute("aria-expanded", "false");
      toggle.setAttribute("aria-label", toggle.dataset.openLabel);
      sidebar.setAttribute("aria-hidden", "false");
      sidebar.inert = false;
    }
  });

  setOpen(false, false);
  restaurarColapso();
})();
