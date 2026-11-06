async function runBenchmark(event) {
    event.preventDefault();

    const promptInput = document.getElementById("benchmarkPrompt");
    const runButton = document.getElementById("runBtn");
    const loader = document.getElementById("loaderState");
    const grid = document.getElementById("metricsGrid");

    const targetPrompt = promptInput.value.trim();
    if (!targetPrompt) return;

    promptInput.disabled = true;
    runButton.disabled = true;
    grid.classList.add("d-none");
    loader.classList.remove("d-none");
    grid.innerHTML = "";

    try {
        const response = await fetch(`/Gateway/Compare?prompt=${encodeURIComponent(targetPrompt)}`);
        const result = await response.json();

        loader.classList.add("d-none");

        if (result.success && result.metrics) {
            grid.classList.remove("d-none");
            result.metrics.forEach(metric => {
                grid.appendChild(createMetricCard(metric));
            });
        } else {
            alert(result.message || "Failed to process comparative analytics.");
        }
    } catch (err) {
        loader.classList.add("d-none");
        alert("Error communicating with gateway benchmark endpoint.");
    } finally {
        promptInput.disabled = false;
        runButton.disabled = false;
        promptInput.focus();
    }
}

function createMetricCard(metric) {
    const col = document.createElement("div");
    col.className = "col";

    const providerNames = { 1: "Ollama (Local)", 2: "OpenAI", 3: "Azure OpenAI", 4: "Claude", 5: "Gemini" };
    const providerName = providerNames[metric.provider] || "Unknown Backend";

    const statusBadge = metric.isSuccess
        ? '<span class="badge bg-success-subtle text-success border border-success-subtle px-2 py-1">ONLINE</span>'
        : '<span class="badge bg-danger-subtle text-danger border border-danger-subtle px-2 py-1">FAILED</span>';

    let ms = 0;
    if (metric.latency) {
        const parts = metric.latency.split(/[:.]/);
        if (parts.length >= 4) {
            const hourMs = parseInt(parts[0], 10) * 3600000;
            const minMs = parseInt(parts[1], 10) * 600000;
            const secMs = parseInt(parts[2], 10) * 1000;
            const fractionStr = parts[3].substring(0, 3).padEnd(3, '0');
            const finalMs = parseInt(fractionStr, 10);

            ms = hourMs + minMs + secMs + finalMs;
        }
    }

    const speedValue = (metric.isSuccess && ms > 0 && metric.tokenCount > 0)
        ? Math.round((metric.tokenCount / (ms / 1000)) * 10) / 10
        : 0;

    const bodyContent = metric.isSuccess
        ? `<div class="mb-3">
                <label class="small text-muted fw-bold d-block text-uppercase mb-1">Response Latency</label>
                <span class="fs-3 fw-bold text-dark">${(ms / 1000).toFixed(2)}s</span>
           </div>
           <div class="row g-0 pt-2 border-top border-light-subtle">
                <div class="col-6">
                    <label class="small text-muted d-block mb-0">Tokens Caught</label>
                    <span class="fw-bold text-secondary">${metric.tokenCount} tokens</span>
                </div>
                <div class="col-6 border-start ps-3">
                    <label class="small text-muted d-block mb-0">Generation Speed</label>
                    <span class="fw-bold text-primary">${speedValue} t/s</span>
                </div>
           </div>`
        : `<div class="alert alert-danger-subtle border-danger-subtle text-danger p-2 small mb-0 overflow-auto" style="max-height: 120px;">
                <strong>Error Diagnostic:</strong><br/>${metric.errorMessage || "Operation timed out without throwing descriptive metadata."}
           </div>`;

    col.innerHTML = `
        <div class="card h-100 shadow-sm border-0 border-top border-3 ${metric.isSuccess ? 'border-primary' : 'border-danger'}">
            <div class="card-body p-4 d-flex flex-column justify-content-between">
                <div>
                    <div class="d-flex justify-content-between align-items-center mb-2">
                        <h5 class="fw-bold mb-0 text-dark">${providerName}</h5>
                        ${statusBadge}
                    </div>
                    <h6 class="text-muted small mb-4">Model: ${metric.modelUsed || "Default"}</h6>
                </div>
                <div>
                    ${bodyContent}
                </div>
            </div>
        </div>
    `;
    return col;
}

async function runSingleExecution(event) {
    event.preventDefault();

    const providerSelect = document.getElementById("targetProvider");
    const modelInput = document.getElementById("customModelName");
    const promptInput = document.getElementById("singlePrompt");
    const runButton = document.getElementById("singleRunBtn");
    const outputContainer = document.getElementById("singleResponseLayout");
    const outputArea = document.getElementById("rawOutputArea");

    const providerValue = parseInt(providerSelect.value, 10);
    const modelValue = modelInput.value.trim();
    const promptValue = promptInput.value.trim();

    if (!promptValue) return;

    providerSelect.disabled = true;
    modelInput.disabled = true;
    promptInput.disabled = true;
    runButton.disabled = true;
    outputContainer.classList.add("d-none");
    outputArea.textContent = "Processing request stream...";

    try {
        const response = await fetch('/Gateway/Execute', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                provider: providerValue,
                modelName: modelValue || null,
                prompt: promptValue
            })
        });

        const result = await response.json();
        outputContainer.classList.remove("d-none");

        if (result.success) {
            outputArea.textContent = result.data || "Empty context returned from host.";
        } else {
            outputArea.textContent = `Error Status: ${result.message || "Execution exception error occurred."}`;
        }
    } catch (err) {
        outputContainer.classList.remove("d-none");
        outputArea.textContent = "Communication failure connecting to runtime execution gateway engine controller pipelines.";
    } finally {
        providerSelect.disabled = false;
        modelInput.disabled = false;
        promptInput.disabled = false;
        runButton.disabled = false;
        promptInput.value = "";
        promptInput.focus();
    }
}
