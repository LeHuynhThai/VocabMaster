import React from 'react';
import { Toast as ToastType } from '../../types';
import './ToastContainer.css';

interface ToastProps {
  toast: ToastType;
  onClose: (id: string) => void;
}

const Toast: React.FC<ToastProps> = ({ toast, onClose }) => {
  return (
    <div className={`toast-item toast-${toast.type} ${toast.isExiting ? 'toast-exiting' : ''}`}>
      <div className="toast-content">
        <span className="toast-message">{toast.message}</span>
      </div>
      <button className="toast-close" onClick={() => onClose(toast.id)}>
        <i className="bi bi-x"></i>
      </button>
    </div>
  );
};

interface ToastContainerProps {
  toasts: ToastType[];
  removeToast: (id: string) => void;
}

const ToastContainer: React.FC<ToastContainerProps> = ({ toasts, removeToast }) => {
  if (toasts.length === 0) return null;

  return (
    <div className="toast-container">
      {toasts.map((toast) => (
        <Toast key={toast.id} toast={toast} onClose={removeToast} />
      ))}
    </div>
  );
};

export default ToastContainer; 