(() => {
  const feedback = document.getElementById("userSettingsFeedback");
  const forms = document.querySelectorAll("form[data-config-form]");

  const clearFieldErrors = form => {
    form.querySelectorAll("[data-field-error]").forEach(el => {
      el.textContent = "";
    });
  };

  const showFieldErrors = (form, errors) => {
    if (!errors) return;

    Object.entries(errors).forEach(([key, messages]) => {
      const directMatch = form.querySelector(`[data-field-error="${key}"]`);
      const fallbackMatch = form.querySelector(`[data-field-error="${key.split(".").pop()}"]`);
      const target = directMatch || fallbackMatch;
      if (!target) return;

      target.textContent = Array.isArray(messages) ? messages.join(" ") : String(messages);
    });
  };

  const showFeedback = (type, message) => {
    if (!feedback) return;

    feedback.className = `alert alert-${type}`;
    feedback.textContent = message;
    feedback.classList.remove("d-none");
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

  const applyLoadingState = (button, loading) => {
    if (!button) return;

    if (loading) {
      button.dataset.originalText = button.innerHTML;
      const loadingText = button.dataset.loadingText || "Salvando...";
      button.disabled = true;
      button.innerHTML = `<span class="spinner-border spinner-border-sm me-2" aria-hidden="true"></span>${loadingText}`;
      return;
    }

    if (button.dataset.originalText) {
      button.innerHTML = button.dataset.originalText;
      delete button.dataset.originalText;
    }

    button.disabled = false;
  };

  forms.forEach(form => {
    form.addEventListener("submit", async event => {
      event.preventDefault();
      clearFieldErrors(form);

      const submitButton = form.querySelector("button[type='submit']");
      applyLoadingState(submitButton, true);

      try {
        const response = await fetch(form.action, {
          method: "POST",
          body: new FormData(form)
        });

        let payload = null;
        try {
          payload = await response.json();
        } catch {
          payload = null;
        }

        if (!response.ok || !payload?.success) {
          showFieldErrors(form, payload?.errors);
          showFeedback("danger", payload?.message || "Não foi possível salvar as alterações.");
          return;
        }

        showFeedback("success", payload.message || "Alterações salvas com sucesso.");

        if (form.dataset.resetOnSuccess === "true") {
          form.reset();
        }
      } catch {
        showFeedback("danger", "Falha de comunicação. Tente novamente em instantes.");
      } finally {
        applyLoadingState(submitButton, false);
      }
    });
  });

  const photoInputs = document.querySelectorAll("[data-photo-input]");
  photoInputs.forEach(input => {
    input.addEventListener("change", () => {
      const targetSelector = input.dataset.photoPreview;
      const preview = targetSelector ? document.querySelector(targetSelector) : null;
      const file = input.files && input.files.length ? input.files[0] : null;
      if (!preview || !file) return;

      const reader = new FileReader();
      reader.onload = event => {
        if (typeof event.target?.result === "string") {
          preview.src = event.target.result;
        }
      };
      reader.readAsDataURL(file);
    });
  });

})();
