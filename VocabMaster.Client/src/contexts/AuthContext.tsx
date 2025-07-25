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

// save user state to localStorage
const saveUserState = (user: User | null) => {
  if (user) {
    localStorage.setItem('user', JSON.stringify(user));
  } else {
    localStorage.removeItem('user');
  }
};

// read user state from localStorage
const getUserState = (): User | null => {
  const userStr = localStorage.getItem('user');
  if (userStr) {
    try {
      return JSON.parse(userStr);
    } catch {
      return null;
    }
  }
  return null;
};

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  // initialize with value from localStorage to avoid UI flickering
  const [user, setUser] = useState<User | null>(getUserState());
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [authError, setAuthError] = useState<boolean>(false);
  const [lastAuthCheck, setLastAuthCheck] = useState<number>(0);

  // update user state in localStorage when changed
  useEffect(() => {
    saveUserState(user);
  }, [user]);

  useEffect(() => {
    const checkAuth = async () => {
      // if there is an auth error or a recent check, do not call API continuously
      const now = Date.now();
      if (authError || (now - lastAuthCheck < 60000)) { // 60 seconds
        return;
      }
      
      // update last auth check time
      setLastAuthCheck(now);
      
      // if localStorage does not have auth state or is not authenticated, do not call API
      if (!getAuthState()) {
        if (user !== null) {
          setUser(null);
        }
        return;
      }
      
      // only set loading state when actually checking auth
      setIsLoading(true);
      
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

    checkAuth();
    
    // set interval to check auth periodically
    const intervalId = setInterval(checkAuth, 300000); // 5 minutes
    
    return () => clearInterval(intervalId);
  }, [authError, lastAuthCheck, user]);

  const login = async (credentials: LoginRequest) => {
    setIsLoading(true);
    try {
      const loggedInUser = await authService.login(credentials);
      setUser(loggedInUser);
      // reset auth error when login is successful
      setAuthError(false);
      // save auth state
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