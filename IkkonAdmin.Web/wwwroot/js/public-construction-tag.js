(() => {
  const constructionTag = document.querySelector("[data-public-hanging-tag]");

  if (!constructionTag) {
    return;
  }

  const tagMessage = constructionTag.querySelector("[data-tag-message]");
  const tagWelcome = constructionTag.querySelector("[data-tag-welcome]");
  const welcomeLabel = constructionTag.getAttribute("data-welcome-label") || "Yōkoso ようこそ";

  window.requestAnimationFrame(() => {
    window.requestAnimationFrame(() => {
      constructionTag.classList.add("is-mounted");
    });
  });

  window.setTimeout(() => {
    constructionTag.classList.add("is-welcome");
    constructionTag.setAttribute("aria-label", welcomeLabel);
    tagMessage?.setAttribute("aria-hidden", "true");
    tagWelcome?.setAttribute("aria-hidden", "false");
  }, 10000);

  window.setTimeout(() => {
    constructionTag.classList.add("is-leaving");
    constructionTag.setAttribute("aria-hidden", "true");
  }, 20000);

  window.setTimeout(() => {
    constructionTag.classList.add("is-gone");
  }, 21600);
})();
