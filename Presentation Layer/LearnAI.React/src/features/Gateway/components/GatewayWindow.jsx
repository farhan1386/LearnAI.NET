import React, { useState } from 'react';
import gatewayService from '../Services/gatewayService';

const GatewayWindow = () => {
    const [activeTab, setActiveTab] = useState('benchmark');

    const [benchmarkPrompt, setBenchmarkPrompt] = useState('');
    const [modelNameOverride, setModelNameOverride] = useState('');
    const [benchmarkLoading, setBenchmarkLoading] = useState(false);
    const [comparisonReport, setComparisonReport] = useState(null);

    const [providerId, setProviderId] = useState('1');
    const [modelOverride, setModelOverride] = useState('');
    const [singlePrompt, setSinglePrompt] = useState('');
    const [singleLoading, setSingleLoading] = useState(false);
    const [singleResponse, setSingleResponse] = useState('');

    const handleBenchmarkSubmit = async (e) => {
        e.preventDefault();
        if (!benchmarkPrompt.trim() || benchmarkLoading) return;

        setBenchmarkLoading(true);
        try {
            const data = await gatewayService.compareBackends(benchmarkPrompt.trim(), modelNameOverride.trim() || null);
            setComparisonReport(data);
        } catch (error) {
            console.error(error);
        } finally {
            setBenchmarkLoading(false);
        }
    };

    const handleSingleSubmit = async (e) => {
        e.preventDefault();
        if (!singlePrompt.trim() || singleLoading) return;

        setSingleLoading(true);
        setSingleResponse('');
        try {
            const data = await gatewayService.switchAndExecute(providerId, modelOverride, singlePrompt);
            const cleanResult = data?.Response ?? data?.response ?? data;
            setSingleResponse(typeof cleanResult === 'object' ? JSON.stringify(cleanResult, null, 2) : cleanResult);
        } catch (error) {
            console.error(error);
            const originalErrResponse = error.originalError?.response?.data;
            const serverErr = originalErrResponse?.Error ?? originalErrResponse?.error ?? error.message;
            setSingleResponse(`[Error Context Details]: ${serverErr}`);
        } finally {
            setSingleLoading(false);
        }
    };

    const providers = comparisonReport ?? [];

    return (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '24px', flex: 1 }}>
            <div class="window-container">
                <div class="tabs-navigation">
                    <button onClick={() => setActiveTab('benchmark')} class={`tab-btn ${activeTab === 'benchmark' ? 'active' : ''}`}>Cross-Provider Benchmarking</button>
                    <button onClick={() => setActiveTab('single')} class={`tab-btn ${activeTab === 'single' ? 'active' : ''}`}>Single Provider Testing</button>
                </div>

                <div>
                    {activeTab === 'benchmark' ? (
                        <div class="gateway-form-panel">
                            <p style={{ margin: 0, fontSize: '0.9rem', color: 'var(--text-muted)' }}>Broadcast a single uniform test prompt simultaneously across configurations to evaluate core performance metrics.</p>
                            <form onSubmit={handleBenchmarkSubmit} style={{ display: 'flex', gap: '14px' }}>
                                <input type="text" value={benchmarkPrompt} onChange={(e) => setBenchmarkPrompt(e.target.value)} placeholder="Enter a structural benchmark prompt..." disabled={benchmarkLoading} class="form-input-text" />
                                <input type="text" value={modelNameOverride} onChange={(e) => setModelNameOverride(e.target.value)} placeholder="Global model override..." disabled={benchmarkLoading} class="form-input-text" style={{ width: '240px' }} />
                                <button type="submit" disabled={benchmarkLoading} class="btn-action-primary">
                                    {benchmarkLoading ? 'Evaluating...' : 'Execute'}
                                </button>
                            </form>
                        </div>
                    ) : (
                        <div class="gateway-form-panel">
                            <p style={{ margin: 0, fontSize: '0.9rem', color: 'var(--text-muted)' }}>Target a specific provider infrastructure directly to test conversational capabilities.</p>
                            <form onSubmit={handleSingleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
                                <div class="gateway-grid-row">
                                    <select value={providerId} onChange={(e) => setProviderId(e.target.value)} class="form-input-text" style={{ cursor: 'pointer' }}>
                                        <option value="1">Ollama (Local Engine)</option>
                                        <option value="2">OpenAI Cloud</option>
                                        <option value="3">Azure OpenAI Platform</option>
                                        <option value="4">Anthropic Claude Proxy</option>
                                        <option value="5">Google Gemini</option>
                                    </select>
                                    <input type="text" value={modelOverride} onChange={(e) => setModelOverride(e.target.value)} placeholder="Override model (Optional fallback)..." class="form-input-text" />
                                </div>
                                <div style={{ display: 'flex', gap: '14px' }}>
                                    <input type="text" value={singlePrompt} onChange={(e) => setSinglePrompt(e.target.value)} placeholder="Type your instruction or message for the selected engine..." disabled={singleLoading} class="form-input-text" />
                                    <button type="submit" disabled={singleLoading} class="btn-action-secondary">Send</button>
                                </div>
                            </form>
                        </div>
                    )}
                </div>
            </div>

            {activeTab === 'benchmark' && providers.length > 0 && (
                <div class="output-section-card">
                    <h3 class="section-title">Cross-Evaluation Matrix</h3>
                    <div class="metrics-stack">
                        {providers.map((p, idx) => {
                            const providerNum = p.provider ?? p.Provider ?? 0;
                            const modelUsed = p.modelUsed ?? p.ModelUsed ?? 'Default';
                            const success = p.isSuccess ?? p.IsSuccess ?? false;
                            const errorMessage = p.errorMessage ?? p.ErrorMessage ?? '';
                            const latency = p.latency ?? p.Latency ?? '00:00:00';

                            const providerNames = { 1: 'Ollama', 2: 'OpenAI', 3: 'AzureOpenAI', 4: 'Claude', 5: 'Gemini' };
                            const name = providerNames[providerNum] || `Provider #${providerNum}`;

                            return (
                                <div key={idx} class="metric-node-row">
                                    <div class="metric-node-header">
                                        <span class="metric-node-title">{name} <span class="metric-node-meta">({modelUsed})</span></span>
                                        <span class={`status-badge ${success ? 'success' : 'failed'}`}>
                                            {success ? `Success (${latency})` : 'Failed'}
                                        </span>
                                    </div>
                                    {!success && errorMessage && (
                                        <div class="error-tracebox">Error trace: {errorMessage}</div>
                                    )}
                                </div>
                            );
                        })}
                    </div>
                </div>
            )}

            {activeTab === 'single' && singleResponse && (
                <div class="output-section-card bordered-left">
                    <h5 class="section-title">Engine Execution Output</h5>
                    <div class="terminal-console-area">{singleResponse}</div>
                </div>
            )}
        </div>
    );
};

export default GatewayWindow;
