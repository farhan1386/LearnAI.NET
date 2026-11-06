import React, { useState } from 'react';
import ChatWindow from '../components/ChatWindow';

const ChatPage = () => {
    const [selectedModel, setSelectedModel] = useState('phi4:latest');
    const [temperature, setTemperature] = useState(0.7);

    return (
        <div class="page-container">
            <header class="page-header">
                <div>
                    <h1 class="page-title">Ollama Workspace</h1>
                    <p class="page-subtitle">Connected to local inference engine</p>
                </div>
                <div class="header-controls">
                    <div class="control-group">
                        <label class="control-label">Model</label>
                        <select value={selectedModel} onChange={(e) => setSelectedModel(e.target.value)} class="control-select">
                            <option value="phi4:latest">phi4:latest</option>
                            <option value="phi3:latest">phi3:latest</option>
                            <option value="llama3.2:latest">llama3.2:latest</option>
                            <option value="llama3.2:1b">llama3.2:1b</option>
                            <option value="all-minilm:latest">all-minilm:latest</option>
                        </select>
                    </div>
                    <div class="control-group">
                        <label class="control-label">Creativity ({temperature})</label>
                        <input type="range" min="0" max="1" step="0.1" value={temperature} onChange={(e) => setTemperature(parseFloat(e.target.value))} class="control-slider" />
                    </div>
                </div>
            </header>
            <main class="main-content">
                <ChatWindow model={selectedModel} temperature={temperature} />
            </main>
        </div>
    );
};

export default ChatPage;