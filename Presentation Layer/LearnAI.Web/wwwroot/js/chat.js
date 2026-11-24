const sessionId = Math.random().toString(36).substring(2, 15);
let activeAbortController = null;

document.addEventListener("DOMContentLoaded", () => {
    const textarea = document.getElementById("userInput");
    if (!textarea) return;

    textarea.addEventListener("input", () => {
        textarea.style.height = "auto";
        textarea.style.height = Math.min(textarea.scrollHeight, 150) + "px";
    });

    textarea.addEventListener("keydown", (e) => {
        if (e.key === "Enter" && !e.shiftKey) {
            e.preventDefault();
            if (!activeAbortController) {
                document.getElementById("chatForm").requestSubmit();
            }
        }
    });
});

function useStarterPrompt(text) {
    const textarea = document.getElementById("userInput");
    if (!textarea) return;
    textarea.value = text;
    document.getElementById("chatForm").requestSubmit();
}

function toggleStreamingButton(isStreaming) {
    const btn = document.getElementById("sendBtn");
    const sendIcon = document.getElementById("sendIcon");
    const stopIcon = document.getElementById("stopIcon");

    if (isStreaming) {
        btn.classList.add("is-streaming");
        btn.setAttribute("aria-label", "Stop generating");
        sendIcon.classList.add("d-none");
        stopIcon.classList.remove("d-none");
    } else {
        btn.classList.remove("is-streaming");
        btn.setAttribute("aria-label", "Send message");
        sendIcon.classList.remove("d-none");
        stopIcon.classList.add("d-none");
    }
}

function renderMarkdown(rawText) {
    if (typeof marked === "undefined" || typeof DOMPurify === "undefined") {
        return rawText;
    }

    const html = marked.parse(rawText, { breaks: true, gfm: true });
    const cleanHtml = DOMPurify.sanitize(html);
    const tempDiv = document.createElement("div");
    tempDiv.innerHTML = cleanHtml;

    tempDiv.querySelectorAll("pre code").forEach((codeEl) => {
        if (typeof hljs !== "undefined") {
            hljs.highlightElement(codeEl);
        }

        const pre = codeEl.parentElement;
        const langClass = Array.from(codeEl.classList).find((c) => c.startsWith("language-"));
        const lang = langClass ? langClass.replace("language-", "") : "code";

        const wrapper = document.createElement("div");
        wrapper.className = "code-block-wrapper";

        const header = document.createElement("div");
        header.className = "code-block-header";
        header.innerHTML = `<span>${lang}</span>`;

        const copyBtn = document.createElement("button");
        copyBtn.type = "button";
        copyBtn.className = "btn-copy-code";
        copyBtn.textContent = "Copy";
        copyBtn.onclick = () => {
            navigator.clipboard.writeText(codeEl.textContent);
            copyBtn.textContent = "Copied!";
            setTimeout(() => (copyBtn.textContent = "Copy"), 1500);
        };

        header.appendChild(copyBtn);
        pre.parentNode.insertBefore(wrapper, pre);
        wrapper.appendChild(header);
        wrapper.appendChild(pre);
    });

    return tempDiv.innerHTML;
}

async function handleChatSubmit(event) {
    event.preventDefault();

    if (activeAbortController) {
        activeAbortController.abort();
        activeAbortController = null;
        return;
    }

    const inputElement = document.getElementById("userInput");
    const chatHistory = document.getElementById("chatHistory");
    const modelSelector = document.getElementById("modelSelector");
    const systemPromptInput = document.getElementById("systemPromptInput");
    const temperatureInput = document.getElementById("temperatureInput");
    const maxTokensInput = document.getElementById("maxTokensInput");

    const question = inputElement.value.trim();
    if (!question) return;

    const welcomeHero = document.getElementById("welcomeHero");
    if (welcomeHero) {
        welcomeHero.remove();
    }

    const temperature = temperatureInput ? temperatureInput.value : 0.7;
    const maxTokens = maxTokensInput ? maxTokensInput.value : 1000;
    const systemPrompt = systemPromptInput ? systemPromptInput.value.trim() : "";

    inputElement.value = "";
    inputElement.style.height = "auto";
    inputElement.disabled = true;

    const chatFeed = chatHistory.querySelector(".ai-chat-feed") || chatHistory;

    const userRow = document.createElement("div");
    userRow.className = "msg-user-row";

    const userBubble = document.createElement("div");
    userBubble.className = "msg-user-bubble";
    userBubble.textContent = question;

    userRow.appendChild(userBubble);
    chatFeed.appendChild(userRow);
    chatHistory.scrollTop = chatHistory.scrollHeight;

    const sessionTitle = document.getElementById("activeSessionTitle");
    if (sessionTitle && sessionTitle.textContent === "Current Conversation") {
        sessionTitle.textContent = question;
    }

    const aiRow = document.createElement("div");
    aiRow.className = "msg-ai-row";

    const aiBadge = document.createElement("div");
    aiBadge.className = "ai-badge";
    aiBadge.textContent = "AI";

    const aiContent = document.createElement("div");
    aiContent.className = "markdown-body";
    aiContent.innerHTML = `<div class="typing-dots"><span></span><span></span><span></span></div>`;

    aiRow.appendChild(aiBadge);
    aiRow.appendChild(aiContent);
    chatFeed.appendChild(aiRow);
    chatHistory.scrollTop = chatHistory.scrollHeight;

    const model = modelSelector.value;
    const url = `/Chat/Stream?question=${encodeURIComponent(question)}&model=${encodeURIComponent(model)}&sessionId=${encodeURIComponent(sessionId)}&systemPrompt=${encodeURIComponent(systemPrompt)}&temperature=${encodeURIComponent(temperature)}&maxTokens=${encodeURIComponent(maxTokens)}`;

    activeAbortController = new AbortController();
    toggleStreamingButton(true);

    let fullMarkdown = "";

    try {
        const response = await fetch(url, { signal: activeAbortController.signal });
        if (!response.ok) throw new Error("Stream connection failed");

        const reader = response.body.getReader();
        const decoder = new TextDecoder();
        let buffer = "";

        const appendSseLine = (line) => {
            if (!line.startsWith("data: ")) return;
            const rawPayload = line.substring(6);
            let cleanText = rawPayload;

            try {
                cleanText = JSON.parse(rawPayload);
            } catch {
            }

            fullMarkdown += cleanText;
            aiContent.innerHTML = renderMarkdown(fullMarkdown);
            chatHistory.scrollTop = chatHistory.scrollHeight;
        };

        while (true) {
            const { done, value } = await reader.read();

            if (done) {
                if (buffer) {
                    appendSseLine(buffer.trim());
                }
                break;
            }

            buffer += decoder.decode(value, { stream: true });
            const lines = buffer.split("\n");
            buffer = lines.pop() || "";

            for (const line of lines) {
                appendSseLine(line.trim());
            }
        }
    } catch (error) {
        if (error.name !== "AbortError") {
            aiContent.textContent = "Error: Failed to fetch response from local server.";
            aiContent.classList.add("text-danger");
        }
    } finally {
        activeAbortController = null;
        toggleStreamingButton(false);
        inputElement.disabled = false;
        inputElement.focus();
    }
}