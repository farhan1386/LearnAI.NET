import React from 'react';
import GatewayWindow from '../components/GatewayWindow';

const GatewayPage = () => {
    return (
        <div class="page-container">
            <header class="page-header">
                <div>
                    <h1 class="page-title">AI Gateway Matrix</h1>
                    <p class="page-subtitle">Proxy, route, and benchmark multi-provider LLM infrastructure</p>
                </div>
            </header>
            <main class="main-content">
                <GatewayWindow />
            </main>
        </div>
    );
};

export default GatewayPage;
