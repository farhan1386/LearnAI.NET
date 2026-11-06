import apiService from '../../../Common/apiService';

export const gatewayService = {
    switchAndExecute: (providerId, modelOverride, prompt) => {
        const cleanedModel = modelOverride && modelOverride.trim() !== '' ? modelOverride.trim() : null;

        return apiService.post('/gateway/switch', {
            Provider: Number(providerId), // Binds directly to ProviderType enum integers
            ModelName: cleanedModel,      // PascalCase property name from backend class
            Prompt: prompt
        });
    },
    compareBackends: (prompt, modelName = null) => {
        const params = { prompt };
        if (modelName && modelName.trim() !== '') params.modelName = modelName.trim();
        return apiService.get('/gateway/compare', { params });
    }
};

export default gatewayService;
