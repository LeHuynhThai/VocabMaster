import api, { removeToken } from './api';
import { LoginRequest, RegisterRequest, User, TokenResponse, GoogleAuthRequest } from '../types';

const getErrorMessage = (error: any, fallback: string): string => {
  const serverMessage = error?.response?.data?.message || error?.response?.data?.Message;
  const modelStateErrors = error?.response?.data?.errors;
  let firstModelError = '';

  if (modelStateErrors && typeof modelStateErrors === 'object') {
    const firstKey = Object.keys(modelStateErrors)[0];
    const arr = (firstKey && modelStateErrors[firstKey]) || [];
    if (Array.isArray(arr) && arr.length > 0) firstModelError = arr[0];
  }

  return serverMessage || firstModelError || fallback;
};

const authService = {
  login: async (credentials: LoginRequest): Promise<User> => {
    try {
      const response = await api.post<TokenResponse>('/api/account/login', credentials);
      return {
        id: response.data.userId,
        name: response.data.userName,
        role: response.data.role
      };
    } catch (error: any) {
      const message = getErrorMessage(error, 'Đăng nhập thất bại. Vui lòng kiểm tra thông tin đăng nhập.');
      throw new Error(message);
    }
  },

  googleLogin: async (googleAuth: GoogleAuthRequest): Promise<User> => {
    try {
      const payload = {
        accessToken: googleAuth.accessToken,
        idToken: googleAuth.idToken || 'dummy_token'
      };
      const response = await api.post<TokenResponse>('/api/account/google-login', payload, {
        headers: { 'Content-Type': 'application/json' }
      });
      return {
        id: response.data.userId,
        name: response.data.userName,
        role: response.data.role
      };
    } catch (error: any) {
      const message = getErrorMessage(error, 'Đăng nhập Google thất bại. Vui lòng thử lại.');
      throw new Error(message);
    }
  },

  register: async (userData: RegisterRequest): Promise<boolean> => {
    try {
      const response = await api.post('/api/account/register', userData);
      return response.status === 200;
    } catch (error: any) {
      let friendly = getErrorMessage(error, 'Đăng ký thất bại. Vui lòng kiểm tra lại thông tin.');
      if (/Username already exists/i.test(error?.response?.data?.message || '')) {
        friendly = 'Tên đăng nhập đã tồn tại';
      }
      throw new Error(friendly);
    }
  },

  logout: async (): Promise<void> => {
    try {
      removeToken();
    } catch (error: any) {
      throw error;
    }
  },

  getCurrentUser: async (): Promise<User | null> => {
    try {
      const response = await api.get('/api/account/currentuser');
      return response.data;
    } catch (error: any) {
      if (error.response?.status !== 401) {
        console.error('Get current user API error:', error.response?.data || error.message);
      }
      return null;
    }
  },

  refreshToken: async (): Promise<boolean> => {
    try {
      await api.get<TokenResponse>('/api/account/refresh-token');
      return true;
    } catch (error: any) {
      console.error('Refresh token error:', error.response?.data || error.message);
      return false;
    }
  },

  isAuthenticated: async (): Promise<boolean> => {
    const user = await authService.getCurrentUser();
    return user !== null;
  }
};

export default authService; 