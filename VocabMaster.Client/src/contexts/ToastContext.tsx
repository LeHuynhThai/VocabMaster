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

// Giới hạn số lượng toast hiển thị cùng lúc
const MAX_TOASTS = 3;

export const ToastProvider: React.FC<ToastProviderProps> = ({ children }) => {
  const [toasts, setToasts] = useState<Toast[]>([]);
  const toastTimeoutsRef = useRef<Record<string, NodeJS.Timeout>>({});
  const lastToastMessageRef = useRef<string>('');
  const lastToastTimeRef = useRef<number>(0);

  const removeToast = useCallback((id: string) => {
    // Xóa timeout nếu có
    if (toastTimeoutsRef.current[id]) {
      clearTimeout(toastTimeoutsRef.current[id]);
      delete toastTimeoutsRef.current[id];
    }

    // Đánh dấu toast đang thoát để thêm hiệu ứng fade-out
    setToasts(prevToasts => 
      prevToasts.map(toast => 
        toast.id === id ? { ...toast, isExiting: true } : toast
      )
    );
    
    // Đợi hiệu ứng hoàn thành rồi mới xóa toast khỏi state
    setTimeout(() => {
      setToasts(prevToasts => prevToasts.filter(toast => toast.id !== id));
    }, 300); // Thời gian hiệu ứng fade-out
  }, []);

  const addToast = useCallback((toast: Omit<Toast, 'id'>) => {
    // Kiểm tra nếu toast trùng lặp trong khoảng thời gian ngắn
    const now = Date.now();
    if (
      toast.message === lastToastMessageRef.current && 
      now - lastToastTimeRef.current < 3000 // 3 giây
    ) {
      return; // Không hiển thị toast trùng lặp
    }

    // Cập nhật thông tin toast cuối cùng
    lastToastMessageRef.current = toast.message;
    lastToastTimeRef.current = now;

    const id = uuidv4();
    const newToast: Toast = {
      ...toast,
      id,
      duration: toast.duration || 5000, // Thời gian mặc định là 5 giây
      isExiting: false
    };

    // Thêm toast mới và giới hạn số lượng
    setToasts(prevToasts => {
      // Nếu đã đạt giới hạn, xóa toast cũ nhất
      if (prevToasts.length >= MAX_TOASTS) {
        const oldestToast = prevToasts[0];
        if (oldestToast && !oldestToast.isExiting) {
          removeToast(oldestToast.id);
        }
      }
      return [...prevToasts, newToast];
    });

    // Tự động xóa toast sau khoảng thời gian
    const timeout = setTimeout(() => {
      removeToast(id);
    }, newToast.duration);

    // Lưu timeout để có thể xóa nếu cần
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