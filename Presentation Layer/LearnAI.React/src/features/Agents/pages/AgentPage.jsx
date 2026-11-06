import React, { useState } from 'react';
import AgentWindow from '../components/AgentWindow';

const AgentPage = () => {
    const [selectedEngine, setSelectedEngine] = useState('SemanticKernel');

    return (
        <div class="page-container">
            <header class="page-header">
                <div>
                    <h1 class="page-title">Agent Multi-Mesh Core</h1>
                    <p class="page-subtitle">Orchestrate autonomous agent executors</p>
                </div>
                <div class="header-controls">
                    <div class="control-group">
                        <label class="control-label">Orchestration Infrastructure</label>
                        <select value={selectedEngine} onChange={(e) => setSelectedEngine(e.target.value)} class="control-select">
                            <option value="SemanticKernel">Semantic Kernel</option>
                            <option value="McpMesh">MCP Infrastructure</option>
                        </select>
                    </div>
                </div>
            </header>
            <main class="main-content">
                <AgentWindow engine={selectedEngine} />
            </main>
        </div>
    );
};

export default AgentPage;
