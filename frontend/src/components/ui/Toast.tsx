import React, { useState, useEffect } from 'react';

/**
 * Toast component props
 */
export interface ToastProps {
  /** Toast message */
  message: string;
  /** Toast type */
  type?: 'success' | 'danger' | 'warning' | 'info';
  /** Auto hide duration in milliseconds */
  duration?: number;
  /** Whether the toast is visible */
  show: boolean;
  /** Callback when toast is closed */
  onClose: () => void;
}

/**
 * Toast notification component
 * Displays a single toast notification
 */
const Toast: React.FC<ToastProps> = ({
  message,
  type = 'info',
  duration = 3000,
  show,
  onClose
}) => {
  const [visible, setVisible] = useState(show);

  const toneClasses = {
    success: 'border-emerald-500 bg-emerald-50 text-emerald-900',
    danger: 'border-rose-500 bg-rose-50 text-rose-900',
    warning: 'border-amber-500 bg-amber-50 text-amber-900',
    info: 'border-sky-500 bg-sky-50 text-sky-900',
  };

  const iconClasses = {
    success: 'bi bi-check-circle-fill text-emerald-600',
    danger: 'bi bi-exclamation-circle-fill text-rose-600',
    warning: 'bi bi-exclamation-triangle-fill text-amber-600',
    info: 'bi bi-info-circle-fill text-sky-600',
  };

  // Handle show prop changes
  useEffect(() => {
    setVisible(show);
  }, [show]);

  // Auto hide toast after duration
  useEffect(() => {
    if (visible && duration > 0) {
      const timer = setTimeout(() => {
        setVisible(false);
        onClose();
      }, duration);

      return () => clearTimeout(timer);
    }
  }, [visible, duration, onClose]);

  // Get icon based on toast type
  const getIcon = () => {
    switch (type) {
      case 'success':
        return <i className={`${iconClasses.success} mr-2`}></i>;
      case 'danger':
        return <i className={`${iconClasses.danger} mr-2`}></i>;
      case 'warning':
        return <i className={`${iconClasses.warning} mr-2`}></i>;
      case 'info':
      default:
        return <i className={`${iconClasses.info} mr-2`}></i>;
    }
  };

  // Get title based on toast type
  const getTitle = () => {
    switch (type) {
      case 'success':
        return 'Thành công';
      case 'danger':
        return 'Lỗi';
      case 'warning':
        return 'Cảnh báo';
      case 'info':
      default:
        return 'Thông tin';
    }
  };

  if (!visible) {
    return null;
  }

  return (
    <div
      className={[
        'min-w-[250px] rounded-lg border-l-4 px-4 py-3 shadow-lg',
        toneClasses[type],
      ].join(' ')}
      role="alert"
    >
      <div className="mb-2 flex items-start justify-between gap-4">
        <strong className="flex items-center text-sm font-semibold">
          {getIcon()}
          {getTitle()}
        </strong>
        <button
          type="button"
          className="text-sm opacity-60 transition hover:opacity-100"
          onClick={() => {
            setVisible(false);
            onClose();
          }}
          aria-label="Đóng thông báo"
        >
          <i className="bi bi-x-lg"></i>
        </button>
      </div>
      <p className="m-0 text-sm">{message}</p>
    </div>
  );
};

export default Toast; 