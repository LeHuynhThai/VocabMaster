import React, { useState, useEffect } from 'react';
import { Toast as BootstrapToast, ToastContainer } from 'react-bootstrap';
import './Toast.css';

/**
 * Toast notification types
 */
export type ToastType = 'success' | 'danger' | 'warning' | 'info';

/**
 * Toast notification props
 */
interface ToastProps {
  show: boolean;
  type: ToastType;
  message: string;
  onClose: () => void;
  autoHideDelay?: number;
}

/**
 * Toast notification component
 * Displays temporary notifications to the user
 */
const Toast: React.FC<ToastProps> = ({
  show,
  type,
  message,
  onClose,
  autoHideDelay = 3000,
}) => {
  const [isVisible, setIsVisible] = useState(show);

  // Update visibility when show prop changes
  useEffect(() => {
    setIsVisible(show);
  }, [show]);

  /**
   * Get appropriate icon based on toast type
   */
  const getIcon = () => {
    switch (type) {
      case 'success':
        return 'bi-check-circle-fill';
      case 'danger':
        return 'bi-x-circle-fill';
      case 'warning':
        return 'bi-exclamation-triangle-fill';
      case 'info':
        return 'bi-info-circle-fill';
      default:
        return 'bi-bell-fill';
    }
  };

  /**
   * Handle toast close event
   */
  const handleClose = () => {
    setIsVisible(false);
    onClose();
  };

  return (
    <ToastContainer position="top-end" className="p-3">
      <BootstrapToast
        show={isVisible}
        onClose={handleClose}
        delay={autoHideDelay}
        autohide
        className={`toast-${type}`}
      >
        <BootstrapToast.Header closeButton>
          <i className={`bi ${getIcon()} me-2`}></i>
          <strong className="me-auto">
            {type === 'success' && 'Thành công'}
            {type === 'danger' && 'Lỗi'}
            {type === 'warning' && 'Cảnh báo'}
            {type === 'info' && 'Thông báo'}
          </strong>
        </BootstrapToast.Header>
        <BootstrapToast.Body>{message}</BootstrapToast.Body>
      </BootstrapToast>
    </ToastContainer>
  );
};

export default Toast; 