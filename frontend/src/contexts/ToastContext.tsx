import React, { createContext, useCallback, useContext, useRef, useState, ReactNode } from 'react';
import { v4 as uuidv4 } from 'uuid';
import ToastContainer from '../components/ui/ToastContainer';
import { Toast } from '../types';

interface ToastContextType {
  toasts: Toast[];
  addToast: (toast: Omit<Toast, 'id'>) => void;
  removeToast: (id: string) => void;
}

const ToastContext = createContext<ToastContextType | undefined>(undefined);

interface ToastProviderProps {
  children: ReactNode;
}

const MAX_TOASTS = 3;

export const ToastProvider: React.FC<ToastProviderProps> = ({ children }) => {
  const [toasts, setToasts] = useState<Toast[]>([]);
  const toastTimeoutsRef = useRef<Record<string, ReturnType<typeof setTimeout>>>({});
  const lastToastMessageRef = useRef('');
  const lastToastTimeRef = useRef(0);

  const removeToast = useCallback((id: string) => {
    if (toastTimeoutsRef.current[id]) {
      clearTimeout(toastTimeoutsRef.current[id]);
      delete toastTimeoutsRef.current[id];
    }

    setToasts((prevToasts) =>
      prevToasts.map((toast) =>
        toast.id === id ? { ...toast, isExiting: true } : toast
      )
    );

    setTimeout(() => {
      setToasts((prevToasts) => prevToasts.filter((toast) => toast.id !== id));
    }, 300);
  }, []);

  const addToast = useCallback((toast: Omit<Toast, 'id'>) => {
    const now = Date.now();
    if (toast.message === lastToastMessageRef.current && now - lastToastTimeRef.current < 3000) {
      return;
    }

    lastToastMessageRef.current = toast.message;
    lastToastTimeRef.current = now;

    const id = uuidv4();
    const newToast: Toast = {
      ...toast,
      id,
      duration: toast.duration || 5000,
      isExiting: false,
    };

    setToasts((prevToasts) => {
      if (prevToasts.length >= MAX_TOASTS) {
        const oldestToast = prevToasts[0];
        if (oldestToast && !oldestToast.isExiting) {
          removeToast(oldestToast.id);
        }
      }

      return [...prevToasts, newToast];
    });

    const timeout = setTimeout(() => {
      removeToast(id);
    }, newToast.duration);

    toastTimeoutsRef.current[id] = timeout;
  }, [removeToast]);

  return (
    <ToastContext.Provider value={{ toasts, addToast, removeToast }}>
      {children}
      <ToastContainer toasts={toasts} removeToast={removeToast} />
    </ToastContext.Provider>
  );
};

export const useToast = (): ToastContextType => {
  const context = useContext(ToastContext);
  if (context === undefined) {
    throw new Error('useToast must be used within a ToastProvider');
  }
  return context;
};