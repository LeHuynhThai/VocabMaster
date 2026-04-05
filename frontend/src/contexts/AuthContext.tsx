import React, { createContext, useState, useContext, useEffect, ReactNode } from 'react';
import authService from '../services/authService';
import { logger } from '../utils/logger';
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
  const [isLoading, setIsLoading] = useState<boolean>(true);

  // Kiểm tra xem có token trong localStorage không
  const checkAuthToken = () => {
    const token = localStorage.getItem('vocabmaster_token');
    return !!token;
  };

  // Lưu trạng thái đăng nhập
  const saveAuthState = (isAuthenticated: boolean) => {
    localStorage.setItem('isAuthenticated', isAuthenticated ? 'true' : 'false');
  };

  // Kiểm tra trạng thái đăng nhập khi khởi động ứng dụng
  useEffect(() => {
    const initAuth = async () => {
      setIsLoading(true);
      try {
        // Nếu có token, thử lấy thông tin người dùng hiện tại
        if (checkAuthToken()) {
          const currentUser = await authService.getCurrentUser();
          if (currentUser) {
            setUser(currentUser);
            saveAuthState(true);
          } else {
            // Nếu không lấy được thông tin người dùng, thử refresh token
            const refreshSuccess = await authService.refreshToken();
            if (refreshSuccess) {
              const refreshedUser = await authService.getCurrentUser();
              if (refreshedUser) {
                setUser(refreshedUser);
                saveAuthState(true);
              } else {
                saveAuthState(false);
              }
            } else {
              saveAuthState(false);
            }
          }
        } else {
          saveAuthState(false);
        }
      } catch (error) {
        logger.error('Auth initialization error', error);
        saveAuthState(false);
      } finally {
        setIsLoading(false);
      }
    };

    initAuth();
  }, []);

  const login = async (credentials: LoginRequest) => {
    setIsLoading(true);
    try {
      const loggedInUser = await authService.login(credentials);
      setUser(loggedInUser);
      // save auth state
      saveAuthState(true);
    } catch (error) {
      logger.error('Login failed', error);
      saveAuthState(false);
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
      logger.error('Registration failed', error);
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
      // clear auth state
      saveAuthState(false);
    } catch (error) {
      logger.error('Logout failed', error);
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