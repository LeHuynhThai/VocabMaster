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

// save auth state to localStorage
const saveAuthState = (isAuthenticated: boolean) => {
  localStorage.setItem('isAuthenticated', isAuthenticated ? 'true' : 'false');
};

// read auth state from localStorage
const getAuthState = (): boolean => {
  return localStorage.getItem('isAuthenticated') === 'true';
};

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [authError, setAuthError] = useState<boolean>(false);
  const [lastAuthCheck, setLastAuthCheck] = useState<number>(0);

  useEffect(() => {
    const loadUser = async () => {
      // if there is an auth error or a recent check, do not call API continuously
      const now = Date.now();
      if (authError || (now - lastAuthCheck < 60000)) { // 60 seconds
        setIsLoading(false);
        return;
      }
      
      // update last auth check time
      setLastAuthCheck(now);
      
      // if localStorage does not have auth state or is not authenticated, do not call API
      if (!getAuthState()) {
        setUser(null);
        setIsLoading(false);
        return;
      }
      
      try {
        const currentUser = await authService.getCurrentUser();
        if (currentUser) {
          setUser(currentUser);
          saveAuthState(true);
        } else {
          setUser(null);
          saveAuthState(false);
        }
      } catch (error) {
        console.error('Failed to load user:', error);
        setUser(null);
        saveAuthState(false);
        // mark auth error to avoid calling API continuously
        setAuthError(true);
      } finally {
        setIsLoading(false);
      }
    };

    loadUser();
  }, [authError, lastAuthCheck]);

  const login = async (credentials: LoginRequest) => {
    setIsLoading(true);
    try {
      const loggedInUser = await authService.login(credentials);
      setUser(loggedInUser);
      // Đặt lại lỗi xác thực khi đăng nhập thành công
      setAuthError(false);
      // Lưu trạng thái đăng nhập
      saveAuthState(true);
    } catch (error) {
      console.error('Login failed:', error);
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
      // clear auth state
      saveAuthState(false);
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