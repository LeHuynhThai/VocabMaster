import React, { createContext, useContext, useState, ReactNode, useRef, useCallback } from 'react';
import { v4 as uuidv4 } from 'uuid';
import { Toast } from '../types';
import ToastContainer from '../components/ui/ToastContainer';

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
  const toastTimeoutsRef = useRef<Record<string, NodeJS.Timeout>>({});
  const lastToastMessageRef = useRef<string>('');
  const lastToastTimeRef = useRef<number>(0);

  const removeToast = useCallback((id: string) => {
    // delete timeout if it exists
    if (toastTimeoutsRef.current[id]) {
      clearTimeout(toastTimeoutsRef.current[id]);
      delete toastTimeoutsRef.current[id];
    }

    // mark toast as exiting to add fade-out effect
    setToasts(prevToasts => 
      prevToasts.map(toast => 
        toast.id === id ? { ...toast, isExiting: true } : toast
      )
    );
    
    // wait for fade-out effect to complete before removing toast from state
    setTimeout(() => {
      setToasts(prevToasts => prevToasts.filter(toast => toast.id !== id));
    }, 300); // fade-out duration
  }, []);

  const addToast = useCallback((toast: Omit<Toast, 'id'>) => {
    // check if toast is duplicate in a short time
    const now = Date.now();
    if (
      toast.message === lastToastMessageRef.current && 
      now - lastToastTimeRef.current < 3000 // 3 giây
    ) {
      return; // do not show duplicate toast
    }

    // update last toast information
    lastToastMessageRef.current = toast.message;
    lastToastTimeRef.current = now;

    const id = uuidv4();
    const newToast: Toast = {
      ...toast,
      id,
      duration: toast.duration || 5000, // default duration is 5 seconds
      isExiting: false
    };

    // add new toast and limit number of toasts
    setToasts(prevToasts => {
      // if limit is reached, delete oldest toast
      if (prevToasts.length >= MAX_TOASTS) {
        const oldestToast = prevToasts[0];
        if (oldestToast && !oldestToast.isExiting) {
          removeToast(oldestToast.id);
        }
      }
      return [...prevToasts, newToast];
    });

    // automatically delete toast after duration
    const timeout = setTimeout(() => {
      removeToast(id);
    }, newToast.duration);

    // save timeout to be able to delete if needed
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