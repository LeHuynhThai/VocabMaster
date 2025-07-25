import api from './api';
import { LoginRequest, RegisterRequest, User } from '../types';

const authService = {
  login: async (credentials: LoginRequest): Promise<User> => {
    try {
      const response = await api.post('/api/account/login', credentials);
      return response.data;
    } catch (error: any) {
      console.error('Login API error:', error.response?.data || error.message);
      throw error;
    }
  },

  register: async (userData: RegisterRequest): Promise<boolean> => {
    try {
      const response = await api.post('/api/account/register', userData);
      return response.status === 200;
    } catch (error: any) {
      console.error('Register API error:', error.response?.data || error.message);
      throw error;
    }
  },

  logout: async (): Promise<void> => {
    try {
      await api.get('/api/account/logout');
    } catch (error: any) {
      console.error('Logout API error:', error.response?.data || error.message);
      throw error;
    }
  },

  getCurrentUser: async (): Promise<User | null> => {
    try {
      const response = await api.get('/api/account/currentuser');
      return response.data;
    } catch (error: any) {
      // Nếu lỗi 401 (Unauthorized), không log lỗi vì đây là trường hợp bình thường khi chưa đăng nhập
      if (error.response?.status !== 401) {
        console.error('Get current user API error:', error.response?.data || error.message);
      }
      return null;
    }
  },

  isAuthenticated: async (): Promise<boolean> => {
    const user = await authService.getCurrentUser();
    return user !== null;
  }
};

export default authService; 