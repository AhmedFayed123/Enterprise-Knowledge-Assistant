import axios from 'axios';
import { getToken, clearAuth } from '../context/authStorage';

const baseURL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5274/api';

const api = axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json'
  }
});

api.interceptors.request.use((config) => {
  const token = getToken();
  if (token && config.headers) {
    config.headers['Authorization'] = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  r => r,
  (error) => {
    if (error.response && error.response.status === 401) {
      clearAuth();
      // redirect handled by auth context
    }
    return Promise.reject(error);
  }
);

export default api;
