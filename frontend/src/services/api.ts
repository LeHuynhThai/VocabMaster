import axios, { AxiosError } from 'axios';
import { logger } from '../utils/logger';

// URL API backend
const API_URL = process.env.REACT_APP_API_URL || (window.location.hostname === 'localhost' ? 'https://localhost:64732' : '');
const TOKEN_KEY = 'vocabmaster_token';

// Create axios instance with default config
const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 15000, // 15 seconds timeout
  withCredentials: true, // Allow sending cookies when cross-domain request
});

// Function to get token from localStorage
const getToken = (): string | null => {
  return localStorage.getItem(TOKEN_KEY);
};

// Function to save token to localStorage
export const setToken = (token: string): void => {
  localStorage.setItem(TOKEN_KEY, token);
};

// Function to remove token from localStorage
export const removeToken = (): void => {
  localStorage.removeItem(TOKEN_KEY);
};

// Request interceptor
api.interceptors.request.use(
  (config) => {
    // Debug request (sanitized)
    logger.debug({ type: 'request', method: config.method, url: config.url, params: config.params, data: config.data });
    
    // Add token to header if exists
    const token = getToken();
    if (token && config.headers) {
      config.headers['Authorization'] = `Bearer ${token}`;
    }
    
    // Prevent default events that can cause page reload
    if (config.method?.toLowerCase() === 'post' || config.method?.toLowerCase() === 'put') {
      if (config.headers) {
        config.headers['X-Requested-With'] = 'XMLHttpRequest';
      }
    }
    return config;
  },
  (error) => {
    logger.error('Request Error', error);
    return Promise.reject(error);
  }
);

// Response interceptor
api.interceptors.response.use(
  (response) => {
    // Debug response (sanitized)
    logger.debug({ type: 'response', url: response.config?.url, status: response.status, data: response.data });
    
    // Save token if response contains token (login successful)
    if (response.data && response.data.accessToken) {
      setToken(response.data.accessToken);
    }
    
    return response;
  },
  (error: AxiosError) => {
    // Debug error (sanitized)
    logger.error('Response Error', error);

    // Prevent default events that can cause page reload
    if (error.config && error.response) {
      // Log minimal info
      logger.warn(`API Error: ${error.response.status} - ${error.response.statusText}`);

      // Handle 401 Unauthorized error without page reload
      if (error.response.status === 401) {
        logger.warn('Authentication error - not redirecting automatically');
        // Check if token is expired
        const isTokenExpired = error.response.headers['token-expired'] === 'true';
        if (isTokenExpired) {
          logger.warn('Token expired, removing from storage');
          removeToken();
        }
      }
    } else if (error.request) {
      logger.warn('API Error: No response received');
    } else {
      logger.error('API Error', error.message);
    }
    
    // Return error to allow component to handle
    return Promise.reject(error);
  }
);

export default api; 