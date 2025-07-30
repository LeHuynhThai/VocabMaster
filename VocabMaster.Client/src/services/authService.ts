import api, { removeToken } from './api';

const authService = {
  login: async (credentials: LoginRequest): Promise<User> => {
    try {
      const response = await api.post<TokenResponse>('/api/account/login', credentials);
      // Token đã được lưu tự động trong api interceptor
      // Trả về thông tin user từ token response
      return {
        id: response.data.userId,
        name: response.data.userName,
        role: response.data.role
      };
    } catch (error: any) {
      console.error('Login API error:', error.response?.data || error.message);
      throw error;
    }
  },

  googleLogin: async (googleAuth: GoogleAuthRequest): Promise<User> => {
    try {
      const response = await api.post<TokenResponse>('/api/account/google-login', googleAuth);
      // Token đã được lưu tự động trong api interceptor
      // Trả về thông tin user từ token response
      return {
        id: response.data.userId,
        name: response.data.userName,
        role: response.data.role
      };
    } catch (error: any) {
      console.error('Google Login API error:', error.response?.data || error.message);
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
      // Với JWT, chỉ cần xóa token ở client
      removeToken();
    } catch (error: any) {
      console.error('Logout error:', error.message);
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

  refreshToken: async (): Promise<boolean> => {
    try {
      const response = await api.get<TokenResponse>('/api/account/refresh-token');
      // Token đã được lưu tự động trong api interceptor
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