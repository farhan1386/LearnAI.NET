import apiService from '../../../Common/apiService';

export const chatService = {
    ask: (question, sessionId = null, model = 'phi4', temperature = 0.7, maxTokens = 1000) => {
        const params = { question, sessionId, model, temperature, maxTokens };
        return apiService.get('/chat', { params });
    },

    streamUrl: (question, sessionId = null, model = 'phi4', temperature = 0.7, maxTokens = 1000) => {
        const url = new URL('https://localhost:7154/api/chat/stream');
        url.searchParams.append('question', question);
        if (sessionId) url.searchParams.append('sessionId', sessionId);
        url.searchParams.append('model', model);
        url.searchParams.append('temperature', temperature.toString());
        url.searchParams.append('maxTokens', maxTokens.toString());
        return url.toString();
    }
};

export default chatService;