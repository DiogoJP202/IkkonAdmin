(() => {
  "use strict";

  const SELECTORS = Object.freeze({
    gateway: "[data-ikkon-gateway]",
    gatewayChoice: "[data-gateway-choice]",
    gatewayDismiss: "[data-gateway-dismiss]",
    heroCarousel: "[data-ikkon-carousel]",
    heroSlide: ".ikkon-hero-carousel-slide",
    heroDot: "[data-carousel-index]",
    heroDots: ".ikkon-hero-carousel-dots",
    reveal: ".reveal",
    header: ".institucional-header",
    nav: "#navLanding",
    navLink: ".nav-link",
    eventGallery: "#eventosGaleria",
    eventGalleryWrap: ".institucional-eventos-media-wrap",
    eventSlide: ".institucional-eventos-photo",
    eventDot: ".institucional-evento-dot"
  });

  const GATEWAY_STORAGE_KEY = "ikkon-gateway-seen";
  const REDUCED_MOTION_QUERY = "(prefers-reduced-motion: reduce)";
  const MOBILE_NAV_MAX_WIDTH = 992;
  const HERO_INTERVAL_MS = 4300;
  const EVENT_GALLERY_INTERVAL_MS = 5200;
  const SWIPE_THRESHOLD_PX = 45;

  const queryAll = (selector, root = document) =>
    Array.from(root.querySelectorAll(selector));

  const prefersReducedMotion = () =>
    window.matchMedia(REDUCED_MOTION_QUERY).matches;

  const readSessionFlag = key => {
    try {
      return window.sessionStorage.getItem(key) === "1";
    } catch {
      return false;
    }
  };

  const writeSessionFlag = key => {
    try {
      window.sessionStorage.setItem(key, "1");
    } catch {
      // The public website remains fully usable when storage is unavailable.
    }
  };

  /**
   * Shared timer and active-index controller for public image carousels.
   * Rendering stays in each component so their existing classes and ARIA remain intact.
   */
  const createAutoRotator = ({ itemCount, initialIndex, intervalMs, render }) => {
    let activeIndex = Math.max(initialIndex, 0);
    let timerId = null;

    const normalizeIndex = index => (index + itemCount) % itemCount;

    const clearTimer = () => {
      if (timerId === null) {
        return;
      }

      window.clearTimeout(timerId);
      timerId = null;
    };

    const update = index => {
      activeIndex = normalizeIndex(index);
      render(activeIndex);
    };

    const schedule = () => {
      clearTimer();

      if (prefersReducedMotion() || document.hidden) {
        return;
      }

      timerId = window.setTimeout(() => {
        update(activeIndex + 1);
        schedule();
      }, intervalMs);
    };

    const goTo = index => {
      update(index);
      schedule();
    };

    return {
      get activeIndex() {
        return activeIndex;
      },
      goTo,
      pause: clearTimer,
      resume: schedule,
      start() {
        update(activeIndex);
        schedule();
      }
    };
  };

  const bindDotNavigation = ({ dots, itemCount, rotator }) => {
    dots.forEach((dot, index) => {
      dot.addEventListener("click", () => {
        rotator.goTo(index);
      });

      dot.addEventListener("keydown", event => {
        if (!["ArrowLeft", "ArrowRight", "Home", "End"].includes(event.key)) {
          return;
        }

        event.preventDefault();
        const destination = event.key === "Home"
          ? 0
          : event.key === "End"
            ? itemCount - 1
            : rotator.activeIndex + (event.key === "ArrowRight" ? 1 : -1);

        rotator.goTo(destination);
        dots[rotator.activeIndex]?.focus();
      });
    });
  };

  const initGateway = () => {
    const gateway = document.querySelector(SELECTORS.gateway);

    if (!gateway) {
      return;
    }

    const choices = queryAll(SELECTORS.gatewayChoice, gateway);
    const dismissButton = gateway.querySelector(SELECTORS.gatewayDismiss);
    const params = new URLSearchParams(window.location.search);
    const forceGateway = params.get("entrada") === "1";

    const rememberGateway = () => {
      writeSessionFlag(GATEWAY_STORAGE_KEY);
    };

    const clearEntranceQuery = () => {
      if (!forceGateway || !window.history.replaceState) {
        return;
      }

      params.delete("entrada");
      const query = params.toString();
      const nextUrl = `${window.location.pathname}${query ? `?${query}` : ""}${window.location.hash}`;
      window.history.replaceState({}, "", nextUrl);
    };

    const setGatewayVisibility = visible => {
      gateway.classList.toggle("is-hidden", !visible);
      gateway.setAttribute("aria-hidden", visible ? "false" : "true");
      document.body.classList.toggle("ikkon-gateway-open", visible);
    };

    const hideGateway = () => {
      rememberGateway();
      setGatewayVisibility(false);
      clearEntranceQuery();
    };

    choices.forEach(choice => {
      const stateClass = choice.dataset.gatewayChoice === "events"
        ? "is-events-hover"
        : "is-school-hover";
      const activate = () => gateway.classList.add(stateClass);
      const deactivate = () => gateway.classList.remove(stateClass);

      choice.addEventListener("mouseenter", activate);
      choice.addEventListener("mouseleave", deactivate);
      choice.addEventListener("focus", activate);
      choice.addEventListener("blur", deactivate);
      choice.addEventListener("click", rememberGateway);
    });

    dismissButton?.addEventListener("click", () => {
      hideGateway();
      document.getElementById("site-home")?.scrollIntoView({ block: "start" });
    });

    gateway.addEventListener("keydown", event => {
      if (event.key === "Escape") {
        hideGateway();
      }
    });

    setGatewayVisibility(forceGateway || !readSessionFlag(GATEWAY_STORAGE_KEY));
  };

  const initHeroCarousels = () => {
    queryAll(SELECTORS.heroCarousel).forEach(carousel => {
      const slides = queryAll(SELECTORS.heroSlide, carousel);
      const dots = queryAll(SELECTORS.heroDot, carousel);

      if (slides.length < 2) {
        return;
      }

      const configuredInterval = Number.parseInt(carousel.dataset.carouselInterval ?? "", 10);
      const intervalMs = Number.isFinite(configuredInterval)
        ? configuredInterval
        : HERO_INTERVAL_MS;
      let pointerStartX = null;

      const rotator = createAutoRotator({
        itemCount: slides.length,
        initialIndex: slides.findIndex(slide => slide.classList.contains("is-active")),
        intervalMs,
        render: activeIndex => {
          slides.forEach((slide, index) => {
            const active = index === activeIndex;
            slide.classList.toggle("is-active", active);
            slide.setAttribute("aria-hidden", active ? "false" : "true");
          });

          dots.forEach((dot, index) => {
            const active = index === activeIndex;
            dot.classList.toggle("is-active", active);
            dot.setAttribute("aria-selected", active ? "true" : "false");
            dot.setAttribute("tabindex", active ? "0" : "-1");
          });
        }
      });

      bindDotNavigation({ dots, itemCount: slides.length, rotator });

      carousel.addEventListener("pointerdown", event => {
        if (event.pointerType === "mouse" || event.target.closest(SELECTORS.heroDots)) {
          return;
        }

        pointerStartX = event.clientX;
      });

      carousel.addEventListener("pointerup", event => {
        if (pointerStartX === null) {
          return;
        }

        const distance = event.clientX - pointerStartX;
        pointerStartX = null;

        if (Math.abs(distance) >= SWIPE_THRESHOLD_PX) {
          rotator.goTo(rotator.activeIndex + (distance < 0 ? 1 : -1));
        }
      });

      carousel.addEventListener("pointercancel", () => {
        pointerStartX = null;
      });

      document.addEventListener("visibilitychange", rotator.resume);
      rotator.start();
    });
  };

  const initRevealAnimations = () => {
    const revealItems = queryAll(SELECTORS.reveal);

    if (
      revealItems.length === 0 ||
      prefersReducedMotion() ||
      !("IntersectionObserver" in window)
    ) {
      revealItems.forEach(item => item.classList.add("is-visible"));
      return;
    }

    const observer = new IntersectionObserver(
      entries => {
        entries.forEach(entry => {
          if (!entry.isIntersecting) {
            return;
          }

          entry.target.classList.add("is-visible");
          observer.unobserve(entry.target);
        });
      },
      { threshold: 0.15, rootMargin: "0px 0px -40px 0px" }
    );

    revealItems.forEach(item => observer.observe(item));
  };

  const initPublicHeader = () => {
    const header = document.querySelector(SELECTORS.header);

    if (!header) {
      return;
    }

    const navLinks = queryAll(SELECTORS.navLink, header);
    const landingNav = document.querySelector(SELECTORS.nav);
    const sectionLinks = navLinks.filter(link => {
      const href = link.getAttribute("href");
      return typeof href === "string" && href.startsWith("#") && href.length > 1;
    });
    const sections = sectionLinks
      .map(link => document.querySelector(link.getAttribute("href")))
      .filter(Boolean);
    let scrollFrameId = null;

    const renderHeaderState = () => {
      header.classList.toggle("is-scrolled", window.scrollY > 16);

      if (sectionLinks.length > 0) {
        const scrollPosition = window.scrollY + 130;
        let activeId = "";

        sections.forEach(section => {
          if (scrollPosition >= section.offsetTop) {
            activeId = `#${section.id}`;
          }
        });

        sectionLinks.forEach(link => {
          link.classList.toggle("active", link.getAttribute("href") === activeId);
        });
      }

      scrollFrameId = null;
    };

    const requestHeaderRender = () => {
      if (scrollFrameId !== null) {
        return;
      }

      scrollFrameId = window.requestAnimationFrame(renderHeaderState);
    };

    renderHeaderState();
    window.addEventListener("scroll", requestHeaderRender, { passive: true });

    queryAll("a", landingNav ?? document.createDocumentFragment()).forEach(link => {
      link.addEventListener("click", () => {
        if (
          window.innerWidth < MOBILE_NAV_MAX_WIDTH &&
          landingNav?.classList.contains("show") &&
          window.bootstrap?.Collapse
        ) {
          window.bootstrap.Collapse.getOrCreateInstance(landingNav).hide();
        }
      });
    });
  };

  const initEventGallery = () => {
    const gallery = document.querySelector(SELECTORS.eventGallery);

    if (!gallery) {
      return;
    }

    const galleryWrap = gallery.closest(SELECTORS.eventGalleryWrap);
    const slides = queryAll(SELECTORS.eventSlide, gallery);
    const dots = queryAll(SELECTORS.eventDot, galleryWrap ?? document);

    if (slides.length < 2) {
      return;
    }

    const rotator = createAutoRotator({
      itemCount: slides.length,
      initialIndex: slides.findIndex(slide => slide.classList.contains("is-active")),
      intervalMs: EVENT_GALLERY_INTERVAL_MS,
      render: activeIndex => {
        slides.forEach((slide, index) => {
          const active = index === activeIndex;
          slide.classList.toggle("is-active", active);
          slide.setAttribute("aria-hidden", active ? "false" : "true");
        });

        dots.forEach((dot, index) => {
          const active = index === activeIndex;
          dot.classList.toggle("is-active", active);
          dot.setAttribute("aria-selected", active ? "true" : "false");
          dot.setAttribute("tabindex", active ? "0" : "-1");
        });
      }
    });

    bindDotNavigation({ dots, itemCount: slides.length, rotator });

    if (galleryWrap) {
      galleryWrap.addEventListener("mouseenter", rotator.pause);
      galleryWrap.addEventListener("mouseleave", rotator.resume);
      galleryWrap.addEventListener("focusin", rotator.pause);
      galleryWrap.addEventListener("focusout", event => {
        if (!galleryWrap.contains(event.relatedTarget)) {
          rotator.resume();
        }
      });
    }

    document.addEventListener("visibilitychange", rotator.resume);
    rotator.start();
  };

  // O projeto não possui envio de e-mail: o formulário de contato monta a
  // mensagem e entrega no WhatsApp, que é o canal oficial da escola.
  const initWhatsAppForms = () => {
    const forms = document.querySelectorAll("[data-whatsapp-form]");

    forms.forEach((form) => {
      form.addEventListener("submit", (event) => {
        event.preventDefault();

        if (typeof form.reportValidity === "function" && !form.reportValidity()) {
          return;
        }

        const numero = form.dataset.whatsappNumber;
        if (!numero) {
          return;
        }

        const valor = (campo) => (form.elements[campo]?.value || "").trim();
        const linhas = [
          valor("nome") && `Nome: ${valor("nome")}`,
          valor("email") && `E-mail: ${valor("email")}`,
          valor("mensagem")
        ].filter(Boolean);

        const url = `https://wa.me/${numero}?text=${encodeURIComponent(linhas.join("\n"))}`;
        window.open(url, "_blank", "noopener");
      });
    });
  };

  const initPublicFrontend = () => {
    initGateway();
    initHeroCarousels();
    initRevealAnimations();
    initPublicHeader();
    initEventGallery();
    initWhatsAppForms();
  };

  initPublicFrontend();
})();
