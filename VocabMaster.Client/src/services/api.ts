import axios, { AxiosError } from 'axios';

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
    // Debug request
    console.log('Starting Request', JSON.stringify(config, null, 2));
    
    // Ngăn chặn các sự kiện mặc định có thể gây reload trang
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
    return response;
  },
  (error: AxiosError) => {
    // Debug error
    console.log('Response Error:', error);
    
    // Ngăn chặn các hành vi mặc định có thể gây reload trang
    if (error.config && error.response) {
      // Ghi log lỗi nhưng không làm gián đoạn luồng ứng dụng
      console.log(`API Error: ${error.response.status} - ${error.response.statusText}`);
      
      // Xử lý lỗi 401 Unauthorized mà không reload trang
      if (error.response.status === 401) {
        console.log('Authentication error - not redirecting automatically');
        // Không tự động chuyển hướng, để component xử lý
      }
    } else if (error.request) {
      console.log('API Error: No response received');
    } else {
      console.log('API Error:', error.message);
    }
    
    // Trả về lỗi để component có thể xử lý
    return Promise.reject(error);
  }
);

export default api; 