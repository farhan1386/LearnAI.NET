import { Routes, Route, Navigate } from "react-router-dom";
import chatRoutes from "../../src/features/Chat/routes";
import agentRoutes from "../../src/features/Agents/routes";
import gatewayRoutes from "../../src/features/Gateway/routes";
import kernelRoutes from "../../src/features/Kernel/routes";
import mcpRoutes from "../../src/features/MCP/routes";
import ragRoutes from "../../src/features/RAG/routes";
import toolsRoutes from "../../src/features/Tools/routes";

const AppRoutes = () => {
    const safeRoutes = (routes) => Array.isArray(routes) ? routes : [];

    const appRoutes = [
        ...safeRoutes(chatRoutes),
        ...safeRoutes(agentRoutes),
        ...safeRoutes(gatewayRoutes),
        ...safeRoutes(kernelRoutes),
        ...safeRoutes(mcpRoutes),
        ...safeRoutes(ragRoutes),
        ...safeRoutes(toolsRoutes)
    ];

    return (
        <Routes>
            <Route path="/" element={<Navigate to="/chat" replace />} />
            {appRoutes.map((route, index) => (
                <Route key={index} path={route.path} element={route.element} />
            ))}
            <Route path="*" element={<Navigate to="/chat" replace />} />
        </Routes>
    );
};

export default AppRoutes;
