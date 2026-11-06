import React, { useState, useEffect, useRef } from 'react';
import chatService from '../Services/chatService';

const ChatWindow = ({ model, temperature }) => {
    const [messages, setMessages] = useState([]);
    const [input, setInput] = useState('');
    const [loading, setLoading] = useState(false);
    const [sessionId] = useState(() => crypto.randomUUID());
    const bottomRef = useRef(null);

    useEffect(() => {
        bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
    }, [messages]);

    const handleSend = async (e) => {
        e.preventDefault();
        if (!input.trim() || loading) return;

        const userMessage = input.trim();
        setInput('');
        setLoading(true);

        setMessages((prev) => [...prev, { role: 'user', content: userMessage }]);
        setMessages((prev) => [...prev, { role: 'assistant', content: '' }]);

        try {
            const url = chatService.streamUrl(userMessage, sessionId, model, temperature);
            const eventSource = new EventSource(url);

            eventSource.onmessage = (event) => {
                setMessages((prev) => {
                    const updated = [...prev];
                    const lastIndex = updated.length - 1;
                    updated[lastIndex] = {
                        ...updated[lastIndex],
                        content: updated[lastIndex].content + event.data,
                    };
                    return updated;
                });
            };

            eventSource.onerror = () => {
                eventSource.close();
                setLoading(false);
            };
        } catch {
            setLoading(false);
        }
    };

    return (
        <div class="window-container">
            <div class="window-scroller">
                {messages.length === 0 ? (
                    <div class="empty-state">
                        <span class="empty-icon">🤖</span>
                        <p class="empty-text">Local inference instance initialized. What shall we create?</p>
                    </div>
                ) : (
                    messages.map((msg, index) => (
                        <div key={index} class={`msg-row ${msg.role}`}>
                            <div class="msg-bubble">
                                {msg.content || (loading && index === messages.length - 1 ? '🔄 Thinking...' : '')}
                            </div>
                        </div>
                    ))
                )}
                <div ref={bottomRef} />
            </div>

            <form onSubmit={handleSend} class="window-footer-form">
                <input type="text" value={input} onChange={(e) => setInput(e.target.value)} placeholder={`Message local engine (${model})...`} disabled={loading} class="form-input-text" />
                <button type="submit" disabled={loading} class="btn-action-primary">Send</button>
            </form>
        </div>
    );
};

export default ChatWindow;
