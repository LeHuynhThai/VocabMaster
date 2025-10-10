import React, { useState, useEffect } from 'react';
import { Toast as BootstrapToast } from 'react-bootstrap';
import './Toast.css';

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
        return <i className="bi bi-check-circle-fill me-2"></i>;
      case 'danger':
        return <i className="bi bi-exclamation-circle-fill me-2"></i>;
      case 'warning':
        return <i className="bi bi-exclamation-triangle-fill me-2"></i>;
      case 'info':
      default:
        return <i className="bi bi-info-circle-fill me-2"></i>;
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

  return (
    <BootstrapToast
      show={visible}
      onClose={() => {
        setVisible(false);
        onClose();
      }}
      className={`custom-toast toast-${type}`}
      delay={duration}
      autohide={duration > 0}
    >
      <BootstrapToast.Header closeButton>
        <strong className="me-auto">
          {getIcon()}
          {getTitle()}
        </strong>
      </BootstrapToast.Header>
      <BootstrapToast.Body>{message}</BootstrapToast.Body>
    </BootstrapToast>
  );
};

export default Toast; 