import axios, { AxiosError } from 'axios';

// URL API backend
const API_URL = window.location.hostname === 'localhost' ? 'https://localhost:64732' : '';
const TOKEN_KEY = 'vocabmaster_token';

// Create axios instance with default config
const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 120000, // 120 seconds timeout
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
    // Debug request
    console.log('Starting Request', JSON.stringify(config, null, 2));
    
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
    console.log('Request Error:', error);
    return Promise.reject(error);
  }
);

// Response interceptor
api.interceptors.response.use(
  (response) => {
    // Debug response
    console.log('Response:', JSON.stringify(response.data, null, 2));
    
    // Save token if response contains token (login successful)
    if (response.data && response.data.accessToken) {
      setToken(response.data.accessToken);
    }
    
    return response;
  },
  (error: AxiosError) => {
    // Debug error
    console.log('Response Error:', error);
    
    // Prevent default events that can cause page reload
    if (error.config && error.response) {
      // Log error but don't interrupt application flow
      console.log(`API Error: ${error.response.status} - ${error.response.statusText}`);
      
      // Handle 401 Unauthorized error without page reload
      if (error.response.status === 401) {
        console.log('Authentication error - not redirecting automatically');
        // Check if token is expired
        const isTokenExpired = error.response.headers['token-expired'] === 'true';
        if (isTokenExpired) {
          console.log('Token expired, removing from storage');
          removeToken();
        }
      }
    } else if (error.request) {
      console.log('API Error: No response received');
    } else {
      console.log('API Error:', error.message);
    }
    
    // Return error to allow component to handle
    return Promise.reject(error);
  }
);

export default api; 