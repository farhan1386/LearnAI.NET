import apiService from '../../../Common/apiService';

export const agentService = {
    executeGoal: (goal) => {
        return apiService.post('/agents/execute', JSON.stringify(goal), {
            headers: {
                'Content-Type': 'application/json'
            }
        });
    }
};

export default agentService;