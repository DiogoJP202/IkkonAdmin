(() => {
  const mobileQuery = window.matchMedia("(max-width: 768px)");
  const body = document.body;
  const sidebar = document.getElementById("alunoPortalSidebar");
  const toggle = document.querySelector("[data-aluno-menu-toggle]");
  const closeTargets = document.querySelectorAll("[data-aluno-menu-close]");

  if (!sidebar || !toggle) {
    return;
  }

  const setOpen = open => {
    body.classList.toggle("aluno-menu-open", open);
    toggle.setAttribute("aria-expanded", String(open));
    toggle.setAttribute("aria-label", open ? "Fechar menu da Área do Aluno" : "Abrir menu da Área do Aluno");
    sidebar.setAttribute("aria-hidden", mobileQuery.matches ? String(!open) : "false");
  };

  const closeIfMobile = () => {
    if (mobileQuery.matches) {
      setOpen(false);
    }
  };

  toggle.addEventListener("click", () => {
    setOpen(!body.classList.contains("aluno-menu-open"));
  });

  closeTargets.forEach(target => {
    target.addEventListener("click", closeIfMobile);
  });

  sidebar.querySelectorAll("a").forEach(link => {
    link.addEventListener("click", closeIfMobile);
  });

  document.addEventListener("keydown", event => {
    if (event.key === "Escape") {
      closeIfMobile();
    }
  });

  mobileQuery.addEventListener("change", event => {
    setOpen(!event.matches);
  });

  setOpen(!mobileQuery.matches);
})();
