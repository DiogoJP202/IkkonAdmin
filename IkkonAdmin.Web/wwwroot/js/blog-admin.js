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
    let slugTouched = !!(slugInput && slugInput.value.trim());
    let editor = null;

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
            throw new Error(payload && payload.message ? payload.message : "Nao foi possivel enviar a imagem.");
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
                    window.alert(error && error.message ? error.message : "Nao foi possivel enviar a imagem.");
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

    form.querySelectorAll("[data-blog-submit]").forEach(function (button) {
        button.addEventListener("click", function () {
            syncEditorFields();
            if (actionInput) {
                actionInput.value = button.getAttribute("data-blog-submit") || "Draft";
            }
            form.requestSubmit();
        });
    });

    form.addEventListener("submit", syncEditorFields);

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
