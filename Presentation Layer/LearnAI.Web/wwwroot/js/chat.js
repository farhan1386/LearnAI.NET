const sessionId = Math.random().toString(36).substring(2, 15);

async function handleChatSubmit(event) {
    event.preventDefault();

    const inputElement = document.getElementById("userInput");
    const chatHistory = document.getElementById("chatHistory");
    const sendBtn = document.getElementById("sendBtn");
    const modelSelector = document.getElementById("modelSelector");
    const temperatureInput = document.getElementById("temperatureInput");
    const maxTokensInput = document.getElementById("maxTokensInput");

    const question = inputElement.value.trim();
    if (!question) return;

    const temperature = temperatureInput ? temperatureInput.value : 0.7;
    const maxTokens = maxTokensInput ? maxTokensInput.value : 1000;

    inputElement.value = "";
    inputElement.disabled = true;
    sendBtn.disabled = true;

    const chatContainer = chatHistory.querySelector(".chat-container") || chatHistory;

    const userMsgDiv = document.createElement("div");
    userMsgDiv.className = "msg-user-wrapper";
    userMsgDiv.innerHTML = `<div class="msg-user">${escapeHtml(question)}</div>`;
    chatContainer.appendChild(userMsgDiv);
    chatHistory.scrollTop = chatHistory.scrollHeight;

    const sidebarHistory = document.getElementById("sidebarHistory");
    if (sidebarHistory) {
        const activeItem = sidebarHistory.querySelector(".history-item.active");
        if (activeItem) {
            const titleSpan = activeItem.querySelector(".text-truncate");
            if (titleSpan && (titleSpan.textContent === "Current Conversation" || sidebarHistory.children.length === 1)) {
                titleSpan.textContent = question;
            }
        }
    }

    const assistantMsgDiv = document.createElement("div");
    assistantMsgDiv.className = "d-flex gap-3 mb-4 align-items-start";

    const avatarDiv = document.createElement("div");
    avatarDiv.className = "avatar-box bg-dark text-white";
    avatarDiv.textContent = "🤖";

    const assistantTextDiv = document.createElement("div");
    assistantTextDiv.className = "message-text";
    assistantTextDiv.textContent = "...";

    assistantMsgDiv.appendChild(avatarDiv);
    assistantMsgDiv.appendChild(assistantTextDiv);
    chatContainer.appendChild(assistantMsgDiv);
    chatHistory.scrollTop = chatHistory.scrollHeight;

    const model = modelSelector.value;
    const url = `/Chat/Stream?question=${encodeURIComponent(question)}&model=${encodeURIComponent(model)}&sessionId=${encodeURIComponent(sessionId)}&temperature=${encodeURIComponent(temperature)}&maxTokens=${encodeURIComponent(maxTokens)}`;

    try {
        const response = await fetch(url);
        if (!response.ok) throw new Error("Stream connection failed");

        const reader = response.body.getReader();
        const decoder = new TextDecoder();
        let firstChunk = true;
        let buffer = "";

        while (true) {
            const { done, value } = await reader.read();

            if (done) {
                if (buffer.startsWith("data: ")) {
                    const cleanText = buffer.substring(6);
                    if (firstChunk) {
                        assistantTextDiv.textContent = "";
                    }
                    assistantTextDiv.textContent += cleanText;
                }
                break;
            }

            buffer += decoder.decode(value, { stream: true });
            const lines = buffer.split("\n");

            buffer = lines.pop() || "";

            for (const line of lines) {
                if (line.startsWith("data: ")) {
                    const cleanText = line.substring(6);

                    if (firstChunk) {
                        assistantTextDiv.textContent = "";
                        firstChunk = false;
                    }

                    assistantTextDiv.textContent += cleanText;
                    chatHistory.scrollTop = chatHistory.scrollHeight;
                }
            }
        }
    } catch (error) {
        assistantTextDiv.textContent = "Error: Failed to fetch response from local server.";
        assistantTextDiv.className = "message-text text-danger";
    } finally {
        inputElement.disabled = false;
        sendBtn.disabled = false;
        inputElement.focus();
    }
}

function escapeHtml(text) {
    return text
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "'");
}