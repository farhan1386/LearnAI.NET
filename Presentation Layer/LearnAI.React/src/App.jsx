import { NavLink } from 'react-router-dom';
import AppRoutes from '../app/routes/AppRoutes.jsx';
import viteLogo from './assets/vite.svg';

function App() {
    return (
        <div className="app-container">
            <aside className="sidebar">
                <div className="sidebar-header">
                    <img src={viteLogo} alt="Logo" style={{ height: '16px', width: 'auto' }} />
                    <span className="logo-text">LearnAI <span className="logo-version">v2</span></span>
                </div>

                <nav className="sidebar-nav">
                    <NavLink to="/chat" className="nav-link-node">
                        <svg viewBox="0 0 24 24" style={{ width: '16px', height: '16px' }}><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"></path></svg>
                        Chat Workspace
                    </NavLink>

                    <NavLink to="/agent" className="nav-link-node">
                        <svg viewBox="0 0 24 24" style={{ width: '16px', height: '16px' }}><polygon points="13 2 3 14 12 14 11 22 21 10 12 10 13 2"></polygon></svg>
                        Agent Multi-Mesh
                    </NavLink>

                    <NavLink to="/gateway" className="nav-link-node">
                        <svg viewBox="0 0 24 24" style={{ width: '16px', height: '16px' }}><rect x="2" y="2" width="20" height="8" rx="2" ry="2"></rect><rect x="2" y="14" width="20" height="8" rx="2" ry="2"></rect><line x1="6" y1="6" x2="6.01" y2="6"></line><line x1="6" y1="18" x2="6.01" y2="18"></line></svg>
                        Gateway Matrix
                    </NavLink>

                    <NavLink to="/kernel" className="nav-link-node">
                        <svg viewBox="0 0 24 24" style={{ width: '16px', height: '16px' }}><path d="M12 22c5.523 0 10-4.477 10-10S17.523 2 12 2 2 6.477 2 12s4.477 10 10 10z"></path><path d="M12 6v6l4 2"></path></svg>
                        Semantic Kernel
                    </NavLink>

                    <NavLink to="/mcp" className="nav-link-node">
                        <svg viewBox="0 0 24 24" style={{ width: '16px', height: '16px' }}><path d="M18.36 6.64a9 9 0 1 1-12.73 0"></path><line x1="12" y1="2" x2="12" y2="12"></line></svg>
                        MCP Protocol
                    </NavLink>

                    <NavLink to="/rag" className="nav-link-node">
                        <svg viewBox="0 0 24 24" style={{ width: '16px', height: '16px' }}><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"></path><path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"></path></svg>
                        RAG Context
                    </NavLink>

                    <NavLink to="/tools" className="nav-link-node">
                        <svg viewBox="0 0 24 24" style={{ width: '16px', height: '16px' }}><path d="M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z"></path></svg>
                        Tools Registry
                    </NavLink>
                </nav>
            </aside>
            <div className="content-window">
                <AppRoutes />
            </div>
        </div>
    );
}

export default App;
