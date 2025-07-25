import React, { createContext, useState, useContext, useEffect, ReactNode } from 'react';
import authService from '../services/authService';
import { User, LoginRequest, RegisterRequest } from '../types';

interface AuthContextType {
  user: User | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: (credentials: LoginRequest) => Promise<void>;
  register: (userData: RegisterRequest) => Promise<boolean>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | null>(null);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [authError, setAuthError] = useState<boolean>(false);

  useEffect(() => {
    const loadUser = async () => {
      // Nếu đã có lỗi xác thực, không gọi API liên tục
      if (authError) {
        setIsLoading(false);
        return;
      }
      
      try {
        const currentUser = await authService.getCurrentUser();
        setUser(currentUser);
      } catch (error) {
        console.error('Failed to load user:', error);
        // Đánh dấu là đã có lỗi xác thực để không gọi API liên tục
        setAuthError(true);
      } finally {
        setIsLoading(false);
      }
    };

    loadUser();
  }, [authError]);

  const login = async (credentials: LoginRequest) => {
    setIsLoading(true);
    try {
      const loggedInUser = await authService.login(credentials);
      setUser(loggedInUser);
      // Reset lỗi xác thực khi đăng nhập thành công
      setAuthError(false);
    } catch (error) {
      console.error('Login failed:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const register = async (userData: RegisterRequest) => {
    setIsLoading(true);
    try {
      const success = await authService.register(userData);
      return success;
    } catch (error) {
      console.error('Registration failed:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const logout = async () => {
    setIsLoading(true);
    try {
      await authService.logout();
      setUser(null);
    } catch (error) {
      console.error('Logout failed:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const value = {
    user,
    isLoading,
    isAuthenticated: !!user,
    login,
    register,
    logout
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export default AuthContext; 