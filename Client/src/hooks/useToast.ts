import { useState } from 'react';

/**
 * Toast notification type
 */
export type ToastType = 'success' | 'danger' | 'warning' | 'info';

/**
 * Toast notification interface
 */
export interface Toast {
  id: number;
  message: string;
  type: ToastType;
  duration?: number;
}

/**
 * Custom hook for managing toast notifications
 * @returns Toast management functions and state
 */
const useToast = () => {
  const [toasts, setToasts] = useState<Toast[]>([]);

  /**
   * Add a new toast notification
   * @param message - Toast message
   * @param type - Toast type (success, danger, warning, info)
   * @param duration - Display duration in milliseconds (default: 3000ms)
   */
  const showToast = (message: string, type: ToastType = 'info', duration: number = 3000) => {
    const id = Date.now();
    const newToast: Toast = { id, message, type, duration };
    
    setToasts(prev => [...prev, newToast]);
    
    // Auto-remove toast after duration
    if (duration > 0) {
      setTimeout(() => {
        removeToast(id);
      }, duration);
    }
  };

  /**
   * Remove a toast by id
   * @param id - Toast id to remove
   */
  const removeToast = (id: number) => {
    setToasts(prev => prev.filter(toast => toast.id !== id));
  };

  /**
   * Shorthand for success toast
   * @param message - Success message
   * @param duration - Display duration
   */
  const showSuccess = (message: string, duration?: number) => {
    showToast(message, 'success', duration);
  };

  /**
   * Shorthand for error toast
   * @param message - Error message
   * @param duration - Display duration
   */
  const showError = (message: string, duration?: number) => {
    showToast(message, 'danger', duration);
  };

  /**
   * Shorthand for warning toast
   * @param message - Warning message
   * @param duration - Display duration
   */
  const showWarning = (message: string, duration?: number) => {
    showToast(message, 'warning', duration);
  };

  /**
   * Shorthand for info toast
   * @param message - Info message
   * @param duration - Display duration
   */
  const showInfo = (message: string, duration?: number) => {
    showToast(message, 'info', duration);
  };

  return {
    toasts,
    showToast,
    removeToast,
    showSuccess,
    showError,
    showWarning,
    showInfo
  };
};

export default useToast; 