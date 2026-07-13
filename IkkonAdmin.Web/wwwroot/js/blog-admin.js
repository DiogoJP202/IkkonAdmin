(function () {
    const form = document.getElementById("blogPostForm");
    if (!form) return;

    const titleInput = document.getElementById("Title");
    const slugInput = document.getElementById("Slug");
    const actionInput = document.getElementById("SubmissionAction");
    const coverInput = document.getElementById("CoverImage");
    const coverPreview = document.getElementById("blogCoverPreview");
    const editorElement = document.getElementById("blogContentEditor");
    const contentTextInput = document.getElementById("ContentInput");
    const contentHtmlInput = document.getElementById("ContentHtmlInput");
    const contentJsonInput = document.getElementById("ContentJsonInput");
    const antiForgeryToken = form.querySelector('input[name="__RequestVerificationToken"]');
    const categorySelect = document.getElementById("CategoryId");
    const categoryModal = document.getElementById("blogCategoryModal");
    const categoryModalAlert = document.getElementById("blogCategoryModalAlert");
    const categoryModalList = document.getElementById("blogCategoryModalList");
    const categoryEditId = document.getElementById("blogCategoryEditId");
    const categoryName = document.getElementById("blogCategoryName");
    const categorySlug = document.getElementById("blogCategorySlug");
    const categoryDescription = document.getElementById("blogCategoryDescription");
    const categoryIsActive = document.getElementById("blogCategoryIsActive");
    const categorySaveButton = document.getElementById("blogCategorySaveButton");
    const categoryClearButton = document.getElementById("blogCategoryClearButton");
    const versionsModal = document.getElementById("blogVersionsModal");
    const versionsModalBody = document.getElementById("blogVersionsModalBody");
    const versionsModalAlert = document.getElementById("blogVersionsModalAlert");
    const tagsInput = document.getElementById("TagsInput");
    const tagPicker = document.getElementById("blogTagPicker");
    const tagList = document.getElementById("blogTagList");
    const tagInput = document.getElementById("blogTagInput");
    const addTagButton = document.getElementById("blogAddTagButton");
    let slugTouched = !!(slugInput && slugInput.value.trim());
    let editor = null;
    let tags = [];

    const slugify = function (value) {
        return (value || "")
            .normalize("NFD")
            .replace(/[\u0300-\u036f]/g, "")
            .toLowerCase()
            .replace(/[^a-z0-9]+/g, "-")
            .replace(/^-+|-+$/g, "");
    };

    const normalizeYouTubeUrl = function (value) {
        if (!value) return null;

        try {
            const url = new URL(value.trim());
            const host = url.hostname.toLowerCase();
            let videoId = null;

            if (host === "youtu.be") {
                videoId = url.pathname.replace(/^\/+/, "").split("/")[0];
            } else if (host.endsWith("youtube.com") || host.endsWith("youtube-nocookie.com")) {
                const parts = url.pathname.replace(/^\/+/, "").split("/");
                if ((parts[0] === "embed" || parts[0] === "shorts") && parts[1]) {
                    videoId = parts[1];
                } else if (url.pathname === "/watch") {
                    videoId = url.searchParams.get("v");
                }
            }

            return /^[A-Za-z0-9_-]{11}$/.test(videoId || "")
                ? "https://www.youtube.com/embed/" + videoId
                : null;
        } catch {
            return null;
        }
    };

    const createFormData = function () {
        const body = new FormData();
        if (antiForgeryToken) {
            body.append("__RequestVerificationToken", antiForgeryToken.value);
        }
        return body;
    };

    const requestJson = async function (url, options) {
        const response = await fetch(url, {
            credentials: "same-origin",
            ...options
        });

        let payload = null;
        try {
            payload = await response.json();
        } catch {
            payload = null;
        }

        if (!response.ok || !payload || payload.success === false) {
            throw new Error(payload && payload.message ? payload.message : "Não foi possível concluir a operação.");
        }

        return payload;
    };

    const requestHtml = async function (url) {
        const response = await fetch(url, {
            method: "GET",
            credentials: "same-origin"
        });

        if (!response.ok) {
            throw new Error("Não foi possível carregar as versões do post.");
        }

        return await response.text();
    };

    const showCategoryAlert = function (message, type) {
        if (!categoryModalAlert) return;

        categoryModalAlert.className = "alert " + (type === "success" ? "alert-success" : "alert-danger");
        categoryModalAlert.textContent = message || "";
        categoryModalAlert.classList.toggle("d-none", !message);
    };

    const clearCategoryForm = function () {
        if (categoryEditId) categoryEditId.value = "";
        if (categoryName) categoryName.value = "";
        if (categorySlug) categorySlug.value = "";
        if (categoryDescription) categoryDescription.value = "";
        if (categoryIsActive) categoryIsActive.checked = true;
        if (categorySaveButton) categorySaveButton.textContent = "Salvar categoria";
        categoryName && categoryName.focus();
    };

    const fillCategoryForm = function (category) {
        if (categoryEditId) categoryEditId.value = category.id || "";
        if (categoryName) categoryName.value = category.name || "";
        if (categorySlug) categorySlug.value = category.slug || "";
        if (categoryDescription) categoryDescription.value = category.description || "";
        if (categoryIsActive) categoryIsActive.checked = !!category.isActive;
        if (categorySaveButton) categorySaveButton.textContent = "Atualizar categoria";
        categoryName && categoryName.focus();
    };

    const buildUrlFromTemplate = function (template, id) {
        return (template || "").replace("__id__", encodeURIComponent(id));
    };

    const renderCategoryOptions = function (options, selectedCategoryId) {
        if (!categorySelect) return;

        const selectedValue = selectedCategoryId ? String(selectedCategoryId) : categorySelect.value;
        categorySelect.innerHTML = "";

        const emptyOption = document.createElement("option");
        emptyOption.value = "";
        emptyOption.textContent = "Selecione";
        categorySelect.appendChild(emptyOption);

        (options || []).forEach(function (option) {
            const optionElement = document.createElement("option");
            optionElement.value = String(option.id);
            optionElement.textContent = option.name + (option.isActive ? "" : " (inativa)");
            categorySelect.appendChild(optionElement);
        });

        if (selectedValue && [...categorySelect.options].some(option => option.value === selectedValue)) {
            categorySelect.value = selectedValue;
        }
    };

    const renderCategoryList = function (categories) {
        if (!categoryModalList) return;

        categoryModalList.innerHTML = "";

        if (!categories || categories.length === 0) {
            const empty = document.createElement("div");
            empty.className = "blog-admin-category-empty";
            empty.textContent = "Nenhuma categoria cadastrada.";
            categoryModalList.appendChild(empty);
            return;
        }

        categories.forEach(function (category) {
            const item = document.createElement("article");
            item.className = "blog-admin-category-item";

            const content = document.createElement("div");
            content.className = "blog-admin-category-item-copy";

            const name = document.createElement("strong");
            name.textContent = category.name || "-";

            const meta = document.createElement("small");
            meta.textContent = (category.slug ? "/" + category.slug : "sem slug") + " - " + (category.totalPosts || 0) + " post(s)";

            const description = document.createElement("span");
            description.textContent = category.description || "Sem descrição.";

            const badge = document.createElement("em");
            badge.className = "admin-panel-badge " + (category.isActive ? "is-success" : "is-muted");
            badge.textContent = category.isActive ? "Ativa" : "Inativa";

            content.append(name, meta, description, badge);

            const actions = document.createElement("div");
            actions.className = "blog-admin-category-item-actions";

            const editButton = document.createElement("button");
            editButton.type = "button";
            editButton.className = "btn admin-panel-btn-table";
            editButton.textContent = "Editar";
            editButton.addEventListener("click", function () {
                fillCategoryForm(category);
            });

            const statusButton = document.createElement("button");
            statusButton.type = "button";
            statusButton.className = "btn admin-panel-btn-table " + (category.isActive ? "is-warning" : "is-success");
            statusButton.textContent = category.isActive ? "Desativar" : "Ativar";
            statusButton.addEventListener("click", function () {
                toggleCategoryStatus(category.id, !category.isActive);
            });

            const deleteButton = document.createElement("button");
            deleteButton.type = "button";
            deleteButton.className = "btn admin-panel-btn-table is-danger";
            deleteButton.textContent = "Excluir";
            deleteButton.disabled = (category.totalPosts || 0) > 0;
            deleteButton.title = deleteButton.disabled ? "Categorias com posts devem ser desativadas." : "Excluir categoria";
            deleteButton.addEventListener("click", function () {
                deleteCategory(category.id);
            });

            actions.append(editButton, statusButton, deleteButton);
            item.append(content, actions);
            categoryModalList.appendChild(item);
        });
    };

    const applyCategoryPayload = function (payload, selectedCategoryId) {
        const data = payload && payload.data ? payload.data : payload;
        renderCategoryOptions(data.options || [], selectedCategoryId || data.selectedCategoryId);
        renderCategoryList(data.categories || []);
    };

    const loadCategories = async function (selectedCategoryId) {
        if (!categoryModal) return;

        const url = new URL(categoryModal.getAttribute("data-modal-data-url"), window.location.origin);
        if (selectedCategoryId) {
            url.searchParams.set("selectedCategoryId", selectedCategoryId);
        }

        const payload = await requestJson(url.toString(), { method: "GET" });
        applyCategoryPayload(payload, selectedCategoryId);
    };

    const saveCategory = async function () {
        if (!categoryModal || !categoryName || !categorySaveButton) return;

        const editId = categoryEditId && categoryEditId.value ? categoryEditId.value : "";
        const template = editId
            ? categoryModal.getAttribute("data-edit-url-template")
            : categoryModal.getAttribute("data-create-url");
        const url = editId ? buildUrlFromTemplate(template, editId) : template;

        const body = createFormData();
        body.append("Name", categoryName.value);
        body.append("Slug", categorySlug ? categorySlug.value : "");
        body.append("Description", categoryDescription ? categoryDescription.value : "");
        body.append("IsActive", categoryIsActive && categoryIsActive.checked ? "true" : "false");

        categorySaveButton.disabled = true;
        try {
            const payload = await requestJson(url, { method: "POST", body });
            applyCategoryPayload(payload, payload.entityId);
            if (categorySelect && payload.entityId) categorySelect.value = String(payload.entityId);
            showCategoryAlert(payload.message, "success");
            clearCategoryForm();
        } catch (error) {
            showCategoryAlert(error && error.message ? error.message : "Não foi possível salvar a categoria.", "error");
        } finally {
            categorySaveButton.disabled = false;
        }
    };

    const toggleCategoryStatus = async function (id, active) {
        if (!categoryModal) return;

        const body = createFormData();
        body.append("ativo", active ? "true" : "false");
        body.append("selectedCategoryId", categorySelect ? categorySelect.value : "");

        try {
            const payload = await requestJson(buildUrlFromTemplate(categoryModal.getAttribute("data-status-url-template"), id), {
                method: "POST",
                body
            });
            applyCategoryPayload(payload, categorySelect ? categorySelect.value : null);
            showCategoryAlert(payload.message, "success");
        } catch (error) {
            showCategoryAlert(error && error.message ? error.message : "Não foi possível atualizar a categoria.", "error");
        }
    };

    const deleteCategory = async function (id) {
        if (!categoryModal || !window.confirm("Excluir esta categoria?")) return;

        const body = createFormData();
        body.append("selectedCategoryId", categorySelect ? categorySelect.value : "");

        try {
            const payload = await requestJson(buildUrlFromTemplate(categoryModal.getAttribute("data-delete-url-template"), id), {
                method: "POST",
                body
            });
            applyCategoryPayload(payload, payload.data && payload.data.selectedCategoryId);
            if (categorySelect && String(categorySelect.value) === String(id)) categorySelect.value = "";
            showCategoryAlert(payload.message, "success");
            if (categoryEditId && String(categoryEditId.value) === String(id)) clearCategoryForm();
        } catch (error) {
            showCategoryAlert(error && error.message ? error.message : "Não foi possível excluir a categoria.", "error");
        }
    };

    const showVersionsAlert = function (message, type) {
        if (!versionsModalAlert) return;

        versionsModalAlert.className = "alert " + (type === "success" ? "alert-success" : "alert-danger");
        versionsModalAlert.textContent = message || "";
        versionsModalAlert.classList.toggle("d-none", !message);
    };

    const loadVersions = async function () {
        if (!versionsModal || !versionsModalBody) return;

        const url = versionsModal.getAttribute("data-versions-url");
        if (!url) return;

        versionsModalBody.innerHTML = '<div class="blog-admin-category-empty">Carregando versões...</div>';
        const html = await requestHtml(url);
        versionsModalBody.innerHTML = html;
    };

    const createVersion = async function (button) {
        if (!versionsModal || !button) return;

        const url = versionsModal.getAttribute("data-create-url");
        const languageCode = button.getAttribute("data-language-code");
        if (!url || !languageCode) return;

        const body = createFormData();
        body.append("languageCode", languageCode);

        button.disabled = true;
        try {
            const payload = await requestJson(url, { method: "POST", body });
            showVersionsAlert(payload.message, "success");
            if (payload.redirectUrl) {
                window.location.href = payload.redirectUrl;
                return;
            }

            await loadVersions();
        } catch (error) {
            showVersionsAlert(error && error.message ? error.message : "Não foi possível criar a versão.", "error");
        } finally {
            button.disabled = false;
        }
    };

    const deleteVersion = async function (button) {
        if (!versionsModal || !button) return;

        const versionId = button.getAttribute("data-version-id");
        const languageLabel = button.getAttribute("data-language-label") || "esta versão";
        if (!versionId || !window.confirm("Excluir a versão " + languageLabel + "?")) return;

        const url = buildUrlFromTemplate(versionsModal.getAttribute("data-delete-url-template"), versionId);
        const body = createFormData();

        button.disabled = true;
        try {
            const payload = await requestJson(url, { method: "POST", body });
            showVersionsAlert(payload.message, "success");
            await loadVersions();
        } catch (error) {
            showVersionsAlert(error && error.message ? error.message : "Não foi possível excluir a versão.", "error");
        } finally {
            button.disabled = false;
        }
    };

    const normalizeTag = function (value) {
        return (value || "")
            .replace(/^#+/, "")
            .replace(/\s+/g, " ")
            .trim();
    };

    const syncTags = function () {
        if (tagsInput) {
            tagsInput.value = tags.join(", ");
        }
    };

    const renderTags = function () {
        if (!tagList) return;

        tagList.innerHTML = "";
        if (tags.length === 0) {
            const empty = document.createElement("span");
            empty.className = "blog-admin-tag-empty";
            empty.textContent = "Nenhuma tag adicionada.";
            tagList.appendChild(empty);
            syncTags();
            return;
        }

        tags.forEach(function (tag) {
            const chip = document.createElement("span");
            chip.className = "blog-admin-tag-chip";

            const text = document.createElement("span");
            text.textContent = "#" + tag;

            const remove = document.createElement("button");
            remove.type = "button";
            remove.setAttribute("aria-label", "Remover tag " + tag);
            remove.textContent = "x";
            remove.addEventListener("click", function () {
                tags = tags.filter(item => item.toLowerCase() !== tag.toLowerCase());
                renderTags();
            });

            chip.append(text, remove);
            tagList.appendChild(chip);
        });

        syncTags();
    };

    const addTagsFromValue = function (value) {
        const incoming = (value || "")
            .split(/[,;\n\r]+/)
            .map(normalizeTag)
            .filter(Boolean);

        incoming.forEach(function (tag) {
            if (tags.length >= 12) return;
            if (tag.length > 40) tag = tag.substring(0, 40).trim();
            if (!tags.some(item => item.toLowerCase() === tag.toLowerCase())) {
                tags.push(tag);
            }
        });

        if (tagInput) tagInput.value = "";
        renderTags();
    };

    const initializeTags = function () {
        if (!tagsInput || !tagPicker) return;

        tags = [];
        addTagsFromValue(tagsInput.value);
    };

    const syncEditorFields = function () {
        if (!editor || !contentTextInput || !contentHtmlInput || !contentJsonInput) return;

        const plainText = editor.getText().trim();
        const hasMedia = !!editor.root.querySelector("img, iframe");
        contentTextInput.value = plainText;
        contentHtmlInput.value = plainText.length === 0 && !hasMedia ? "" : editor.root.innerHTML;
        contentJsonInput.value = plainText.length === 0 && !hasMedia ? "" : JSON.stringify(editor.getContents());
    };

    const uploadEditorImage = async function (file) {
        const uploadUrl = editorElement && editorElement.getAttribute("data-image-upload-url");
        if (!uploadUrl || !file) return null;

        const body = new FormData();
        body.append("image", file);
        if (antiForgeryToken) {
            body.append("__RequestVerificationToken", antiForgeryToken.value);
        }

        const response = await fetch(uploadUrl, {
            method: "POST",
            body: body,
            credentials: "same-origin"
        });

        let payload = null;
        try {
            payload = await response.json();
        } catch {
            payload = null;
        }

        if (!response.ok || !payload || !payload.success || !payload.url) {
            throw new Error(payload && payload.message ? payload.message : "Não foi possível enviar a imagem.");
        }

        return payload.url;
    };

    const loadEditorContent = function () {
        if (!editor) return;

        const json = contentJsonInput && contentJsonInput.value ? contentJsonInput.value.trim() : "";
        const html = contentHtmlInput && contentHtmlInput.value ? contentHtmlInput.value.trim() : "";
        const text = contentTextInput && contentTextInput.value ? contentTextInput.value.trim() : "";

        if (json) {
            try {
                editor.setContents(JSON.parse(json));
                return;
            } catch {
                // Fallback to sanitized HTML/text from previous saves.
            }
        }

        if (html) {
            editor.clipboard.dangerouslyPasteHTML(html);
            return;
        }

        if (text) {
            editor.setText(text);
        }
    };

    if (editorElement && window.Quill) {
        editor = new window.Quill(editorElement, {
            theme: "snow",
            placeholder: editorElement.getAttribute("data-placeholder") || "",
            modules: {
                toolbar: "#blogEditorToolbar"
            }
        });

        const toolbar = editor.getModule("toolbar");
        toolbar.addHandler("video", function () {
            const rawUrl = window.prompt("URL do YouTube");
            const embedUrl = normalizeYouTubeUrl(rawUrl);
            if (!embedUrl) return;

            const range = editor.getSelection(true);
            editor.insertEmbed(range.index, "video", embedUrl, "user");
            editor.setSelection(range.index + 1, 0, "silent");
            syncEditorFields();
        });

        toolbar.addHandler("image", function () {
            const input = document.createElement("input");
            input.type = "file";
            input.accept = "image/jpeg,image/png,image/webp";

            input.addEventListener("change", async function () {
                const file = input.files && input.files[0];
                if (!file) return;

                try {
                    const imageUrl = await uploadEditorImage(file);
                    if (!imageUrl) return;

                    const range = editor.getSelection(true);
                    editor.insertEmbed(range.index, "image", imageUrl, "user");
                    editor.setSelection(range.index + 1, 0, "silent");
                    syncEditorFields();
                } catch (error) {
                    window.alert(error && error.message ? error.message : "Não foi possível enviar a imagem.");
                }
            });

            input.click();
        });

        loadEditorContent();
        editor.on("text-change", syncEditorFields);
        syncEditorFields();
    }

    slugInput && slugInput.addEventListener("input", function () {
        slugTouched = slugInput.value.trim().length > 0;
    });

    titleInput && titleInput.addEventListener("input", function () {
        if (!slugInput || slugTouched) return;
        slugInput.value = slugify(titleInput.value);
    });

    if (categoryModal) {
        categoryModal.addEventListener("shown.bs.modal", function () {
            showCategoryAlert("", "success");
            loadCategories(categorySelect ? categorySelect.value : null).catch(function (error) {
                showCategoryAlert(error && error.message ? error.message : "Não foi possível carregar categorias.", "error");
            });
        });
    }

    categorySaveButton && categorySaveButton.addEventListener("click", saveCategory);
    categoryClearButton && categoryClearButton.addEventListener("click", function () {
        showCategoryAlert("", "success");
        clearCategoryForm();
    });

    if (versionsModal) {
        versionsModal.addEventListener("shown.bs.modal", function () {
            showVersionsAlert("", "success");
            loadVersions().catch(function (error) {
                showVersionsAlert(error && error.message ? error.message : "Não foi possível carregar as versões.", "error");
            });
        });

        versionsModal.addEventListener("click", function (event) {
            const target = event.target instanceof Element ? event.target : null;
            if (!target) return;

            const createButton = target.closest("[data-blog-version-create]");
            if (createButton) {
                createVersion(createButton);
                return;
            }

            const deleteButton = target.closest("[data-blog-version-delete]");
            if (deleteButton) {
                deleteVersion(deleteButton);
            }
        });
    }

    initializeTags();

    addTagButton && addTagButton.addEventListener("click", function () {
        addTagsFromValue(tagInput ? tagInput.value : "");
    });

    tagInput && tagInput.addEventListener("keydown", function (event) {
        if (event.key === "Enter" || event.key === "," || (event.key === "Tab" && tagInput.value.trim())) {
            event.preventDefault();
            addTagsFromValue(tagInput.value);
            return;
        }

        if (event.key === "Backspace" && !tagInput.value && tags.length > 0) {
            tags.pop();
            renderTags();
        }
    });

    tagInput && tagInput.addEventListener("paste", function (event) {
        const pasted = event.clipboardData && event.clipboardData.getData("text");
        if (!pasted || !/[,;\n\r]/.test(pasted)) return;

        event.preventDefault();
        addTagsFromValue(pasted);
    });

    form.querySelectorAll("[data-blog-submit]").forEach(function (button) {
        button.addEventListener("click", function () {
            syncEditorFields();
            syncTags();
            if (actionInput) {
                actionInput.value = button.getAttribute("data-blog-submit") || "Draft";
            }
            form.requestSubmit();
        });
    });

    form.addEventListener("submit", function () {
        syncEditorFields();
        syncTags();
    });

    coverInput && coverInput.addEventListener("change", function () {
        const file = coverInput.files && coverInput.files[0];
        if (!file || !coverPreview) return;

        const reader = new FileReader();
        reader.onload = function (event) {
            coverPreview.src = event.target && event.target.result ? event.target.result : coverPreview.src;
        };
        reader.readAsDataURL(file);
    });
})();
