import axios from 'axios';

const api = axios.create({
    baseURL: 'https://localhost:7154/api',
    timeout: 10000,
    headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
    },
});

api.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('token');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

api.interceptors.response.use(
    (response) => response.data,
    (error) => {
        const customError = {
            message: error.response?.data?.message || 'An unexpected error occurred.',
            status: error.response?.status || 500,
            originalError: error,
        };

        if (customError.status === 401) {
            console.warn('Unauthorized access.');
        }

        return Promise.reject(customError);
    }
);

const apiService = {
    get: (url, config = {}) => api.get(url, config),
    post: (url, data, config = {}) => api.post(url, data, config),
    put: (url, data, config = {}) => api.put(url, data, config),
    delete: (url, config = {}) => api.delete(url, config),
};

export default apiService;
