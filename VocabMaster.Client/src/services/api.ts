import axios, { AxiosError } from 'axios';

const API_URL = process.env.REACT_APP_API_URL || '';
const TOKEN_KEY = 'vocabmaster_token';

// Create axios instance with default config
const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10000, // 10 seconds timeout
});

// Hàm lấy token từ localStorage
const getToken = (): string | null => {
  return localStorage.getItem(TOKEN_KEY);
};

// Hàm lưu token vào localStorage
export const setToken = (token: string): void => {
  localStorage.setItem(TOKEN_KEY, token);
};

// Hàm xóa token khỏi localStorage
export const removeToken = (): void => {
  localStorage.removeItem(TOKEN_KEY);
};

// Request interceptor
api.interceptors.request.use(
  (config) => {
    // Debug request
    console.log('Starting Request', JSON.stringify(config, null, 2));
    
    // Thêm token vào header nếu có
    const token = getToken();
    if (token && config.headers) {
      config.headers['Authorization'] = `Bearer ${token}`;
    }
    
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
    
    // Lưu token nếu response chứa token (đăng nhập thành công)
    if (response.data && response.data.accessToken) {
      setToken(response.data.accessToken);
    }
    
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
        // Kiểm tra nếu token hết hạn
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
    
    // Trả về lỗi để component có thể xử lý
    return Promise.reject(error);
  }
);

export default api; 