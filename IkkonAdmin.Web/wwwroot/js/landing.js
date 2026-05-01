(() => {
  const revealItems = document.querySelectorAll(".reveal");

  if (revealItems.length > 0 && "IntersectionObserver" in window) {
    const observer = new IntersectionObserver(
      entries => {
        entries.forEach(entry => {
          if (entry.isIntersecting) {
            entry.target.classList.add("is-visible");
            observer.unobserve(entry.target);
          }
        });
      },
      { threshold: 0.15, rootMargin: "0px 0px -40px 0px" }
    );

    revealItems.forEach(item => observer.observe(item));
  } else {
    revealItems.forEach(item => item.classList.add("is-visible"));
  }

  const header = document.querySelector(".institucional-header");
  const navLinks = Array.from(document.querySelectorAll(".institucional-header .nav-link"));
  const sectionLinks = navLinks.filter(link => {
    const href = link.getAttribute("href");
    return typeof href === "string" && href.startsWith("#") && href.length > 1;
  });

  const sections = sectionLinks
    .map(link => document.querySelector(link.getAttribute("href")))
    .filter(Boolean);

  const updateHeaderState = () => {
    if (!header) return;
    header.classList.toggle("is-scrolled", window.scrollY > 16);
  };

  const updateActiveLink = () => {
    const scrollPos = window.scrollY + 130;
    let activeId = "";

    sections.forEach(section => {
      if (!section) return;
      if (scrollPos >= section.offsetTop) {
        activeId = `#${section.id}`;
      }
    });

    navLinks.forEach(link => {
      const href = link.getAttribute("href") ?? "";
      if (!href.startsWith("#")) {
        link.classList.remove("active");
        return;
      }

      const isActive = link.getAttribute("href") === activeId;
      link.classList.toggle("active", isActive);
    });
  };

  updateHeaderState();
  updateActiveLink();

  window.addEventListener("scroll", () => {
    updateHeaderState();
    updateActiveLink();
  });

  const eventosGaleria = document.getElementById("eventosGaleria");
  const eventosWrap = document.querySelector(".institucional-eventos-media-wrap");
  const fotosEventos = Array.from(document.querySelectorAll(".institucional-eventos-photo"));
  const dotsEventos = Array.from(document.querySelectorAll(".institucional-evento-dot"));

  if (eventosGaleria && fotosEventos.length > 1) {
    const prefersReducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    const intervaloRotacaoMs = 5200;
    let indiceAtual = Math.max(
      fotosEventos.findIndex(foto => foto.classList.contains("is-active")),
      0
    );
    let timerRotacaoId = null;

    const atualizarGaleria = indice => {
      indiceAtual = (indice + fotosEventos.length) % fotosEventos.length;

      fotosEventos.forEach((foto, idx) => {
        const ativa = idx === indiceAtual;
        foto.classList.toggle("is-active", ativa);
        foto.setAttribute("aria-hidden", ativa ? "false" : "true");
      });

      dotsEventos.forEach((dot, idx) => {
        const ativo = idx === indiceAtual;
        dot.classList.toggle("is-active", ativo);
        dot.setAttribute("aria-selected", ativo ? "true" : "false");
        dot.setAttribute("tabindex", ativo ? "0" : "-1");
      });
    };

    const pararRotacao = () => {
      if (timerRotacaoId === null) {
        return;
      }

      window.clearInterval(timerRotacaoId);
      timerRotacaoId = null;
    };

    const iniciarRotacao = () => {
      if (prefersReducedMotion || timerRotacaoId !== null) {
        return;
      }

      timerRotacaoId = window.setInterval(() => {
        atualizarGaleria(indiceAtual + 1);
      }, intervaloRotacaoMs);
    };

    dotsEventos.forEach((dot, indice) => {
      dot.addEventListener("click", () => {
        atualizarGaleria(indice);
        pararRotacao();
        iniciarRotacao();
      });
    });

    if (eventosWrap) {
      eventosWrap.addEventListener("mouseenter", pararRotacao);
      eventosWrap.addEventListener("mouseleave", iniciarRotacao);
      eventosWrap.addEventListener("focusin", pararRotacao);
      eventosWrap.addEventListener("focusout", event => {
        if (!eventosWrap.contains(event.relatedTarget)) {
          iniciarRotacao();
        }
      });
    }

    atualizarGaleria(indiceAtual);
    iniciarRotacao();
  }
})();
