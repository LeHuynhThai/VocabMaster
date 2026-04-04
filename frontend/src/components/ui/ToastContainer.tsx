import React from 'react';
import { Toast as ToastType } from '../../types';

interface ToastProps {
  toast: ToastType;
  onClose: (id: string) => void;
}

const Toast: React.FC<ToastProps> = ({ toast, onClose }) => {
  const toneClasses = {
    success: 'border-emerald-500 bg-emerald-50 text-emerald-900',
    error: 'border-rose-500 bg-rose-50 text-rose-900',
    info: 'border-sky-500 bg-sky-50 text-sky-900',
    warning: 'border-amber-500 bg-amber-50 text-amber-900',
  };

  const iconClasses = {
    success: 'bi bi-check-circle-fill text-emerald-600',
    error: 'bi bi-exclamation-circle-fill text-rose-600',
    info: 'bi bi-info-circle-fill text-sky-600',
    warning: 'bi bi-exclamation-triangle-fill text-amber-600',
  };

  return (
    <div
      className={[
        'pointer-events-auto flex min-w-[250px] items-start justify-between gap-3 overflow-hidden rounded-lg border-l-4 px-4 py-3 shadow-lg transition-all duration-300',
        toneClasses[toast.type],
        toast.isExiting ? 'max-h-0 translate-x-full py-0 opacity-0' : 'max-h-[200px] translate-x-0 opacity-100',
      ].join(' ')}
    >
      <div className="flex flex-1 items-start gap-3">
        <i className={`${iconClasses[toast.type]} mt-0.5 text-lg`}></i>
        <span className="text-[0.95rem]">{toast.message}</span>
      </div>
      <button
        className="ml-2 shrink-0 bg-transparent p-0 text-inherit opacity-70 transition hover:opacity-100"
        onClick={() => onClose(toast.id)}
        type="button"
        aria-label="Đóng thông báo"
      >
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
    <div className="pointer-events-none fixed right-4 top-24 z-[1060] flex w-[calc(100%-2rem)] max-w-[350px] flex-col gap-3">
      {toasts.map((toast) => (
        <Toast key={toast.id} toast={toast} onClose={removeToast} />
      ))}
    </div>
  );
};

export default ToastContainer; 