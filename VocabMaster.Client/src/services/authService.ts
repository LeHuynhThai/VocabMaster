import api from './api';
import { LoginRequest, RegisterRequest, User } from '../types';

const authService = {
  login: async (credentials: LoginRequest): Promise<User> => {
    const response = await api.post('/Account/Login', credentials);
    return response.data;
  },

  register: async (userData: RegisterRequest): Promise<boolean> => {
    const response = await api.post('/Account/Register', userData);
    return response.status === 200;
  },

  logout: async (): Promise<void> => {
    await api.get('/Account/Logout');
  },

  getCurrentUser: async (): Promise<User | null> => {
    try {
      const response = await api.get('/Account/GetCurrentUser');
      return response.data;
    } catch (error) {
      return null;
    }
  },

  isAuthenticated: async (): Promise<boolean> => {
    const user = await authService.getCurrentUser();
    return user !== null;
  }
};

export default authService; 