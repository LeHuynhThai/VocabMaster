import axios from 'axios';

const API_URL = process.env.REACT_APP_API_URL || '';

// Create axios instance with default config
const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true, // Important for cookies/auth
  timeout: 10000, // 10 seconds timeout
});

// Request interceptor
api.interceptors.request.use(
  (config) => {
    // You can add auth token here if needed
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor
api.interceptors.response.use(
  (response) => response,
  (error) => {
    // Không tự động chuyển hướng khi 401 để tránh vòng lặp vô tận
    // Chỉ log lỗi và để component xử lý
    if (error.response) {
      console.log(`API Error: ${error.response.status} - ${error.response.statusText}`);
    } else if (error.request) {
      console.log('API Error: No response received');
    } else {
      console.log('API Error:', error.message);
    }
    
    return Promise.reject(error);
  }
);

export default api; 