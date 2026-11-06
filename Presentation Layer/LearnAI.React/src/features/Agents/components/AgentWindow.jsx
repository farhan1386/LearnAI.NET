import React, { useState } from 'react';
import agentService from '../Services/agentService';

const AgentWindow = ({ engine }) => {
    const [goalInput, setGoalInput] = useState('');
    const [loading, setLoading] = useState(false);
    const [logs, setLogs] = useState([]);
    const [currentState, setCurrentState] = useState(null);

    const [mockNodes] = useState([
        { name: 'Research Node', capabilities: 'Web Extraction, Analysis', active: true },
        { name: 'Refactoring Node', capabilities: 'AST Optimization, Compiles', active: false },
        { name: 'Validation Node', capabilities: 'Unit Verification, Mocking', active: false }
    ]);

    const handleRunGoal = async (e) => {
        e.preventDefault();
        if (!goalInput.trim() || loading) return;

        const currentGoal = goalInput.trim();
        setGoalInput('');
        setLoading(true);
        setCurrentState({ status: 'Active', stepsCompleted: 0, activeAgentNode: 'Research Node' });
        setLogs((prev) => [...prev, `[System]: Dispatched goal directive via ${engine} -> "${currentGoal}"`]);

        try {
            const data = await agentService.executeGoal(currentGoal);

            const normalizedData = {
                status: data?.status ?? data?.Status ?? 'Completed',
                stepsCompleted: data?.stepsCompleted ?? data?.StepsCompleted ?? 0,
                activeAgentNode: data?.activeAgentNode ?? data?.ActiveAgentNode ?? 'None',
                result: data?.result ?? data?.Result ?? data?.output ?? data?.Output ?? 'Execution loop finished successfully.'
            };

            setCurrentState(normalizedData);
            setLogs((prev) => [...prev, `[Orchestrator]: Pipeline complete. Status: ${normalizedData.status}\nOutput: ${normalizedData.result}`]);
        } catch (error) {
            const errMessage = error.message || (error.originalError?.response?.data) || 'Server communication breakdown';
            setLogs((prev) => [...prev, `[Error]: Process iteration failed -> ${errMessage}`]);
            setCurrentState({ status: 'Failed', stepsCompleted: 0, activeAgentNode: 'None' });
        } finally {
            setLoading(false);
        }
    };

    const tasks = currentState?.executionPlan?.tasks ?? currentState?.ExecutionPlan?.Tasks ?? [];
    const steps = currentState?.steps ?? currentState?.Steps ?? [];
    const isComplete = currentState?.isComplete ?? currentState?.IsComplete ?? false;
    const finalAnswer = currentState?.finalAnswer ?? currentState?.FinalAnswer ?? '';

    return (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '24px', flex: 1, minHeight: 0 }}>
            {/* Search Input Bar */}
            <div class="output-section-card">
                <form onSubmit={handleRunGoal} style={{ display: 'flex', gap: '12px' }}>
                    <input
                        type="text"
                        value={goalInput}
                        onChange={(e) => setGoalInput(e.target.value)}
                        placeholder="Enter a complex goal (e.g., Write a compliance report on solar data)..."
                        disabled={loading}
                        class="form-input-text"
                    />
                    <button type="submit" disabled={loading} class="btn-action-primary">
                        {loading ? 'Running Agent...' : 'Run Agent'}
                    </button>
                </form>
            </div>

            {/* Real-time Loader Logs */}
            {loading && (
                <div class="terminal-console-area" style={{ color: 'var(--success-color)' }}>
                    {logs.map((log, i) => <div key={i}>{log}</div>)}
                    <div style={{ color: 'var(--text-muted)', animation: 'pulse 1.5s infinite' }}>Processing orchestration loops...</div>
                </div>
            )}

            {/* Grid Content Panel Panels */}
            {tasks.length > 0 && (
                <div style={{ display: 'grid', gridTemplateColumns: '5fr 7fr', gap: '24px', flex: 1, minHeight: 0 }}>
                    {/* Left Column: Strategic Execution Roadmap */}
                    <div class="window-container">
                        <div class="page-header" style={{ padding: '14px 20px' }}>
                            <h5 class="page-title" style={{ fontSize: '0.95rem' }}>Strategic Execution Roadmap</h5>
                        </div>
                        <div class="window-scroller" style={{ padding: '20px' }}>
                            {tasks.map((task, index) => {
                                const isTaskDone = task.isCompleted ?? task.IsCompleted ?? false;
                                const targetAgent = task.targetAgent ?? task.TargetAgent ?? '';
                                const description = task.description ?? task.Description ?? '';
                                const summary = task.outputSummary ?? task.OutputSummary ?? '';

                                return (
                                    <div key={index} class="metric-node-row" style={{ backgroundColor: isTaskDone ? 'rgba(16, 185, 129, 0.05)' : 'var(--bg-input)', borderColor: isTaskDone ? 'var(--success-color)' : 'var(--border-color)' }}>
                                        <div class="metric-node-header">
                                            <span class="metric-node-title" style={{ fontSize: '0.9rem' }}>{targetAgent}</span>
                                            <span class={`status-badge ${isTaskDone ? 'success' : 'failed'}`} style={{ color: isTaskDone ? 'var(--success-color)' : 'var(--text-muted)' }}>
                                                {isTaskDone ? 'Done' : 'Pending'}
                                            </span>
                                        </div>
                                        <div style={{ fontSize: '0.85rem', color: 'var(--text-muted)', marginTop: '4px' }}>{description}</div>
                                        {summary && (
                                            <div class="terminal-console-area" style={{ marginTop: '8px', padding: '8px', fontSize: '0.75rem', maxHeight: '150px' }}>{summary}</div>
                                        )}
                                    </div>
                                );
                            })}
                        </div>
                    </div>

                    {/* Right Column: Execution Traces & Reasoning Monologue */}
                    <div class="window-container">
                        <div class="page-header" style={{ padding: '14px 20px', backgroundColor: 'var(--bg-user-msg)' }}>
                            <h5 class="page-title" style={{ fontSize: '0.95rem' }}>Execution Traces & Reasoning Monologue</h5>
                        </div>
                        <div class="window-scroller" style={{ padding: '20px', backgroundColor: 'var(--bg-input)' }}>
                            {steps.map((step, index) => {
                                const timestamp = step.timestamp ?? step.Timestamp ?? '';
                                const stepId = step.id ?? step.Id ?? '';
                                const thought = step.thought ?? step.Thought ?? '';
                                const action = step.action ?? step.Action ?? '';
                                const observation = step.observation ?? step.Observation ?? '';

                                return (
                                    <div key={index} style={{ borderBottom: '1px solid var(--border-color)', paddingBottom: '12px', marginBottom: '12px' }}>
                                        <div style={{ color: 'var(--text-dark)', fontSize: '0.75rem' }}>[{timestamp}] Step ID: {stepId}</div>
                                        <div style={{ color: 'var(--accent-color)', fontWeight: '700', marginTop: '6px' }}>🤔 Thought:</div>
                                        <div style={{ color: 'var(--text-muted)', paddingLeft: '12px', margin: '2px 0 6px 0', fontFamily: 'monospace' }}>{thought}</div>
                                        <div style={{ color: 'var(--error-color)', fontWeight: '700' }}>🚀 Action:</div>
                                        <div style={{ color: 'var(--text-main)', paddingLeft: '12px', margin: '2px 0 6px 0', fontFamily: 'monospace' }}><strong>{action}</strong></div>
                                        <div style={{ color: 'var(--success-color)', fontWeight: '700' }}>👁️ Observation:</div>
                                        <div style={{ color: 'var(--success-color)', paddingLeft: '12px', marginTop: '2px', fontFamily: 'monospace' }}>{observation}</div>
                                    </div>
                                );
                            })}
                        </div>
                    </div>
                </div>
            )}

            {/* Fallback Empty Placeholder Box */}
            {!loading && tasks.length === 0 && (
                <div class="window-container" style={{ minHeight: '300px' }}>
                    <div class="empty-state">
                        <span class="empty-icon">⚡</span>
                        <p class="empty-text">Enter an autonomous execution goal target directive to deploy worker node clusters.</p>
                    </div>
                </div>
            )}

            {/* Bottom Block: Final Answer Output Artifact */}
            {isComplete && finalAnswer && (
                <div class="output-section-card bordered-left" style={{ borderLeftColor: 'var(--success-color)' }}>
                    <h5 class="section-title" style={{ color: 'var(--success-color)' }}>🏁 Final Answer Output Artifact</h5>
                    <div style={{ color: 'var(--text-main)', fontSize: '1rem', lineHeight: '1.6', whiteSpace: 'pre-line' }}>{finalAnswer}</div>
                </div>
            )}
        </div>
    );
};

export default AgentWindow;
