import React from 'react';
import { Toast as BootstrapToast } from 'react-bootstrap';
import { Toast as ToastType } from '../../hooks/useToast';
import './ToastContainer.css';

/**
 * Toast container props
 */
interface ToastContainerProps {
  /** Array of toast notifications */
  toasts: ToastType[];
  /** Function to remove a toast */
  onRemove: (id: number) => void;
  /** Position of the toast container */
  position?: 'top-right' | 'top-left' | 'bottom-right' | 'bottom-left';
}

/**
 * Toast container component
 * Displays multiple toast notifications
 */
const ToastContainer: React.FC<ToastContainerProps> = ({
  toasts,
  onRemove,
  position = 'top-right'
}) => {
  if (toasts.length === 0) {
    return null;
  }

  return (
    <div className={`toast-container ${position}`}>
      {toasts.map((toast) => (
        <BootstrapToast
          key={toast.id}
          onClose={() => onRemove(toast.id)}
          className={`custom-toast bg-${toast.type}`}
          delay={toast.duration}
          autohide={toast.duration !== undefined && toast.duration > 0}
        >
          <BootstrapToast.Header closeButton>
            <strong className="me-auto">
              {toast.type === 'success' && <i className="bi bi-check-circle-fill me-2"></i>}
              {toast.type === 'danger' && <i className="bi bi-exclamation-circle-fill me-2"></i>}
              {toast.type === 'warning' && <i className="bi bi-exclamation-triangle-fill me-2"></i>}
              {toast.type === 'info' && <i className="bi bi-info-circle-fill me-2"></i>}
              {toast.type === 'success' && 'Thành công'}
              {toast.type === 'danger' && 'Lỗi'}
              {toast.type === 'warning' && 'Cảnh báo'}
              {toast.type === 'info' && 'Thông tin'}
            </strong>
          </BootstrapToast.Header>
          <BootstrapToast.Body>{toast.message}</BootstrapToast.Body>
        </BootstrapToast>
      ))}
    </div>
  );
};

export default ToastContainer; 